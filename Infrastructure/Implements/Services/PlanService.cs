using Application.DTOs.Generic;
using Application.DTOs.Plan;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Announcement;
using Domain.Enums.Plan;
using Domain.Enums.Transaction;
using FluentValidation;
using Infrastructure.Constants;
using Infrastructure.Utilities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Implements.Services
{
    public class PlanService : GenericService<Plan>, IPlanService
    {
        private readonly IAnnouncementService announcementService;
        private readonly IBackgroundService backgroundService;
        public PlanService(IOptionsSnapshot<AppConfig> configSnapshot,
                           IUnitOfWork uow,
                           ITimeService timeService,
                           IClaimService claimService,
                           ICacheService cacheService,
                           IAnnouncementService announcementService,
                           IBackgroundService backgroundService) : base(configSnapshot,
                                                              uow,
                                                              timeService,
                                                              claimService,
                                                              cacheService)
        {
            this.announcementService = announcementService;
            this.backgroundService = backgroundService;
        }
        #region Get plans
        public IQueryable<Plan> GetPlans(string? searchTerm)
        {
            var source = uow.GetRepo<Plan>().GetAll();
            if (searchTerm == null) return source;
            string unaccent = searchTerm.RemoveDiacritics();
            return unaccent.Length < 5
                ? source.Where(p => EF.Functions.Unaccent(p.Name).Contains(unaccent))
                : source.Where(p => EF.Functions.TrigramsAreSimilar(EF.Functions.Unaccent(p.Name), unaccent));
        }
        public IQueryable<Plan> GetOwnedPlans()
        {
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            return GetAll().Where(p => p.AccountId == accountId);
        }
        public IQueryable<Plan> GetJoinedPlans()
        {
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            return GetAll().Where(p => p.Members.Any(p => p.AccountId == accountId
                                                          && p.Status == MemberStatus.JOINED));
        }
        public IQueryable<Plan> GetInvitations()
        {
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            return GetAll().Where(p => p.Members.Any(m => m.AccountId == accountId
                                                          && ((m.Status == MemberStatus.INVITED && p.Status == PlanStatus.REGISTERING)
                                                             || m.Status == MemberStatus.JOINED)));
        }
        public IQueryable<Plan> GetScannablePlans()
        {
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            return GetAll().Where(p => p.Status == PlanStatus.REGISTERING
                                       && p.JoinMethod == JoinMethod.SCAN
                                       && !p.Members.Any(m => m.AccountId == accountId && m.Status == MemberStatus.BLOCKED));
        }
        public IQueryable<Plan> GetPublishedPlans()
        {
            return GetAll().Where(p => p.IsPublished);
        }
        #endregion
        #region Create plan
        public async Task<Plan> CreatePlanAsync(PlanCreate dto)
        {
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            var plan = dto.Adapt<Plan>();
            plan.AccountId = accountId;
            plan.CreatedAt = timeService.Now;
            var arrivedAt = dto.DepartAt.Add(plan.TravelDuration);
            var arrivalTime = arrivedAt.TimeOfDay;
            var arrivedAtNight = arrivalTime >= ValidationConstants.HALF_EVENING;
            var arrivedAtEvening = !arrivedAtNight && arrivalTime >= ValidationConstants.HALF_AFTERNOON;
            var startAt = arrivedAtNight ? arrivedAt.Date.AddDays(1).Add(GlobalConstants.MORNING_START) : arrivedAt;
            plan.UtcStartAt = startAt.UtcDateTime;
            plan.StartDate = DateOnly.FromDateTime(startAt.Date);
            var dayEqualNight = plan.PeriodCount % 2 == 0;
            var maxDateLength = (int)Math.Ceiling(plan.PeriodCount * 1.0 / 2);
            var isEndAtNoon = (arrivedAtEvening && dayEqualNight) || (!arrivedAtEvening && !dayEqualNight);
            var endAt = startAt.AddDays(maxDateLength - (arrivedAtEvening && dayEqualNight ? 0 : 1)).Date
                               .Add(isEndAtNoon ? GlobalConstants.AFTERNOON_START : GlobalConstants.EVENING_END);
            plan.UtcEndAt = DateTime.SpecifyKind(endAt.Add(-plan.Offset), DateTimeKind.Utc);
            plan.EndDate = DateOnly.FromDateTime(endAt);
            //var cacheKey = string.Format(CacheConstants.PLAN_PROVIDERS_FORMAT, claimService.GetUniqueRequestId());
            //var savedProviders = await cacheService.GetDataAsync<List<Provider>>(cacheKey);
            //if (savedProviders != null) await cacheService.RemoveDataAsync(cacheKey);
            //else savedProviders = await uow.GetRepo<Provider>()
            //                         .GetAll()
            //                         .Where(p => dto.SavedProviderIds.Contains(p.Id) && p.IsActive)
            //                         .ToListAsync();
            plan.SavedProviders = dto.SavedProviderIds.Select(id => new PlanSavedProvider { Plan = plan, ProviderId = id }).ToHashSet();
            var totalPlanBudget = 0m;
            for (int i = 0; i < dto.Schedule.Count; i++)
            {
                for (int j = 0; j < dto.Schedule[i].Count; j++)
                {
                    var ev = dto.Schedule[i][j];
                    if (ev.TempOrder == null) continue;
                    totalPlanBudget += ev.TempOrder.Total;
                }
            }
            foreach (var surcharge in plan.Surcharges)
                totalPlanBudget += surcharge.AlreadyDivided ? surcharge.Amount * plan.MaxMemberCount : surcharge.Amount;
            totalPlanBudget = Math.Ceiling(totalPlanBudget * GlobalConstants.BUDGET_ASSURANCE_RATE);
            plan.GcoinBudgetPerCapita = Math.Ceiling(totalPlanBudget / (plan.MaxMemberCount * GlobalConstants.VND_CONVERT_RATE));
            await uow.GetRepo<Plan>().AddAsync(plan);
            if (await uow.SaveChangesAsync())
            {
                var cancelAt = plan.CreatedAt.AddDays(BackgroundConstants.CANCEL_PLAN_DAYS_AFTER);
                backgroundService.SchedulePlanCancelNotify(plan.Id,
                                                           cancelAt.AddHours(-BackgroundConstants.HOURS_NOTIFY_PLAN_CANCEL));
                backgroundService.SchedulePlanCancel(plan.Id, cancelAt);
                return plan;
            }
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Join plan
        public async Task<Plan> JoinPlanAsync(PlanJoin dto)
        {
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            var plan = await uow.GetRepo<Plan>().FindAsync(dto.PlanId)
                       ?? throw new KeyNotFoundException(AppMessage.ERR_PLAN_NOT_FOUND);
            var member = plan.Members.FirstOrDefault();
            if (member == null)
            {
                member = dto.Adapt<PlanMember>();
                member.AccountId = accountId;
                await uow.GetRepo<PlanMember>().AddAsync(member);
            }
            else dto.Adapt(member);
            member.Status = MemberStatus.JOINED;
            member.Weight = member.Companions == null ? 1 : member.Companions.Count + 1;
            plan.MemberCount += member.Weight;
            var now = timeService.Now;
            member.ModifiedAt = now;
            if (plan.Status == PlanStatus.PENDING) plan.UtcRegCloseAt = now.AddDays(GlobalConstants.PLAN_REG_DATE_DIFF);
            plan.Status = plan.MaxMemberCount == 1 ? PlanStatus.READY : PlanStatus.REGISTERING;
            if (plan.GcoinBudgetPerCapita > 0)
            {
                var account = await uow.GetRepo<Account>().FindAsync(accountId)
                              ?? throw new KeyNotFoundException(AppMessage.ERR_ACCOUNT_NOT_FOUND);
                var gcoinAmount = member.Weight * plan.GcoinBudgetPerCapita;
                var transaction = new Transaction
                {
                    BankTransCode = Guid.NewGuid().ToString(),
                    CreatedAt = now,
                    Gateway = Gateway.INTERNAL,
                    PlanMember = member,
                    AccountId = accountId,
                    Status = TransactionStatus.ACCEPTED,
                    GcoinAmount = gcoinAmount,
                    Type = TransactionType.PLAN_FUND,
                    Description = string.Format(AppMessage.DESC_TRANSACTION_PLAN_JOIN, plan.Name)
                };
                await uow.GetRepo<Transaction>().AddAsync(transaction);
                plan.ActualGcoinBudget += transaction.GcoinAmount;
                plan.DisplayGcoinBudget += transaction.GcoinAmount;
                account.GcoinBalance -= transaction.GcoinAmount;
            }
            Announcement? announcement = null;
            if (plan.MemberCount >= plan.MaxMemberCount && plan.MaxMemberCount > 1)
            {
                announcement = new()
                {
                    Plan = plan,
                    Account = plan.Account,
                    CreatedAt = now,
                    Type = AnnouncementType.PLAN,
                    Title = AppMessage.DESC_NOTIFY_TITLE_PLAN_FULL,
                    Body = string.Format(AppMessage.DESC_NOTIFY_BODY_PLAN_FULL, plan.Name)
                };
                await uow.GetRepo<Announcement>().AddAsync(announcement);
            }
            if (await uow.SaveChangesAsync())
            {
                if (announcement != null && announcement.Account!.DeviceToken != null)
                    await announcementService.PushToDevicesAsync(announcement,
                                                                 announcement.Account.DeviceToken);
                backgroundService.SchedulePlanCancelNotify(plan.Id,
                                                           plan.UtcRegCloseAt!.Value.AddHours(-BackgroundConstants.HOURS_NOTIFY_PLAN_CANCEL));
                backgroundService.SchedulePlanCancelNotify(plan.Id, plan.UtcRegCloseAt!.Value);
                return plan;
            }
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);

        }
        #endregion
        #region Update join method
        public async Task<Plan> UpdateJoinMethodAsync(JoinMethodUpdate dto)
        {
            var plan = await uow.GetRepo<Plan>().FindAsync(dto.PlanId) ?? throw new KeyNotFoundException(AppMessage.ERR_PLAN_NOT_FOUND);
            plan.JoinMethod = dto.JoinMethod;
            if (await uow.SaveChangesAsync()) return plan;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Invite to plan
        public async Task<PlanMember> InviteToPlanAsync(PlanInvite dto)
        {
            var now = timeService.Now;
            var member = dto.Adapt<PlanMember>();
            member.Status = MemberStatus.INVITED;
            member.ModifiedAt = now;
            await uow.GetRepo<PlanMember>().AddAsync(member);
            var plan = await uow.GetRepo<Plan>().FindAsync(dto.PlanId) ?? throw new KeyNotFoundException(AppMessage.ERR_PLAN_NOT_FOUND);
            var avatarUrl = string.IsNullOrEmpty(plan.Account.AvatarPath)
                            ? null : plan.Account.AvatarPath.CreateThumbnailLink();
            var announcement = new Announcement
            {
                AccountId = member.AccountId,
                CreatedAt = now,
                PlanId = member.PlanId,
                Type = AnnouncementType.PLAN,
                Title = AppMessage.DESC_NOTIFY_TITLE_PLAN_INVITE,
                Body = string.Format(AppMessage.DESC_NOTIFY_BODY_PLAN_INVITE, plan.Account.Name, plan.Name),
                ImageUrl = avatarUrl
            };
            await uow.GetRepo<Announcement>().AddAsync(announcement);
            if (await uow.SaveChangesAsync())
            {
                var memberAccount = await uow.GetRepo<Account>().FindAsync(dto.AccountId)
                                    ?? throw new KeyNotFoundException(AppMessage.ERR_ACCOUNT_NOT_FOUND);
                if (memberAccount.DeviceToken != null)
                    await announcementService.PushToDevicesAsync(announcement, memberAccount.DeviceToken);
                return member;
            }
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Remove member
        public async Task<PlanMember> RemoveMemberAsync(MemberRemove dto)
        {
            var now = timeService.Now;
            var member = await uow.GetRepo<PlanMember>().FindAsync(dto.PlanMemberId)
                         ?? throw new KeyNotFoundException(AppMessage.ERR_PLAN_MEMBER_NOT_FOUND);
            Announcement? announcement = null;
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            var gcoinAmount = member.Weight * member.Plan.GcoinBudgetPerCapita;
            if (member.AccountId != accountId)
            {
                var avatarUrl = string.IsNullOrEmpty(member.Plan.Account.AvatarPath)
                                ? null : member.Plan.Account.AvatarPath.CreateThumbnailLink();
                announcement = new Announcement
                {
                    AccountId = member.AccountId,
                    CreatedAt = now,
                    PlanId = member.PlanId,
                    Type = AnnouncementType.PLAN,
                    ImageUrl = avatarUrl

                };
                if (dto.AlsoBlock)
                {
                    member.Status = MemberStatus.BLOCKED;
                    announcement.Level = AnnouncementLevel.WARNING;
                    announcement.Title = AppMessage.DESC_NOTIFY_TITLE_PLAN_BLOCK;
                    announcement.Body = string.Format(AppMessage.DESC_NOTIFY_BODY_PLAN_BLOCK,
                                                      member.Plan.Name,
                                                      member.Plan.Account.Name);
                }
                else
                {
                    member.Status = MemberStatus.REMOVED;
                    announcement.Level = AnnouncementLevel.WARNING;
                    announcement.Title = AppMessage.DESC_NOTIFY_TITLE_PLAN_REMOVE;
                    announcement.Body = string.Format(AppMessage.DESC_NOTIFY_BODY_PLAN_REMOVE,
                                                      member.Plan.Name,
                                                      member.Plan.Account.Name);
                }
                await uow.GetRepo<Announcement>().AddAsync(announcement);
            }
            else
            {
                member.Status = dto.AlsoBlock ? MemberStatus.SELF_BLOCKED : MemberStatus.REMOVED;
                var joinDateDiff = (member.ModifiedAt - member.Plan.UtcRegCloseAt!.Value).TotalDays;
                if (joinDateDiff > 0) gcoinAmount *= appConfig.MEMBER_REFUND_SELF_REMOVE_1_DAY_PCT / 100;
            }
            member.ModifiedAt = now;
            member.Plan.MemberCount -= member.Weight;
            if (gcoinAmount > 0)
            {
                var transaction = new Transaction
                {
                    BankTransCode = Guid.NewGuid().ToString(),
                    CreatedAt = now,
                    Description = AppMessage.DESC_TRANSACTION_PLAN_REFUND,
                    Gateway = Gateway.INTERNAL,
                    PlanMemberId = member.Id,
                    GcoinAmount = gcoinAmount,
                    AccountId = member.AccountId,
                    Status = TransactionStatus.ACCEPTED,
                    Type = TransactionType.PLAN_REFUND
                };
                member.Account.GcoinBalance += transaction.GcoinAmount;
                member.Plan.DisplayGcoinBudget -= transaction.GcoinAmount;
                member.Plan.ActualGcoinBudget -= transaction.GcoinAmount;
                await uow.GetRepo<Transaction>().AddAsync(transaction);
            }

            if (await uow.SaveChangesAsync())
            {
                if (announcement != null && member.Account.DeviceToken != null)
                    await announcementService.PushToDevicesAsync(announcement, member.Account.DeviceToken);
                return member;
            }
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Confirm members
        public async Task<Plan> ConfirmMembersAsync(int planId)
        {
            var plan = await uow.GetRepo<Plan>()
                                .GetAll(true)
                                .Include(p => p.Members.Where(m => m.Status == MemberStatus.JOINED))
                                .ThenInclude(m => m.Account)
                                .FirstOrDefaultAsync(p => p.Id == planId)
                       ?? throw new KeyNotFoundException(AppMessage.ERR_PLAN_NOT_FOUND);
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            if (plan.AccountId != accountId)
                throw new UnauthorizedAccessException(AppMessage.ERR_AUTHORIZE);
            switch (plan.Status)
            {
                case PlanStatus.PENDING:
                    throw new ValidationException(AppMessage.ERR_PLAN_CONFIRM_PENDING);
                case PlanStatus.CANCELED:
                    throw new ValidationException(AppMessage.ERR_PLAN_CONFIRM_CANCELED);
                case var status when status >= PlanStatus.READY:
                    throw new ValidationException(AppMessage.ERR_PLAN_CONFIRM_CONFIRMED);
            }
            var gcoinAmount = plan.GcoinBudgetPerCapita * plan.MaxMemberCount - plan.ActualGcoinBudget;
            var account = await uow.GetRepo<Account>().FindAsync(accountId)
                          ?? throw new KeyNotFoundException(AppMessage.ERR_ACCOUNT_NOT_FOUND);
            if (gcoinAmount > account.GcoinBalance) throw new ValidationException(AppMessage.ERR_BALANCE_NOT_ENOUGH);
            var now = timeService.Now;
            var transaction = new Transaction
            {
                BankTransCode = Guid.NewGuid().ToString(),
                CreatedAt = now,
                Gateway = Gateway.INTERNAL,
                PlanMemberId = plan.Members.First(m => m.AccountId == accountId).Id,
                GcoinAmount = gcoinAmount,
                AccountId = accountId,
                Type = TransactionType.PLAN_FUND,
                Description = string.Format(AppMessage.DESC_TRANSACTION_PLAN_CONFIRM, plan.Name)
            };
            plan.ActualGcoinBudget += transaction.GcoinAmount;
            await uow.GetRepo<Transaction>().AddAsync(transaction);
            plan.Status = PlanStatus.READY;
            List<Announcement> annoucements = [];
            var normalMember = plan.Members.Where(m => m.AccountId != accountId);
            var annoucementBody = string.Format(AppMessage.DESC_NOTIFY_BODY_PLAN_CONFIRM, plan.Name);
            List<string> deviceTokens = [];
            foreach (var member in normalMember)
            {
                annoucements.Add(new Announcement
                {
                    AccountId = member.AccountId,
                    CreatedAt = now,
                    PlanId = planId,
                    Type = AnnouncementType.PLAN,
                    Level = AnnouncementLevel.NONE,
                    Title = AppMessage.DESC_NOTIFY_TITLE_PLAN_CONFIRM,
                    Body = annoucementBody,
                });
                if (member.Account.DeviceToken != null) deviceTokens.Add(member.Account.DeviceToken);
            }
            await uow.GetRepo<Announcement>().AddAsync(annoucements);
            if (await uow.SaveChangesAsync())
            {
                if (deviceTokens.Count > 0)
                    await announcementService.PushToDevicesAsync(annoucements[0], [.. deviceTokens]);
                var startNotifyAt = plan.UtcDepartAt.AddDays(-BackgroundConstants.MAX_DATE_NOTIFY_DEPART);
                for (var date = startNotifyAt; date < plan.UtcDepartAt; date = date.AddDays(1))
                    backgroundService.SchedulePlanDepartNotify(plan.Id, date);
                backgroundService.SchedulePlanVerifyNotify(plan.Id, plan.UtcDepartAt + plan.TravelDuration);
                return plan;
            }
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Cancel plan
        public async Task<Plan> CancelPlanAsync(int planId)
        {
            var plan = await uow.GetRepo<Plan>()
                                .GetAll(true)
                                .Include(p => p.Members.Where(m => m.Status == MemberStatus.JOINED))
                                .ThenInclude(m => m.Account)
                                .FirstOrDefaultAsync(p => p.Id == planId) ?? throw new KeyNotFoundException(AppMessage.ERR_PLAN_NOT_FOUND);
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            if (plan.AccountId != accountId) throw new UnauthorizedAccessException(AppMessage.ERR_AUTHORIZE);
            switch (plan.Status)
            {
                case PlanStatus.CANCELED:
                    throw new ValidationException(AppMessage.ERR_PLAN_CANCEL_CANCELED);
                case var status when status > PlanStatus.REGISTERING:
                    throw new ValidationException(AppMessage.ERR_PLAN_CANCEL_READY);
            }
            plan.Status = PlanStatus.CANCELED;
            if (plan.Status == PlanStatus.PENDING)
            {
                if (await uow.SaveChangesAsync()) return plan;
                throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
            }
            var now = timeService.Now;
            List<string> deviceTokens = [];
            List<Announcement> announcements = [];
            List<Transaction> transactions = [];
            var body = string.Format(AppMessage.DESC_NOTIFY_BODY_PLAN_CANCEL_REGISTERING, plan.Name);
            foreach (var member in plan.Members)
            {
                announcements.Add(new Announcement
                {
                    CreatedAt = now,
                    AccountId = member.AccountId,
                    PlanId = member.PlanId,
                    Type = AnnouncementType.PLAN,
                    Title = AppMessage.DESC_NOTIFY_TITLE_PLAN_CANCEL,
                    Body = body,
                    Level = AnnouncementLevel.ERROR
                });
                if (member.Account.DeviceToken != null) deviceTokens.Add(member.Account.DeviceToken);
                if (plan.ActualGcoinBudget <= 0) continue;
                var refundAmount = plan.GcoinBudgetPerCapita * member.Weight;
                plan.ActualGcoinBudget -= refundAmount;
                plan.DisplayGcoinBudget -= refundAmount;
                member.Account.GcoinBalance += refundAmount;
                transactions.Add(new Transaction
                {
                    BankTransCode = Guid.NewGuid().ToString(),
                    CreatedAt = now,
                    Gateway = Gateway.INTERNAL,
                    GcoinAmount = refundAmount,
                    AccountId = member.AccountId,
                    Status = TransactionStatus.ACCEPTED,
                    Type = TransactionType.PLAN_REFUND,
                    Description = AppMessage.DESC_TRANSACTION_PLAN_REFUND
                });
            }
            await uow.GetRepo<Announcement>().AddAsync(announcements);
            await uow.GetRepo<Transaction>().AddAsync(transactions);
            if (await uow.SaveChangesAsync())
            {
                if (deviceTokens.Count > 0)
                    await announcementService.PushToDevicesAsync(announcements[0], [.. deviceTokens]);
                return plan;
            }
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Verify plan
        public async Task<Plan> VerifyPlanAsync(PlanVerify dto)
        {
            var plan = await uow.GetRepo<Plan>().FindAsync(dto.PlanId)
                       ?? throw new KeyNotFoundException(AppMessage.ERR_PLAN_NOT_FOUND);
            plan.Status = PlanStatus.VERIFIED;
            if (await uow.SaveChangesAsync())
            {
                backgroundService.SchedulePlanComplete(plan.Id, plan.UtcEndAt.Add(-timeService.AdditionalSpan).AddDays(BackgroundConstants.DAYS_FINISH_PLAN));
                return plan;
            }
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Update plan
        public async Task<Plan> UpdatePlanAsync(PlanUpdate dto)
        {
            var plan = await uow.GetRepo<Plan>().FindAsync(dto.PlanId) ?? throw new KeyNotFoundException(AppMessage.ERR_PLAN_NOT_FOUND);
            uow.GetRepo<Surcharge>().Remove(plan.Surcharges);
            uow.GetRepo<PlanSavedProvider>().Remove(plan.SavedProviders);
            dto.Adapt(plan);
            var arrivedAt = dto.DepartAt.Add(plan.TravelDuration);
            var arrivalTime = arrivedAt.TimeOfDay;
            var arrivedAtNight = arrivalTime >= ValidationConstants.HALF_EVENING;
            var arrivedAtEvening = !arrivedAtNight && arrivalTime >= ValidationConstants.HALF_AFTERNOON;
            var startAt = arrivedAtNight ? arrivedAt.Date.AddDays(1).Add(GlobalConstants.MORNING_START) : arrivedAt;
            plan.UtcStartAt = startAt.UtcDateTime;
            plan.StartDate = DateOnly.FromDateTime(startAt.Date);
            var dayEqualNight = plan.PeriodCount % 2 == 0;
            var maxDateLength = (int)Math.Ceiling(plan.PeriodCount * 1.0 / 2);
            var isEndAtNoon = (arrivedAtEvening && dayEqualNight) || (!arrivedAtEvening && !dayEqualNight);
            var endAt = startAt.AddDays(maxDateLength - (arrivedAtEvening && dayEqualNight ? 0 : 1)).Date
                               .Add(isEndAtNoon ? GlobalConstants.AFTERNOON_START : GlobalConstants.EVENING_END);
            plan.UtcEndAt = endAt.Add(-plan.Offset);
            plan.EndDate = DateOnly.FromDateTime(endAt);
            //var cacheKey = string.Format(CacheConstants.PLAN_PROVIDERS_FORMAT, claimService.GetUniqueRequestId());
            //var contacts = await cacheService.GetDataAsync<List<Provider>>(cacheKey);
            //if (contacts != null) await cacheService.RemoveDataAsync(cacheKey);
            //else contacts = await uow.GetRepo<Provider>()
            //                        .GetAll()
            //                        .Where(p => dto.SavedProviderIds.Contains(p.Id) && p.IsActive)
            //                        .ToListAsync();
            //plan.SavedContacts = contacts.Adapt<List<Contact>>();
            plan.SavedProviders = dto.SavedProviderIds.Select(id => new PlanSavedProvider { Plan = plan, ProviderId = id }).ToHashSet();
            var totalPlanBudget = 0m;
            for (int i = 0; i < dto.Schedule.Count; i++)
            {
                for (int j = 0; j < dto.Schedule[i].Count; j++)
                {
                    var ev = dto.Schedule[i][j];
                    if (ev.TempOrder == null) continue;
                    totalPlanBudget += ev.TempOrder.Total;
                }
            }
            foreach (var surcharge in plan.Surcharges)
                totalPlanBudget += surcharge.AlreadyDivided ? surcharge.Amount * plan.MaxMemberCount : surcharge.Amount;
            totalPlanBudget = Math.Ceiling(totalPlanBudget * GlobalConstants.BUDGET_ASSURANCE_RATE);
            plan.GcoinBudgetPerCapita = Math.Ceiling(totalPlanBudget / (plan.MaxMemberCount * GlobalConstants.VND_CONVERT_RATE));
            if (await uow.SaveChangesAsync()) return plan;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Update surcharge
        public async Task<Surcharge> UpdateSurchargeAsync(SurchargeUpdate dto)
        {
            var surcharge = await uow.GetRepo<Surcharge>().FindAsync(dto.SurchargeId) ?? throw new KeyNotFoundException(AppMessage.ERR_SURCHARGE_NOT_FOUND);
            dto.Adapt(surcharge);
            var surchargeGcoin = surcharge.Amount * 1.0m / GlobalConstants.VND_CONVERT_RATE;
            surcharge.Plan.ActualGcoinBudget -= surchargeGcoin;
            surcharge.Plan.DisplayGcoinBudget = surcharge.Plan.DisplayGcoinBudget > surchargeGcoin
                                                ? surcharge.Plan.DisplayGcoinBudget - surchargeGcoin : 0;
            if (await uow.SaveChangesAsync()) return surcharge;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Publish plan
        public async Task<Plan> ChangePlanPublishStatusAsync(int planId)
        {
            var plan = await uow.GetRepo<Plan>().FindAsync(planId)
                       ?? throw new KeyNotFoundException(AppMessage.ERR_PLAN_NOT_FOUND);
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            if (plan.AccountId != accountId)
                throw new UnauthorizedAccessException(AppMessage.ERR_AUTHORIZE);
            if (plan.Status != PlanStatus.COMPLETED)
                throw new ValidationException(AppMessage.ERR_PLAN_PUBLISH_STATUS);
            plan.IsPublished = !plan.IsPublished;
            if (await uow.SaveChangesAsync()) return plan;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);

        }
        #endregion
    }
}
