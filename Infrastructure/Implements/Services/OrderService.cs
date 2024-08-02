using Application.DTOs.Generic;
using Application.DTOs.Order;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Announcement;
using Domain.Enums.Others;
using Domain.Enums.Transaction;
using Domain.JsonEntities;
using FluentValidation;
using Infrastructure.Constants;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vonage.Verify;
using Vonage;
using Infrastructure.Utilities;
using Microsoft.Extensions.Configuration;
using Domain.Enums.Plan;
using Domain.Enums.Provider;

namespace Infrastructure.Implements.Services
{
    public class OrderService : GenericService<Order>, IOrderService
    {
        private readonly IBackgroundService backgroundService;
        private readonly VonageClient vonageClient;
        private readonly IAnnouncementService announcementService;
        private readonly IConfiguration config;

        public OrderService(IOptionsSnapshot<AppConfig> configSnapshot,
                            IUnitOfWork uow,
                            ITimeService timeService,
                            IClaimService claimService,
                            ICacheService cacheService,
                            IBackgroundService backgroundService,
                            VonageClient vonageClient,
                            IAnnouncementService announcementService,
                            IConfiguration config) : base(configSnapshot,
                                                               uow,
                                                               timeService,
                                                               claimService,
                                                               cacheService)
        {
            this.backgroundService = backgroundService;
            this.vonageClient = vonageClient;
            this.announcementService = announcementService;
            this.config = config;
        }
        #region Get orders
        public IQueryable<Order> GetOrders()
        {
            var role = claimService.GetClaim(ClaimConstants.ROLE, Role.TRAVELER);
            var source = uow.GetRepo<Order>().GetAll();
            switch (role)
            {
                case Role.TRAVELER:
                    var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
                    return source.Where(o => o.AccountId == accountId);
                case Role.PROVIDER:
                    var providerId = claimService.GetClaim(ClaimConstants.PROVIDER_ID, -1);
                    return source.Where(o => o.ProviderId == providerId);
                default:
                    return source.Where(o => o.Provider.Account == null);
            }
        }
        #endregion
        #region Create order
        public async Task<Order> CreateOrderAsync(OrderCreate dto)
        {
            var order = dto.Adapt<Order>();
            var now = timeService.Now;
            var holidayMultiply = 1m;
            switch (order.Type)
            {
                case EventType.EAT:
                    holidayMultiply = 1 + appConfig.HOLIDAY_MEAL_UP_PCT * 1m / 100;
                    break;
                case EventType.CHECKIN:
                    holidayMultiply = 1 + appConfig.HOLIDAY_LODGING_UP_PCT * 1m / 100;
                    break;
                case EventType.VISIT:
                    holidayMultiply = 1 + appConfig.HOLIDAY_RIDING_UP_PCT * 1m / 100;
                    break;
            }
            var totalVND = 0m;
            var key = string.Format(CacheConstants.ORDER_DETAILS_FORMAT, claimService.GetUniqueRequestId());
            var products = await cacheService.GetDataAsync<List<Product>>(key)
                           ?? await uow.GetRepo<Product>()
                                       .GetAll()
                                       .Where(p => dto.Cart.Keys.Contains(p.Id))
                                       .ToListAsync();
            order.Details = [];
            foreach (var product in products)
            {
                foreach (var date in order.ServeDates)
                {
                    var price = product.Price;
                    price *= appConfig.HOLIDAYS.Any(h => h.From <= date && h.To >= date) ? holidayMultiply : 1m;
                    var detailTotalVND = price * dto.Cart[product.Id];
                    totalVND += detailTotalVND;
                    order.Total += detailTotalVND;
                    order.Details.Add(new OrderDetail
                    {
                        Date = date,
                        Price = price,
                        ProductId = product.Id,
                        Quantity = dto.Cart[product.Id],
                        Total = detailTotalVND
                    });
                }
            }
            order.Deposit = order.Total;
            order.ProviderId = products[0].ProviderId;
            var gcoinAmount = order.Total / GlobalConstants.VND_CONVERT_RATE;
            var plan = await uow.GetRepo<Plan>().FindAsync(dto.PlanId)
                       ?? throw new KeyNotFoundException(AppMessage.ERR_PLAN_NOT_FOUND);
            if (plan.ActualGcoinBudget < gcoinAmount) throw new ValidationException(AppMessage.ERR_BALANCE_NOT_ENOUGH);
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            order.AccountId = accountId;
            order.CreatedAt = now;
            order.CurrentStatus = OrderStatus.RESERVED;
            order.Traces = [];
            order.Traces.Add(new OrderTrace
            {
                IsClientAction = true,
                ModifiedAt = now,
                Status = order.CurrentStatus,
                Description = AppMessage.DESC_ORDER_TRACE_CREATED
            });
            var transaction = new Transaction
            {
                BankTransCode = Guid.NewGuid().ToString(),
                CreatedAt = now,
                Gateway = Gateway.INTERNAL,
                GcoinAmount = gcoinAmount,
                Order = order,
                AccountId = order.AccountId,
                Status = TransactionStatus.ACCEPTED,
                Type = TransactionType.ORDER,
                Description = string.Format(AppMessage.DESC_TRANSACTION_ORDER_CREATE, plan.Name)
            };
            plan.ActualGcoinBudget -= transaction.GcoinAmount;
            plan.DisplayGcoinBudget = plan.DisplayGcoinBudget > gcoinAmount ? plan.DisplayGcoinBudget - gcoinAmount : 0;
            var provider = await uow.GetRepo<Provider>().FindAsync(order.ProviderId)
                           ?? throw new KeyNotFoundException(AppMessage.ERR_PROVIDER_NOT_FOUND);
            var announcement = new Announcement
            {
                AccountId = provider.Account != null ? provider.Account.Id : null,
                CreatedAt = now,
                Type = AnnouncementType.ORDER,
                Order = order,
                Title = AppMessage.DESC_NOTIFY_TITLE_NEW_ORDER,
                Body = string.Format(AppMessage.DESC_NOTIFY_BODY_NEW_ORDER, provider.Name),
            };
            await uow.GetRepo<Order>().AddAsync(order);
            await uow.GetRepo<Transaction>().AddAsync(transaction);
            await uow.GetRepo<Announcement>().AddAsync(announcement);
            if (await uow.SaveChangesAsync()) return order;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Change status
        public async Task<Order> ChangeStatusAsync(OrderChangeStatus dto)
        {
            var order = await uow.GetRepo<Order>().FindAsync(dto.OrderId)
                        ?? throw new KeyNotFoundException(AppMessage.ERR_ORDER_NOT_FOUND);
            var description = string.Format(order.CurrentStatus == OrderStatus.RESERVED 
                                            ? AppMessage.DESC_ORDER_TRACE_PREPARED : AppMessage.DESC_ORDER_TRACE_SERVED,
                                            order.Provider.Name);
            var now = timeService.Now;
            var trace = new OrderTrace
            {
                Description = description,
                Status = dto.Status,
                ModifiedAt = now,
            };
            order!.CurrentStatus = trace.Status;
            order.Traces.Add(trace);
            if (await uow.SaveChangesAsync())
            {
                if (order.CurrentStatus == OrderStatus.SERVED) 
                    backgroundService.ScheduleOrderFinish(order.Id, now.AddDays(BackgroundConstants.DAYS_FINISH_ORDER));
                return order;
            }
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Get order cancel otp
        public async Task<bool> GetOrderCancelOTPAsync(OrderCancelOTP dto)
        {
            var key = string.Format(CacheConstants.ORDER_CANCEL_FORMAT, dto.OrderId);
            if (appConfig.USE_FIXED_OTP)
            {
                var otp = AuthConstants.TEST_AUTHORIZE_OTP;
                if (!await cacheService.SetDataAsync(key, otp, CacheConstants.DEFAULT_VALID_MINUTE))
                    throw new SystemException(AppMessage.ERR_FUNCTION_DOWN);
                return true;
            }
            var order = await uow.GetRepo<Order>().FindAsync(dto.OrderId)
                        ?? throw new KeyNotFoundException(AppMessage.ERR_ORDER_NOT_FOUND);
            switch (dto.Channel)
            {
                case MessageChannel.VONAGE:
                    var request = new VerifyRequest()
                    {
                        Brand = config["Brandname"] ?? GlobalConstants.DEFAULT_BRAND_NAME,
                        Number = order.Account.Phone,
                        Country = "VN",
                        NextEventWait = CacheConstants.DEFAULT_VALID_MINUTE * 60,
                        CodeLength = (int)AuthConstants.OTP_LENGTH,
                        WorkflowId = VerifyRequest.Workflow.TTS
                    };
                    var response = await vonageClient.VerifyClient.VerifyRequestAsync(request);
                    if (response.IsSuccessVerifyResponse())
                    {
                        if (await cacheService.SetDataAsync(key, response.RequestId, CacheConstants.DEFAULT_VALID_MINUTE))
                            return true;
                        throw new SystemException(AppMessage.ERR_FUNCTION_DOWN);
                    }
                    throw new Exception(response.ErrorText);
            }
            throw new SystemException(AppMessage.ERR_FUNCTION_DOWN);
        }
        #endregion
        #region Cancel order
        public async Task<Order> CancelOrderAsync(OrderCancel dto)
        {
            var order = await uow.GetRepo<Order>().FindAsync(dto.OrderId)
                        ?? throw new KeyNotFoundException(AppMessage.ERR_ORDER_NOT_FOUND);
            var now = timeService.Now;
            var role = claimService.GetClaim(ClaimConstants.ROLE, Role.TRAVELER);
            order.CurrentStatus = OrderStatus.CANCELLED;
            var trace = new OrderTrace
            {
                Description = dto.Reason,
                IsClientAction = dto.Channel.HasValue || role == Role.TRAVELER,
                ModifiedAt = now,
                Status = order.CurrentStatus
            };
            order.Traces.Add(trace);
            var gcoinAmount = order.Deposit / GlobalConstants.VND_CONVERT_RATE;
            if (trace.IsClientAction)
            {
                var dayDiff = (now - order.CreatedAt).TotalDays;
                var minusPct = 0;
                switch (dayDiff)
                {
                    case var t when t <= 1:
                        minusPct = appConfig.ORDER_REFUND_CUSTOMER_CANCEL_1_DAY_PCT;
                        break;
                    case var t when t <= 2:
                        minusPct = appConfig.ORDER_REFUND_CUSTOMER_CANCEL_2_DAY_PCT;
                        break;
                    default:
                        break;
                }
                gcoinAmount *= minusPct * 1.0m / 100;
            }
            var body = string.Format(AppMessage.DESC_NOTIFY_BODY_ORDER_CANCEL,
                                     trace.IsClientAction ? order.Account.Name : order.Provider.Name,
                                     $"{order.Id:D9}");
            var annoucement = new Announcement
            {
                CreatedAt = now,
                Level = AnnouncementLevel.ERROR,
                Type = AnnouncementType.ORDER,
                OrderId = order.Id,
                Title = AppMessage.DESC_NOTIFY_TITLE_ORDER_CANCEL,
                Body = body
            };
            if (trace.IsClientAction) annoucement.ProviderId = order.ProviderId;
            else annoucement.AccountId = order.AccountId;
            await uow.GetRepo<Announcement>().AddAsync(annoucement);
            if (gcoinAmount > 0)
            {
                order.Deposit -= gcoinAmount * GlobalConstants.VND_CONVERT_RATE;
                var travelerTransaction = new Transaction
                {
                    AccountId = order.AccountId,
                    BankTransCode = Guid.NewGuid().ToString(),
                    CreatedAt = now,
                    Gateway = Gateway.INTERNAL,
                    GcoinAmount = gcoinAmount,
                    OrderId = order.Id,
                    Status = TransactionStatus.ACCEPTED,
                    Type = TransactionType.ORDER_REFUND,
                    Description = AppMessage.DESC_TRANSACTION_ORDER_REFUND,
                };
                order.Plan.ActualGcoinBudget += travelerTransaction.GcoinAmount;
                order.Plan.DisplayGcoinBudget += travelerTransaction.GcoinAmount;
                var providerTransaction = new Transaction
                {
                    ProviderId = order.ProviderId,
                    Status = TransactionStatus.ACCEPTED,
                    CreatedAt = now,
                    BankTransCode = Guid.NewGuid().ToString(),
                    Gateway = Gateway.INTERNAL,
                    GcoinAmount = order.Deposit / GlobalConstants.VND_CONVERT_RATE,
                    OrderId = order.Id,
                    Description = string.Format(AppMessage.DESC_TRANSACTION_ORDER_COMPLETE, $"{order.Id:D9}"),
                    Type = TransactionType.ORDER
                };
                order.Provider.Balance += order.Deposit;
                var transactions = new List<Transaction>() { travelerTransaction, providerTransaction };
                await uow.GetRepo<Transaction>().AddAsync(transactions);
            }
            if (await uow.SaveChangesAsync())
            {
                if (!trace.IsClientAction && order.Account.DeviceToken != null)
                    await announcementService.PushToDevicesAsync(annoucement, order.Account.DeviceToken);
                return order;
            }
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
    }
}
