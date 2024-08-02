using Application.DTOs.Traveler;
using Application.Interfaces.Services;
using FluentValidation;
using Infrastructure.Constants;
using Infrastructure.Utilities;

namespace Infrastructure.Validators.Traveler
{
    public class TravelerUpdateValidator : AbstractValidator<TravelerUpdate>
    {
        public TravelerUpdateValidator(IAccountService accountService, IClaimService claimService)
        {
            RuleFor(a => a).CustomAsync(async (a, context, ct) =>
            {
                var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
                var account = await accountService.FindAsync(accountId);
                if (account == null)
                {
                    context.AddFailure(AppMessage.ERR_ACCOUNT_NOT_FOUND);
                    return;
                }
                if (account.Name != a.Name 
                    || account.Address != a.Address 
                    || (account.Coordinate != null && account.Coordinate.Coordinate.Equals(a.Coordinate))) return;
                context.AddFailure(AppMessage.ERR_ACCOUNT_UPDATE_EMPTY);
            });
            RuleFor(t => t.Name).NotEmpty()
                                .Length(ValidationConstants.ACCOUNT_NAME_MIN_LENGTH,
                                        ValidationConstants.ACCOUNT_NAME_MAX_LENGTH)
                                .WithMessage(string.Format(AppMessage.ERR_ACCOUNT_NAME_LENGTH,
                                                           ValidationConstants.ACCOUNT_NAME_MIN_LENGTH,
                                                           ValidationConstants.ACCOUNT_NAME_MAX_LENGTH))
                                .Must(a => !a.RemoveDiacritics().Contains(ValidationConstants.TEST_ACCOUNT_PREFIX))
                                .WithMessage(AppMessage.ERR_ACCOUNT_NAME_INVALID);
            RuleFor(t => t.AvatarUrl!).NotEmpty().FromValidSource().When(t => !string.IsNullOrEmpty(t.AvatarUrl));
            RuleFor(p => p.Address).NotEmpty()
                                   .Length(ValidationConstants.ADDRESS_MIN_LENGTH,
                                           ValidationConstants.ADDRESS_MAX_LENGTH)
                                   .WithMessage(string.Format(AppMessage.ERR_ADDRESS_LENGTH,
                                                              ValidationConstants.ADDRESS_MIN_LENGTH,
                                                              ValidationConstants.ADDRESS_MAX_LENGTH))
                                   .When(t => t.Address != null);
            RuleFor(p => p.Coordinate!).IsInsideGeometry(ValidationConstants.REGION)
                                       .WithMessage(AppMessage.ERR_COORDINATE_INVALID)
                                       .When(t => t.Address != null);
        }
    }
}
