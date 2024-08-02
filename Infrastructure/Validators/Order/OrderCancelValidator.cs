using Application.DTOs.Order;
using Application.DTOs.Generic;
using Application.Interfaces.Services;
using Domain.Enums.Others;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vonage.Verify;
using Vonage;
using Infrastructure.Utilities;
using Domain.Enums.Provider;

namespace Infrastructure.Validators.Order
{
    public class OrderCancelValidator : AbstractValidator<OrderCancel>
    {
        public OrderCancelValidator(IOrderService orderService,
                                    IClaimService claimService,
                                    ITimeService timeService,
                                    ICacheService cacheService,
                                    VonageClient vonageClient,
                                    IOptionsSnapshot<AppConfig> snapshot)
        {
            var role = claimService.GetClaim(ClaimConstants.ROLE, Role.TRAVELER);
            var appConfig = snapshot.Value;
            RuleFor(o => o.OrderId).CustomAsync(async (id, context, ct) =>
            {
                var order = await orderService.GetAll(true)
                                              .Include(o => o.Provider.Account)
                                              .Include(o => o.Plan)
                                              .Include(o => o.Account)
                                              .FirstOrDefaultAsync(o => o.Id == id, ct);
                if (order == null)
                {
                    context.AddFailure(AppMessage.ERR_ORDER_NOT_FOUND);
                    return;
                }
                switch(role)
                {
                    case Role.TRAVELER:
                        var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
                        if (order.AccountId != accountId)
                        {
                            context.AddFailure(AppMessage.ERR_AUTHORIZE);
                            return;
                        }
                        break;
                    case Role.PROVIDER:
                        var providerId = claimService.GetClaim(ClaimConstants.PROVIDER_ID, -1);
                        if (order.ProviderId != providerId)
                        {
                            context.AddFailure(AppMessage.ERR_AUTHORIZE);
                            return;
                        }
                        break;
                    case Role.STAFF:
                        if (order.Provider.Account != null)
                        {
                            context.AddFailure(AppMessage.ERR_AUTHORIZE);
                            return;
                        }
                        break;
                }
                if (order.CurrentStatus > OrderStatus.PREPARED)
                {
                    context.AddFailure(AppMessage.ERR_ORDER_CANCEL_STATUS);
                    return;
                }
                var now = timeService.Now;
                if (order.CreatedAt.AddDays(appConfig.ORDER_CANCEL_DATE_DURATION) < now)
                {
                    context.AddFailure(string.Format(AppMessage.ERR_ORDER_CANCEL_DATE,
                                                     appConfig.ORDER_CANCEL_DATE_DURATION));
                    return;
                }
            });
            var exactOTPLength = (int)AuthConstants.OTP_LENGTH;
            if (role != Role.TRAVELER)
            {
                RuleFor(o => o.OTP).NotEmpty()
                                   .Length(exactOTPLength)
                                   .WithMessage(string.Format(AppMessage.ERR_OTP_LENGTH, exactOTPLength))
                                   .CustomAsync(async (otp, context, ct) =>
                                   {
                                       var dto = context.InstanceToValidate;
                                       var key = string.Format(CacheConstants.ORDER_CANCEL_FORMAT, dto.OrderId);
                                       if (appConfig.USE_FIXED_OTP)
                                       {
                                           var cachedOTP = await cacheService.GetDataAsync<string>(key);
                                           if (dto.OTP != cachedOTP) throw new ArgumentException(AppMessage.ERR_OTP_INVALID);
                                       }
                                       else
                                       {
                                           switch (dto.Channel)
                                           {
                                               case MessageChannel.VONAGE:
                                                   var requestId = await cacheService.GetDataAsync<string>(key);
                                                   var request = new VerifyCheckRequest() { Code = dto.OTP, RequestId = requestId };
                                                   var response = await vonageClient.VerifyClient.VerifyCheckAsync(request);
                                                   if (response.IsSuccessVerifyCheckResponse()) break;
                                                   throw new Exception(response.ErrorText);
                                           }
                                       }
                                       await cacheService.RemoveDataAsync(key);
                                   })
                                   .When(o => !string.IsNullOrWhiteSpace(o.OTP));
                RuleFor(o => o.Channel).IsInEnum().WithMessage(AppMessage.ERR_CHANNEL_INVALID).When(o => !string.IsNullOrWhiteSpace(o.OTP));
            } else
            {
                RuleFor(o => o.OTP).Empty().WithMessage(AppMessage.ERR_AUTHORIZE);
                RuleFor(o => o.Channel).Null().WithMessage(AppMessage.ERR_AUTHORIZE);
            }
            RuleFor(o => o.Reason).Length(ValidationConstants.ORDER_COMMENT_MIN_LENGTH,
                                          ValidationConstants.ORDER_COMMENT_MAX_LENGTH)
                                  .WithMessage(string.Format(AppMessage.ERR_ORDER_CANCEL_REASON_LENGTH,
                                                             ValidationConstants.ORDER_COMMENT_MIN_LENGTH,
                                                             ValidationConstants.ORDER_COMMENT_MAX_LENGTH));
        }
    }
}
