using Application.DTOs.Staff_Admin;
using Application.Interfaces.Services;
using Domain.Enums.Others;
using FluentValidation;
using Infrastructure.Constants;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Validators.Staff_Admin
{
    public class StaffAuthValidator : AbstractValidator<StaffAuth>
    {
        public StaffAuthValidator(IConfiguration config,
                                       IAccountService accountService,
                                       ICacheService cacheService,
                                       IClaimService claimService)
        {
            RuleFor(s => s.Email).EmailAddress()
                                 .WithMessage(AppMessage.ERR_ACCOUNT_EMAIL_INVALID)
                                 .CustomAsync(async (email, context, ct) =>
            {
                bool isRightPassword = config["Admin:Password"] == context.InstanceToValidate.Password;
                if (email != config["Admin:Email"])
                {
                    var account = await accountService.GetAll().FirstOrDefaultAsync(a => a.Email == email && a.Role != Role.TRAVELER, ct);
                    if (account == null)
                    {
                        context.AddFailure(AppMessage.ERR_ACCOUNT_EMAIL_NOT_FOUND);
                        return;
                    }
                    if (!account.IsActive)
                    {
                        context.AddFailure(AppMessage.ERR_ACCOUNT_BLOCKED);
                        return;
                    }
                    isRightPassword = account.PasswordHash!.VerifyHashString(context.InstanceToValidate.Password);
                    if (isRightPassword)
                    {
                        var requestUID = claimService.GetUniqueRequestId();
                        var key = string.Format(CacheConstants.STAFF_INFO_FORMAT, requestUID);
                        await cacheService.SetDataAsync(key, account, CacheConstants.DEFAULT_VALID_MINUTE);
                    }
                }
                if (!isRightPassword)
                {
                    context.AddFailure(AppMessage.ERR_ACCOUNT_AUTHORIZE_FAIL);
                    return;
                }
            });
            RuleFor(s => s.Password).NotEmpty().WithMessage(AppMessage.ERR_ACCOUNT_PASSWORD_EMPTY);
        }
    }
}
