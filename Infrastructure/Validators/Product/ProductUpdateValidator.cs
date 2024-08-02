using Application.DTOs.Generic;
using Application.DTOs.Product;
using Application.Interfaces.Services;
using Domain.Enums.Others;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Validators.Product
{
    public class ProductUpdateValidator : AbstractValidator<ProductUpdate>
    {
        public ProductUpdateValidator(IProductService productService,
                                      IClaimService claimService,
                                      IOptionsSnapshot<AppConfig> snapshot)
        {
            RuleFor(p => p.ProductId).CustomAsync(async (id, context, ct) =>
            {
                var product = await productService.GetAll(true)
                                                  .Include(p => p.Provider.Account)
                                                  .FirstOrDefaultAsync(p => p.Id == id, ct);
                if (product == null)
                {
                    context.AddFailure(AppMessage.ERR_PRODUCT_NOT_FOUND);
                    return;
                }
                var role = claimService.GetClaim(ClaimConstants.ROLE, Role.PROVIDER);
                if (role == Role.PROVIDER)
                {
                    var providerId = claimService.GetClaim(ClaimConstants.PROVIDER_ID, -1);
                    if (product.ProviderId != providerId)
                    {
                        context.AddFailure(AppMessage.ERR_AUTHORIZE);
                        return;
                    }
                } else if (product.Provider.Account == null)
                {
                    context.AddFailure(AppMessage.ERR_AUTHORIZE);
                    return;
                }
                var dto = context.InstanceToValidate;
                var maxPriceUpPct = snapshot.Value.PRODUCT_MAX_PRICE_UP_PCT;
                if (dto.Price > product.Price * (1 + maxPriceUpPct * 1.0m / 100))
                {
                    context.AddFailure(nameof(ProductUpdate.Price),
                                       string.Format(AppMessage.ERR_PRODUCT_UPDATE_PRICE_UP, maxPriceUpPct));
                    return;
                }
            });
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
            RuleFor(p => p.Description).Length(ValidationConstants.PRODUCT_DESCRIPTION_MIN_LENGTH,
                                               ValidationConstants.PRODUCT_DESCRIPTION_MAX_LENGTH)
                                       .WithMessage(string.Format(AppMessage.ERR_PRODUCT_DESCRIPTION_LENGTH,
                                                                  ValidationConstants.PRODUCT_DESCRIPTION_MIN_LENGTH,
                                                                  ValidationConstants.PRODUCT_DESCRIPTION_MAX_LENGTH));
        }
    }
}
