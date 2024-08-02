using Application.DTOs.Provider;
using FluentValidation;
using Infrastructure.Constants;

namespace Infrastructure.Validators.Provider
{
    public class ProviderCreateValidator : AbstractValidator<ProviderCreate>
    {
        public ProviderCreateValidator()
        {
            RuleFor(p => p.Name).Length(ValidationConstants.PROVIDER_NAME_MIN_LENGTH,
                                        ValidationConstants.PROVIDER_NAME_MAX_LENGTH)
                                .WithMessage(string.Format(AppMessage.ERR_PROVIDER_NAME_LENGTH,
                                                           ValidationConstants.PROVIDER_NAME_MIN_LENGTH,
                                                           ValidationConstants.PROVIDER_NAME_MAX_LENGTH));
            RuleFor(p => p.Type).IsInEnum().WithMessage(AppMessage.ERR_PROVIDER_TYPE_INVALID);
            When(p => ValidationConstants.REQUIRE_STANDARD_TYPE.Contains(p.Type), () =>
            {
                RuleFor(p => p.Standard).InclusiveBetween(ValidationConstants.PROVIDER_STANDARD_MIN,
                                                          ValidationConstants.PROVIDER_STANDARD_MAX)
                                        .WithMessage(string.Format(AppMessage.ERR_PROVIDER_NEED_STANDARD,
                                                                   ValidationConstants.PROVIDER_STANDARD_MIN,
                                                                   ValidationConstants.PROVIDER_STANDARD_MAX));
            }).Otherwise(() =>
            {
                RuleFor(p => p.Standard).Null().WithMessage(AppMessage.ERR_PROVIDER_NO_STANDARD);
            });
            RuleFor(p => p.Phone).NotEmpty()
                                 .WithMessage(AppMessage.ERR_PHONE_EMPTY)
                                 .Matches(ValidationConstants.PHONE_FORMAT)
                                 .WithMessage(AppMessage.ERR_PHONE_FORMAT);
            RuleFor(p => p.Address).Length(ValidationConstants.ADDRESS_MIN_LENGTH,
                                           ValidationConstants.ADDRESS_MAX_LENGTH)
                                   .WithMessage(string.Format(AppMessage.ERR_ADDRESS_LENGTH,
                                                              ValidationConstants.ADDRESS_MIN_LENGTH,
                                                              ValidationConstants.ADDRESS_MAX_LENGTH));
            RuleFor(p => p.Coordinate).IsInsideGeometry(ValidationConstants.REGION)
                                      .WithMessage(AppMessage.ERR_COORDINATE_INVALID);
            RuleFor(p => p.ImageUrl).FromValidSource().WithMessage(AppMessage.ERR_IMAGE_SOURCE_INVALID);
        }
    }
}
