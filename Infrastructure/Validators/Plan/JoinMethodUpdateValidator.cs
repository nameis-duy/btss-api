using Application.DTOs.Plan;
using Application.Interfaces.Services;
using Domain.Enums.Plan;
using FluentValidation;
using Infrastructure.Constants;

namespace Infrastructure.Validators.Plan
{
    public class JoinMethodUpdateValidator : AbstractValidator<JoinMethodUpdate>
    {
        public JoinMethodUpdateValidator(IPlanService planService, IClaimService claimService)
        {
            RuleFor(m => m.PlanId).CustomAsync(async (planId, context, ct) =>
            {
                var plan = await planService.FindAsync(planId);
                if (plan == null)
                {
                    context.AddFailure(AppMessage.ERR_PLAN_NOT_FOUND);
                    return;
                }
                var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
                if (plan.AccountId != accountId)
                {
                    context.AddFailure(AppMessage.ERR_AUTHORIZE);
                    return;
                }
                if (plan.Status != PlanStatus.REGISTERING)
                {
                    context.AddFailure(AppMessage.ERR_PLAN_REGISTERING_JOIN_METHOD);
                    return;
                }
                if (plan.JoinMethod == context.InstanceToValidate.JoinMethod)
                {
                    context.AddFailure(AppMessage.ERR_PLAN_JOIN_METHOD_UNCHANGE);
                }
            });
            RuleFor(m => m.JoinMethod).IsInEnum().WithMessage(AppMessage.ERR_PLAN_JOIN_METHOD_INVALID);
        }
    }
}
