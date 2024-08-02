using Application.DTOs.Traveler;
using Application.Interfaces.Services;
using Domain.Enums.Others;
using FluentValidation;
using Infrastructure.Constants;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators.Traveler
{
    public class TravelerRequestOTPValidator : AbstractValidator<TravelerRequestOTP>
    {
        public TravelerRequestOTPValidator(IAccountService accountService,
                                           ICacheService cacheService,
                                           IClaimService claimService)
        {
            RuleFor(t => t.Phone).NotEmpty()
                                 .WithMessage(AppMessage.ERR_PHONE_EMPTY)
                                 .Matches(ValidationConstants.PHONE_FORMAT)
                                 .WithMessage(AppMessage.ERR_PHONE_FORMAT)
                                 .CustomAsync(async (phone, context, ct) =>
                                 {
                                     var account = await accountService.GetAll()
                                                                       .FirstOrDefaultAsync(a => a.Phone == phone, ct);
                                     if (account != null && !account.IsActive)
                                     {
                                         context.AddFailure(AppMessage.ERR_ACCOUNT_BLOCKED);
                                         return;
                                     }
                                     var key = phone.HashWithNoSalt();
                                     if (await cacheService.IsKeyExistedAsync(key))
                                     {
                                         context.AddFailure(AppMessage.ERR_OTP_EXPIRY);
                                         return;
                                     }
                                 });
            RuleFor(t => t.Channel).IsInEnum().WithMessage(AppMessage.ERR_CHANNEL_INVALID);
        }
    }
}
