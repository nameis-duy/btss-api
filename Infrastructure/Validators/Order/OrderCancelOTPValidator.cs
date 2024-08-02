using Application.DTOs.Generic;
using Application.DTOs.Order;
using Application.Interfaces.Services;
using Domain.Enums.Others;
using Domain.Enums.Provider;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace Infrastructure.Validators.Order
{
    public class OrderCancelOTPValidator : AbstractValidator<OrderCancelOTP>
    {
        public OrderCancelOTPValidator(IOrderService orderService,
                                       ICacheService cacheService,
                                       IClaimService claimService,
                                       ITimeService timeService,
                                       IOptionsSnapshot<AppConfig> snapshot)
        {
            RuleFor(o => o.OrderId).CustomAsync(async (id, context, ct) =>
                                    {
                                        var order = await orderService.GetAll(true)
                                                                      .Include(o => o.Provider.Account)
                                                                      .FirstOrDefaultAsync(o => o.Id == id, ct);
                                        if (order == null)
                                        {
                                            context.AddFailure(AppMessage.ERR_ACCOUNT_BLOCKED);
                                            return;
                                        }
                                        var role = claimService.GetClaim(ClaimConstants.ROLE, Role.PROVIDER);
                                        if (role == Role.PROVIDER)
                                        {
                                            var providerId = claimService.GetClaim(ClaimConstants.PROVIDER_ID, -1);
                                            if (order.ProviderId != providerId)
                                            {
                                                context.AddFailure(AppMessage.ERR_AUTHORIZE);
                                                return;
                                            }
                                        }
                                        else if (order.Provider.Account != null)
                                        {
                                            context.AddFailure(AppMessage.ERR_AUTHORIZE);
                                            return;
                                        }
                                        if (order.CurrentStatus > OrderStatus.PREPARED)
                                        {
                                            context.AddFailure(AppMessage.ERR_ORDER_CANCEL_STATUS);
                                            return;
                                        }
                                        var maxDate = order.CreatedAt.AddDays(snapshot.Value.ORDER_CANCEL_DATE_DURATION);
                                        if (timeService.Now > maxDate)
                                        {
                                            context.AddFailure(string.Format(AppMessage.ERR_ORDER_CANCEL_DATE,
                                                                             snapshot.Value.ORDER_CANCEL_DATE_DURATION));
                                            return;
                                        }
                                        var key = string.Format(CacheConstants.ORDER_CANCEL_FORMAT, id);
                                        if (await cacheService.IsKeyExistedAsync(key))
                                        {
                                            context.AddFailure(AppMessage.ERR_OTP_EXPIRY);
                                            return;
                                        }
                                    });
            RuleFor(o => o.Channel).IsInEnum().WithMessage(AppMessage.ERR_CHANNEL_INVALID);
        }
    }
}
