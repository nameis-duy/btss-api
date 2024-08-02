using Application.DTOs.Staff_Admin;
using Application.Interfaces.Services;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators.Staff_Admin
{
    public class StaffCreateValidator : AbstractValidator<StaffCreate>
    {
        public StaffCreateValidator(IAccountService accountService, IProviderService providerService)
        {
            RuleFor(s => s.Name).Length(ValidationConstants.ACCOUNT_NAME_MIN_LENGTH,
                                        ValidationConstants.ACCOUNT_NAME_MAX_LENGTH)
                                .WithMessage(string.Format(AppMessage.ERR_ACCOUNT_NAME_LENGTH,
                                                           ValidationConstants.ACCOUNT_NAME_MIN_LENGTH,
                                                           ValidationConstants.ACCOUNT_NAME_MAX_LENGTH));
            RuleFor(s => s.Email).EmailAddress()
                                 .WithMessage(AppMessage.ERR_ACCOUNT_EMAIL_INVALID)
                                 .MustAsync(async (staff, mail, ct) =>
                                 {
                                     return await accountService.GetAll().AnyAsync(a => a.Email == mail, cancellationToken: ct) == false;
                                  })
                                 .WithMessage(AppMessage.ERR_ACCOUNT_EMAIL_USED);
            RuleFor(s => s.Password).Length(ValidationConstants.ACCOUNT_PWD_MIN_LENGTH,
                                            ValidationConstants.ACCOUNT_PWD_MAX_LENGTH)
                                    .WithMessage(string.Format(AppMessage.ERR_ACCOUNT_PASSWORD_LENGTH,
                                                               ValidationConstants.ACCOUNT_PWD_MIN_LENGTH,
                                                               ValidationConstants.ACCOUNT_PWD_MAX_LENGTH));
            RuleFor(s => s.ProviderId).CustomAsync(async (providerId, context, ct) =>
            {
                var provider = await providerService.GetAll().Include(p => p.Account).FirstOrDefaultAsync(p => p.Id == providerId);
                if (provider == null)
                {
                    context.AddFailure(AppMessage.ERR_PROVIDER_NOT_FOUND);
                    return;
                }
                if (!provider.IsActive)
                {
                    context.AddFailure(AppMessage.ERR_PROVIDER_INACTIVE);
                    return;
                }
                if (ValidationConstants.NO_PRODUCT_TYPE.Contains(provider.Type))
                {
                    context.AddFailure(AppMessage.ERR_PROVIDER_NO_SUPPORT_ACCESS);
                    return;
                }
                if (provider.Account != null)
                {
                    context.AddFailure(AppMessage.ERR_PROVIDER_ACCOUNT_EXISTED);
                    return;
                }
            }).When(s => s.ProviderId.HasValue);
        }
    }
}
