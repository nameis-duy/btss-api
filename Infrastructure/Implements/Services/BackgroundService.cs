using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Announcement;
using Domain.Enums.Plan;
using Domain.Enums.Provider;
using Domain.Enums.Transaction;
using Domain.JsonEntities;
using Hangfire;
using Infrastructure.Constants;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;

namespace Infrastructure.Implements.Services
{
    public class BackgroundService : IBackgroundService
    {
        private readonly IUnitOfWork uow;
        private readonly IAnnouncementService announcementService;
        private readonly ITimeService timeService;
        private readonly ICacheService cacheService;

        public BackgroundService(IUnitOfWork uow,
                                 IAnnouncementService announcementService,
                                 ITimeService timeService,
                                 ICacheService cacheService)
        {
            this.uow = uow;
            this.announcementService = announcementService;
            this.timeService = timeService;
            this.cacheService = cacheService;
            
        }
        #region Recalculate execution time
        public void RecalculateExecutionTime(TimeSpan additionalSpan)
        {
            var utcNow = timeService.UtcNow;
            var jobKvs = JobStorage.Current.GetMonitoringApi().ScheduledJobs(0, int.MaxValue).Where(kv => kv.Value.EnqueueAt >= utcNow);
            foreach (var kv in jobKvs)
            {
                var newEnqueueAt = kv.Value.EnqueueAt.Add(-additionalSpan);
                BackgroundJob.Reschedule(kv.Key, newEnqueueAt);
            }
        }
        #endregion
        #region Schedule plan cancel notify
        public void SchedulePlanCancelNotify(int planId, DateTime enqueueAt)
        {
            BackgroundJob.Schedule(() => NotifyBeforeCancelPlanAsync(planId), enqueueAt);
        }
        [JobDisplayName($"{nameof(NotifyBeforeCancelPlanAsync)}_{{0}}")]
        public async Task NotifyBeforeCancelPlanAsync(int planId)
        {
            var plan = await uow.GetRepo<Plan>()
                                .GetAll()
                                .Include(p => p.Account)
                                .FirstOrDefaultAsync(p => p.Id == planId && p.Status < PlanStatus.READY);
            if (plan == null) return;
            string body = string.Format(plan.Status == PlanStatus.PENDING
                                        ? AppMessage.DESC_NOTIFY_BODY_BEFORE_PLAN_CANCEL_PENDING
                                        : AppMessage.DESC_NOTIFY_BODY_BEFORE_PLAN_CANCEL_REGISTERING,
                                        plan.Name,
                                        BackgroundConstants.HOURS_NOTIFY_PLAN_CANCEL);
            var announcement = new Announcement
            {
                AccountId = plan.AccountId,
                CreatedAt = timeService.Now,
                PlanId = planId,
                Type = AnnouncementType.PLAN,
                Level = AnnouncementLevel.WARNING,
                Title = AppMessage.DESC_NOTIFY_TITLE_BEFORE_PLAN_CANCEL,
                Body = body
            };
            await uow.GetRepo<Announcement>().AddAsync(announcement);
            if (await uow.SaveChangesAsync() && plan.Account.DeviceToken != null)
                await announcementService.PushToDevicesAsync(announcement, plan.Account.DeviceToken);
        }
        #endregion
        #region Schedule plan cancel
        public void SchedulePlanCancel(int planId, DateTime enqueueAt)
        {
            BackgroundJob.Schedule(() => CancelPlanAsync(planId), enqueueAt);
        }
        [JobDisplayName($"{nameof(CancelPlanAsync)}_{{0}}")]
        public async Task CancelPlanAsync(int planId)
        {
            var plan = await uow.GetRepo<Plan>()
                                .GetAll(true)
                                .Include(p => p.Members.Where(m => m.Status == MemberStatus.JOINED))
                                .ThenInclude(m => m.Account)
                                .Include(p => p.Account)
                                .FirstOrDefaultAsync(p => p.Id == planId && p.Status < PlanStatus.READY);
            if (plan == null) return;
            var now = timeService.Now;
            List<Announcement> announcements = [];
            List<Transaction> transactions = [];
            List<string> deviceTokens = [];
            string body = string.Empty;
            if (plan.Status == PlanStatus.PENDING)
            {
                body = string.Format(AppMessage.DESC_NOTIFY_BODY_PLAN_CANCEL_PENDING, plan.Name);
                announcements.Add(new Announcement
                {
                    AccountId = plan.AccountId,
                    Title = AppMessage.DESC_NOTIFY_TITLE_PLAN_CANCEL,
                    Body = body,
                    CreatedAt = now,
                    PlanId = plan.Id,
                    Type = AnnouncementType.PLAN,
                    Level = AnnouncementLevel.ERROR
                });
                if (plan.Account.DeviceToken != null)
                    deviceTokens.Add(plan.Account.DeviceToken);
            }
            else
            {
                body = string.Format(AppMessage.DESC_NOTIFY_BODY_PLAN_CANCEL_REGISTERING, plan.Name);
                foreach (var member in plan.Members)
                {
                    announcements.Add(new Announcement
                    {
                        AccountId = member.AccountId,
                        Title = AppMessage.DESC_NOTIFY_TITLE_PLAN_CANCEL,
                        Body = body,
                        CreatedAt = now,
                        PlanId = plan.Id,
                        Type = AnnouncementType.PLAN,
                        Level = AnnouncementLevel.ERROR
                    });
                    if (member.Account.DeviceToken != null)
                        deviceTokens.Add(member.Account.DeviceToken);
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
            }
            plan.Status = PlanStatus.CANCELED;
            await uow.GetRepo<Transaction>().AddAsync(transactions);
            await uow.GetRepo<Announcement>().AddAsync(announcements);
            if (await uow.SaveChangesAsync() && deviceTokens.Count > 0)
                await announcementService.PushToDevicesAsync(announcements[0], [.. deviceTokens]);
        }
        #endregion
        #region Schedule plan depart notify
        public void SchedulePlanDepartNotify(int planId, DateTime enqueueAt)
        {
            BackgroundJob.Schedule(() => NotifyPlanDepartAsync(planId), enqueueAt);
        }
        [JobDisplayName($"{nameof(NotifyPlanDepartAsync)}_{{0}}")]
        public async Task NotifyPlanDepartAsync(int planId)
        {
            var now = timeService.Now;
            var plan = await uow.GetRepo<Plan>()
                                .GetAll()
                                .Include(p => p.Members.Where(m => m.Status == MemberStatus.JOINED))
                                .ThenInclude(m => m.Account)
                                .FirstOrDefaultAsync(p => p.Id == planId);
            if (plan == null) return;
            var dateDiff = Math.Round((plan.UtcDepartAt - now).TotalDays);
            var title = dateDiff == 0 ? AppMessage.DESC_NOTIFY_TITLE_PLAN_DEPART_TODAY : AppMessage.DESC_NOTIFY_TITLE_PLAN_DEPART;
            var body = dateDiff == 0
                       ? string.Format(AppMessage.DESC_NOTIFY_BODY_PLAN_DEPART_TODAY,
                                       plan.Name,
                                       $"{plan.UtcDepartAt.Add(plan.Offset):hh\\:mm}")
                       : string.Format(AppMessage.DESC_NOTIFY_BODY_PLAN_DEPART,
                                       dateDiff,
                                       plan.Name);
            List<Announcement> announcements = [];
            List<string> deviceTokens = [];
            foreach (var member in plan.Members)
            {
                announcements.Add(new Announcement
                {
                    AccountId = member.AccountId,
                    CreatedAt = now,
                    PlanId = member.PlanId,
                    Type = AnnouncementType.PLAN,
                    Title = title,
                    Body = body,
                });
                if (member.Account.DeviceToken != null) deviceTokens.Add(member.Account.DeviceToken);
            }
            await uow.GetRepo<Announcement>().AddAsync(announcements);
            if (await uow.SaveChangesAsync() && deviceTokens.Count > 0)
                await announcementService.PushToDevicesAsync(announcements[0], [.. deviceTokens]);
        }
        #endregion
        #region Schedule plan verify notify
        public void SchedulePlanVerifyNotify(int planId, DateTime enqueueAt)
        {
            BackgroundJob.Schedule(() => NotifyPlanVerifyAsync(planId), enqueueAt);
        }
        [JobDisplayName($"{nameof(NotifyPlanVerifyAsync)}_{{0}}")]
        public async Task NotifyPlanVerifyAsync(int planId)
        {
            var now = timeService.Now;
            var plan = await uow.GetRepo<Plan>()
                                .GetAll()
                                .Include(p => p.Account)
                                .Include(p => p.Destination)
                                .FirstOrDefaultAsync(p => p.Id == planId);
            if (plan == null) return;
            var announcement = new Announcement
            {
                AccountId = plan.AccountId,
                CreatedAt = now,
                PlanId = plan.Id,
                Type = AnnouncementType.PLAN,
                Title = AppMessage.DESC_NOTIFY_TITLE_PLAN_VERIFY,
                Body = AppMessage.DESC_NOTIFY_BODY_PLAN_VERIFY,
            };
            await uow.GetRepo<Announcement>().AddAsync(announcement);
            if (await uow.SaveChangesAsync() && plan.Account.DeviceToken != null)
                await announcementService.PushToDevicesAsync(announcement, plan.Account.DeviceToken);
        }
        #endregion
        #region Schedule plan complete
        public void SchedulePlanComplete(int planId, DateTime enqueueAt)
        {
            BackgroundJob.Schedule(() => FinishPlanAsync(planId), enqueueAt);
        }
        [JobDisplayName($"{nameof(FinishPlanAsync)}_{{0}}")]
        public async Task FinishPlanAsync(int planId)
        {
            var now = timeService.Now;
            var plan = await uow.GetRepo<Plan>()
                                .GetAll(true)
                                .Include(p => p.Surcharges.Where(s => s.ImagePath != null))
                                .Include(p => p.Members.Where(m => m.Status == MemberStatus.JOINED))
                                .ThenInclude(m => m.Account)
                                .FirstOrDefaultAsync(p => p.Id == planId);
            if (plan == null) return;
            List<Announcement> announcements = [];
            var host = plan.Members.First(m => m.AccountId == plan.AccountId);
            if (plan.Status == PlanStatus.READY) plan.Status = PlanStatus.FLAWED;
            else
            {
                plan.Status = PlanStatus.COMPLETED;
                host.Account.PrestigePoint += plan.Account.PrestigePoint >= 100
                                              ? 0 : GlobalConstants.PRESTIGE_POINT_EARN_ON_PLAN_COMPLETE;
            }
            string title = AppMessage.DESC_NOTIFY_TITLE_PLAN_FINISH;
            string? body = string.Format(AppMessage.DESC_NOTIFY_BODY_PLAN_FINISH, plan.Name);
            List<Transaction> transactions = [];
            List<string> deviceTokens = [];
            var surchargeAmount = plan.Surcharges.Sum(s => s.Amount) / GlobalConstants.VND_CONVERT_RATE;
            if (surchargeAmount > 0)
            {
                var transaction = new Transaction
                {
                    CreatedAt = now,
                    BankTransCode = Guid.NewGuid().ToString(),
                    AccountId = plan.AccountId,
                    Gateway = Gateway.INTERNAL,
                    GcoinAmount = surchargeAmount,
                    PlanMemberId = host.Id,
                    Status = TransactionStatus.ACCEPTED,
                    Type = TransactionType.PLAN_REFUND,
                    Description = string.Format(AppMessage.DESC_TRANSACTION_PLAN_SURCHARGE, plan.Name)
                };
                transactions.Add(transaction);
                host.Account.GcoinBalance += transaction.GcoinAmount;
            }
            var gcoinPerCapita = plan.DisplayGcoinBudget / plan.MemberCount;
            foreach (var member in plan.Members)
            {
                if (gcoinPerCapita > 0)
                {
                    var gcoinAmount = gcoinPerCapita * member.Weight;
                    plan.ActualGcoinBudget -= gcoinAmount;
                    plan.DisplayGcoinBudget -= gcoinAmount;
                    member.Account.GcoinBalance += gcoinAmount;
                    transactions.Add(new Transaction
                    {
                        CreatedAt = now,
                        BankTransCode = Guid.NewGuid().ToString(),
                        Gateway = Gateway.INTERNAL,
                        GcoinAmount = gcoinAmount,
                        AccountId = member.AccountId,
                        PlanMemberId = member.Id,
                        Type = TransactionType.PLAN_REFUND,
                        Status = TransactionStatus.ACCEPTED,
                        Description = AppMessage.DESC_TRANSACTION_PLAN_REFUND
                    });
                }
                announcements.Add(new Announcement
                {
                    AccountId = member.AccountId,
                    CreatedAt = now,
                    PlanId = member.PlanId,
                    Type = AnnouncementType.PLAN,
                    Title = title,
                    Body = body,
                });
                if (member.Account.DeviceToken != null) deviceTokens.Add(member.Account.DeviceToken);
            }
            var gcoinLeft = Math.Ceiling(plan.ActualGcoinBudget);
            host.Account.GcoinBalance += gcoinLeft;
            plan.ActualGcoinBudget = 0;
            transactions.Add(new Transaction
            {
                CreatedAt = now,
                BankTransCode = Guid.NewGuid().ToString(),
                Gateway = Gateway.INTERNAL,
                GcoinAmount = gcoinLeft,
                AccountId = plan.AccountId,
                PlanMemberId = host.Id,
                Type = TransactionType.PLAN_REFUND,
                Status = TransactionStatus.ACCEPTED,
                Description = AppMessage.DESC_TRANSACTION_PLAN_REFUND
            });
            await uow.GetRepo<Transaction>().AddAsync(transactions);
            await uow.GetRepo<Announcement>().AddAsync(announcements);
            if (await uow.SaveChangesAsync() && deviceTokens.Count > 0)
                await announcementService.PushToDevicesAsync(announcements[0], [.. deviceTokens]);
        }
        #endregion
        #region Recur plan finish
        public void RecurPlanFinish()
        {
            RecurringJob.AddOrUpdate(BackgroundConstants.VERIFY_PLAN_COMPLETE_RECURRING_JOB,
                                     () => SchedulePlanFinishAsync(),
                                     HangfireUtil.DayInterval(BackgroundConstants.DAYS_VERIFY_PLAN_COMPLETE));
        }
        [JobDisplayName($"{nameof(SchedulePlanFinishAsync)}")]
        public async Task SchedulePlanFinishAsync()
        {
            var now = timeService.Now;
            var completeCheckpoint = now.AddDays(-BackgroundConstants.DAYS_FINISH_PLAN);
            var plans = await uow.GetRepo<Plan>()
                                 .GetAll()
                                 .Where(p => (p.Status == PlanStatus.READY)
                                             && p.UtcEndAt <= completeCheckpoint)
                                 .ToListAsync();
            foreach (var plan in plans)
            {
                BackgroundJob.Schedule(() => FinishPlanAsync(plan.Id), TimeSpan.FromMinutes(1));
            }
        }
        #endregion
        #region Schedule order finish
        public void ScheduleOrderFinish(int orderId, DateTime enqueueAt)
        {
            BackgroundJob.Schedule(() => FinishOrderAsync(orderId), enqueueAt);
        }
        [JobDisplayName($"{nameof(FinishOrderAsync)}_{{0}}")]
        public async Task FinishOrderAsync(int orderId)
        {
            var order = await uow.GetRepo<Order>()
                                 .GetAll(true)
                                 .Include(o => o.Provider)
                                 .FirstOrDefaultAsync(o => o.Id == orderId && o.CurrentStatus == OrderStatus.SERVED);
            if (order == null) return;
            var now = timeService.Now;
            var trace = new OrderTrace
            {
                Description = AppMessage.DESC_ORDER_TRACE_FINISHED,
                ModifiedAt = now,
                IsClientAction = true,
                Status = OrderStatus.FINISHED
            };
            order.Traces.Add(trace);
            order.CurrentStatus = trace.Status;
            var gcoinAmount = order.Deposit / GlobalConstants.VND_CONVERT_RATE;
            var transaction = new Transaction
            {
                OrderId = order.Id,
                CreatedAt = now,
                GcoinAmount = gcoinAmount,
                BankTransCode = Guid.NewGuid().ToString(),
                Gateway = Gateway.INTERNAL,
                Type = TransactionType.ORDER,
                ProviderId = order.ProviderId,
                Status = TransactionStatus.ACCEPTED,
                Description = string.Format(AppMessage.DESC_TRANSACTION_ORDER_COMPLETE, $"{order.Id:D9}")
            };
            order.Provider.Balance += order.Deposit;
            var announcement = new Announcement
            {
                CreatedAt = now,
                OrderId = order.Id,
                ProviderId = order.ProviderId,
                Type = AnnouncementType.ORDER,
                Title = AppMessage.DESC_NOTIFY_TITLE_ORDER_FINISHED,
                Body = string.Format(AppMessage.DESC_NOTIFY_BODY_ORDER_FINISHED, $"{order.Id:D9}")
            };
            await uow.GetRepo<Transaction>().AddAsync(transaction);
            await uow.GetRepo<Announcement>().AddAsync(announcement);
            await uow.SaveChangesAsync();
        }
        #endregion
        #region Schedule product update
        public void ScheduleProductUpdate(Product product, DateTime enqueueAt)
        {
            var monitor = JobStorage.Current.GetMonitoringApi();
            var jobId = monitor.ScheduledJobs(0, int.MaxValue)
                               .FirstOrDefault(kv => kv.Value.Job.Method.Name == nameof(UpdateProductAsync)
                                                     && (int)kv.Value.Job.Args[0] == product.Id).Key;
            if (jobId != null) BackgroundJob.Delete(jobId);
            if (jobId != null) BackgroundJob.Delete(jobId);
            BackgroundJob.Schedule(() => UpdateProductAsync(product.Id, product), enqueueAt);
        }
        [JobDisplayName($"{nameof(BackgroundService.UpdateProductAsync)}_{{0}}")]
        public async Task UpdateProductAsync(int productId, Product product)
        {
            uow.GetRepo<Product>().Update(product);
            await uow.SaveChangesAsync();
        }
        #endregion
        #region Schedule remove device token
        public void ScheduleRemoveDevice(int accountId, DateTime enqueueAt)
        {
            var monitor = JobStorage.Current.GetMonitoringApi();
            var jobId = monitor.ScheduledJobs(0, int.MaxValue)
                               .FirstOrDefault(kv => kv.Value.Job.Method.Name == nameof(RemoveDeviceAsync) 
                                                     && (int) kv.Value.Job.Args[0] == accountId).Key;
            if (jobId != null) BackgroundJob.Delete(jobId);
            BackgroundJob.Schedule(() => RemoveDeviceAsync(accountId), enqueueAt);
        }
        [JobDisplayName($"{nameof(RemoveDeviceAsync)}_{{0}}")]
        public async Task RemoveDeviceAsync(int accountId)
        {
            var account = await uow.GetRepo<Account>().FindAsync(accountId);
            if (account == null || account.DeviceToken == null) return;
            account.DeviceToken = null;
            await uow.SaveChangesAsync();
        }
        #endregion
        #region Recur calculate destination temp
        public void RecurCalculateDestinationTemp()
        {
            RecurringJob.AddOrUpdate("",() => CalculateDestinationTempAsync(), Cron.Weekly());
        }
        public async Task CalculateDestinationTempAsync()
        {
            var now = timeService.Now;
            var minAt = now.AddDays(BackgroundConstants.TRENDING_DESTINATIONS_CALCULATE_DAY_DURATION);
            var plans = await uow.GetRepo<Plan>()
                                 .GetAll()
                                 .Where(p => p.UtcStartAt <= now
                                             && p.UtcStartAt >= minAt
                                             && p.Status >= PlanStatus.READY 
                                             && p.Status != PlanStatus.CANCELED)
                                 .ToListAsync();
            var groups = plans.GroupBy(p => p.DestinationId).ToList();
            if (groups.Count == 0) return;
            var dict = new Dictionary<int, int>();
            groups.ForEach(g => dict.Add(g.Key, g.Count()));
            dict = dict.OrderBy(kv => kv.Value).Take(10).ToDictionary();
            var json = JsonConvert.SerializeObject(dict);
            var statisticalData = new StatisticalData
            {
                Key = BackgroundConstants.TRENDING_DESTINATIONS,
                Value = json
            };
            await uow.GetRepo<StatisticalData>().AddAsync(statisticalData);
            await uow.SaveChangesAsync();
        }
        #endregion
    }

}
