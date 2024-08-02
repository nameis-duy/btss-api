using Application.DTOs.Provider;
using Application.Interfaces.Services;
using Domain.Enums.Others;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Validators.Provider
{
    public class ProviderUpdateValidator : AbstractValidator<ProviderUpdate>
    {
        public ProviderUpdateValidator(IProviderService providerService,
                                       IClaimService claimService,
                                       ProviderCreateValidator createValidator)
        {
            Include(createValidator);
            RuleFor(p => p.ProviderId).CustomAsync(async (id, context, ct) =>
            {
                var provider = await providerService.FindAsync(id);
                if (provider == null)
                {
                    context.AddFailure(AppMessage.ERR_PROVIDER_NOT_FOUND);
                    return;
                }
                var role = claimService.GetClaim(ClaimConstants.ROLE, Role.PROVIDER);
                if (role == Role.PROVIDER)
                {
                    var providerId = claimService.GetClaim(ClaimConstants.PROVIDER_ID, -1);
                    if (providerId != id)
                    {
                        context.AddFailure(AppMessage.ERR_AUTHORIZE);
                        return;
                    }
                }
                var dto = context.InstanceToValidate;
                if ((ValidationConstants.NO_PRODUCT_TYPE.Contains(provider.Type)
                     && !ValidationConstants.NO_PRODUCT_TYPE.Contains(dto.Type))
                    || (ValidationConstants.NO_PRODUCT_TYPE.Contains(dto.Type)
                        && !ValidationConstants.NO_PRODUCT_TYPE.Contains(provider.Type)))
                {
                    context.AddFailure(AppMessage.ERR_PROVIDER_TYPE_UPDATE_CONFLICT);
                    return;
                }
            });
        }
    }
}
