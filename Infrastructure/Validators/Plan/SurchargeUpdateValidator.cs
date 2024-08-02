using Application.DTOs.Plan;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Plan;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators.Plan
{
    public class SurchargeUpdateValidator : AbstractValidator<SurchargeUpdate>
    {
        public SurchargeUpdateValidator(IGenericService<Surcharge> surchargeService,
                                        IClaimService claimService)
        {
            RuleFor(s => s.ImageUrl).FromValidSource().WithMessage(AppMessage.ERR_IMAGE_SOURCE_INVALID);
            RuleFor(s => s.SurchargeId).CustomAsync(async (surchargeId, context, ct) =>
            {
                var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
                var surcharge = await surchargeService.GetAll(true)
                                                      .Include(s => s.Plan)
                                                      .FirstOrDefaultAsync(s => s.Id == surchargeId, ct);
                if (surcharge == null)
                {
                    context.AddFailure(AppMessage.ERR_SURCHARGE_NOT_FOUND);
                    return;
                }
                if (surcharge.Plan.AccountId != accountId)
                {
                    context.AddFailure(AppMessage.ERR_AUTHORIZE);
                    return;
                }
                switch (surcharge.Plan.Status)
                {
                    case PlanStatus.PENDING:
                    case PlanStatus.REGISTERING:
                        context.AddFailure(AppMessage.ERR_SURCHARGE_PLAN_PRE_READY);
                        return;
                    case PlanStatus.COMPLETED:
                    case PlanStatus.FLAWED:
                        context.AddFailure(AppMessage.ERR_SURCHARGE_PLAN_POST_READY);
                        return;
                    case PlanStatus.CANCELED:
                        context.AddFailure(AppMessage.ERR_SURCHARGE_PLAN_CANCEL);
                        return;
                }
                if (surcharge.ImagePath != null)
                {
                    context.AddFailure(AppMessage.ERR_SURCHARGE_ALREADY_UPDATED);
                    return;
                }
            });
        }
    }
}
