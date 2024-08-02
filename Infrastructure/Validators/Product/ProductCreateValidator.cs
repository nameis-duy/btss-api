using Application.DTOs.Product;
using Application.Interfaces.Services;
using Domain.Enums.Others;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators.Product
{
    public class ProductCreateValidator : AbstractValidator<ProductCreate>
    {
        public ProductCreateValidator(IProviderService providerService, IClaimService claimService)
        {
            RuleFor(p => p.Name).Length(ValidationConstants.PRODUCT_NAME_MIN_LENGTH,
                                        ValidationConstants.PRODUCT_NAME_MAX_LENGTH)
                                .WithMessage(string.Format(AppMessage.ERR_PRODUCT_NAME_LENGTH,
                                                           ValidationConstants.PRODUCT_NAME_MIN_LENGTH,
                                                           ValidationConstants.PRODUCT_NAME_MAX_LENGTH));
            RuleFor(p => p.Type).IsInEnum()
                                .WithMessage(AppMessage.ERR_PRODUCT_TYPE_INVALID);
            RuleFor(p => p.Price).InclusiveBetween(ValidationConstants.PRODUCT_PRICE_MIN,
                                                   ValidationConstants.PRODUCT_PRICE_MAX)
                                 .WithMessage(string.Format(AppMessage.ERR_PRODUCT_PRICE_RANGE,
                                                            ValidationConstants.PRODUCT_PRICE_MIN,
                                                            ValidationConstants.PRODUCT_PRICE_MAX));
            RuleFor(p => p.ImageUrl).FromValidSource().WithMessage(AppMessage.ERR_IMAGE_SOURCE_INVALID);
            RuleFor(p => p.PartySize).InclusiveBetween(ValidationConstants.PRODUCT_PARTYSIZE_MIN,
                                                       ValidationConstants.PRODUCT_PARTYSIZE_MAX)
                                     .WithMessage(string.Format(AppMessage.ERR_PRODUCT_PARTYSIZE_RANGE,
                                                                ValidationConstants.PRODUCT_PARTYSIZE_MIN,
                                                                ValidationConstants.PRODUCT_PARTYSIZE_MAX));
            RuleFor(p => p.Periods).AllValuesValid().WithMessage(AppMessage.ERR_PRODUCT_PERIOD_INVALID);
            RuleFor(p => p.Description).NotEmpty()
                                       .Length(ValidationConstants.PRODUCT_DESCRIPTION_MIN_LENGTH,
                                               ValidationConstants.PRODUCT_DESCRIPTION_MAX_LENGTH)
                                       .WithMessage(string.Format(AppMessage.ERR_PRODUCT_DESCRIPTION_LENGTH,
                                                                  ValidationConstants.PRODUCT_DESCRIPTION_MIN_LENGTH,
                                                                  ValidationConstants.PRODUCT_DESCRIPTION_MAX_LENGTH))
                                       .When(p => !string.IsNullOrWhiteSpace(p.Description));
            RuleFor(p => p.ProviderId).CustomAsync(async (providerId, context, ct) =>
            {
                var provider = await providerService.GetAll()
                                                    .Include(p => p.Account)
                                                    .FirstOrDefaultAsync(p => p.Id == providerId);
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
                    context.AddFailure(AppMessage.ERR_PROVIDER_NO_SUPPORT_PRODUCT);
                    return;
                }
                if (provider.Account == null) return;
                var role = claimService.GetClaim(ClaimConstants.ROLE, Role.STAFF);
                var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
                if (role == Role.STAFF || accountId != provider.Account.Id)
                {
                    context.AddFailure(AppMessage.ERR_AUTHORIZE);
                    return;
                }
            });
        }
    }
}
