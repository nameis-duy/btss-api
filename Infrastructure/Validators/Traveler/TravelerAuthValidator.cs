using Application.DTOs.Generic;
using Application.DTOs.Traveler;
using Application.Interfaces.Services;
using Domain.Enums.Others;
using FirebaseAdmin.Messaging;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.Extensions.Options;
using Vonage.Verify;
using Vonage;
using Infrastructure.Utilities;

namespace Infrastructure.Validators.Traveler
{
    public class TravelerAuthValidator : AbstractValidator<TravelerAuth>
    {
        public TravelerAuthValidator(ICacheService cacheService,
                                     IOptionsSnapshot<AppConfig> snapshot,
                                     VonageClient vonageClient,
                                     IClaimService claimService)
        {
            RuleFor(t => t.Channel).IsInEnum().WithMessage(AppMessage.ERR_CHANNEL_INVALID);
            RuleFor(t => t.Phone).NotEmpty()
                                 .WithMessage(AppMessage.ERR_PHONE_EMPTY)
                                 .Matches(ValidationConstants.PHONE_FORMAT)
                                 .WithMessage(AppMessage.ERR_PHONE_FORMAT);
            var exactOTPLength = (int)AuthConstants.OTP_LENGTH;
            RuleFor(t => t.OTP).NotNull()
                               .Length(exactOTPLength)
                               .WithMessage(string.Format(AppMessage.ERR_OTP_LENGTH, exactOTPLength))
                               .CustomAsync(async (otp, context, ct) =>
                               {

                                   var dto = context.InstanceToValidate;
                                   var key = dto.Phone.HashWithNoSalt();
                                   if (snapshot.Value.USE_FIXED_OTP)
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
                               });
            RuleFor(t => t.DeviceToken).NotEmpty().WithMessage(AppMessage.ERR_ACCOUNT_DEVICE_TOKEN_EMPTY);
        }
    }
}
