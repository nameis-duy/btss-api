using Application.DTOs.Traveler;
using FluentValidation;
using Infrastructure.Constants;
using Infrastructure.Utilities;

namespace Infrastructure.Validators.Traveler
{
    public class TravelerCreateValidator : AbstractValidator<TravelerCreate>
    {
        public TravelerCreateValidator()
        {
            RuleFor(t => t.Name).NotEmpty()
                                .Length(ValidationConstants.ACCOUNT_NAME_MIN_LENGTH,
                                        ValidationConstants.ACCOUNT_NAME_MAX_LENGTH)
                                .WithMessage(string.Format(AppMessage.ERR_ACCOUNT_NAME_LENGTH,
                                                           ValidationConstants.ACCOUNT_NAME_MIN_LENGTH,
                                                           ValidationConstants.ACCOUNT_NAME_MAX_LENGTH))
                                .Must(a => !a.RemoveDiacritics().Contains(ValidationConstants.TEST_ACCOUNT_PREFIX))
                                .WithMessage(AppMessage.ERR_ACCOUNT_NAME_INVALID);
            RuleFor(t => t.AvatarUrl!).NotEmpty().FromValidSource().When(t => !string.IsNullOrEmpty(t.AvatarUrl));
            RuleFor(t => t.DeviceToken).NotEmpty().WithMessage(AppMessage.ERR_ACCOUNT_DEVICE_TOKEN_EMPTY);
        }
    }
}
