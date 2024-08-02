using Application.DTOs.Plan;
using Application.Interfaces.Services;
using Domain.Enums.Plan;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators.Plan
{
    public class PlanJoinValidator : AbstractValidator<PlanJoin>
    {
        private Domain.Entities.Plan? cachedPlan;
        public PlanJoinValidator(IPlanService planService, IAccountService accountService, IClaimService claimService)
        {
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            RuleFor(p => p.PlanId).CustomAsync(async (planId, context, ct) =>
            {
                cachedPlan ??= await planService.GetAll(true)
                                                .Include(p => p.Members.Where(m => m.AccountId == accountId))
                                                .Include(p => p.Account)
                                                .FirstOrDefaultAsync(p => p.Id == planId, ct);
                if (cachedPlan == null)
                {
                    context.AddFailure(AppMessage.ERR_PLAN_NOT_FOUND);
                    return;
                }
                if (cachedPlan.Members.Any(m => m.Status == MemberStatus.BLOCKED))
                {
                    context.AddFailure(AppMessage.ERR_PLAN_BLOCKED);
                    return;
                }
                if (cachedPlan.Members.Any(m => m.Status == MemberStatus.JOINED))
                {
                    switch (cachedPlan.Status)
                    {
                        case PlanStatus.CANCELED:
                            context.AddFailure(AppMessage.ERR_PLAN_CANCEL_CANNOT_JOIN);
                            return;
                        default:
                            context.AddFailure(AppMessage.ERR_PLAN_JOINED);
                            return;
                    }
                }
                switch (cachedPlan.Status)
                {
                    case PlanStatus.CANCELED:
                        context.AddFailure(AppMessage.ERR_PLAN_CANCEL_CANNOT_JOIN);
                        return;
                    case var status when status > PlanStatus.READY:
                        context.AddFailure(AppMessage.ERR_PLAN_READY_CANNOT_JOIN);
                        return;
                    case PlanStatus.PENDING:
                        if (accountId != cachedPlan.AccountId)
                        {
                            context.AddFailure(AppMessage.ERR_PLAN_PRIVATE);
                            return;
                        }
                        break;
                    case PlanStatus.REGISTERING:
                        switch (cachedPlan.JoinMethod)
                        {
                            case JoinMethod.NONE:
                                context.AddFailure(AppMessage.ERR_PLAN_PRIVATE);
                                return;
                            case JoinMethod.INVITE:
                                if (!cachedPlan.Members.Any(m => m.Status == MemberStatus.INVITED))
                                {
                                    context.AddFailure(AppMessage.ERR_PLAN_NOT_INVITED);
                                    return;
                                }
                                break;
                        }
                        break;
                }
            });
            RuleFor(p => p.Companions).CustomAsync(async (companions, context, ct) =>
            {
                
                cachedPlan ??= await planService.GetAll(true)
                                                .Include(p => p.Members.Where(m => m.AccountId == accountId))
                                                .FirstOrDefaultAsync(p => p.Id == context.InstanceToValidate.PlanId, ct);
                if (cachedPlan == null)
                {
                    context.AddFailure(nameof(PlanJoin.PlanId),AppMessage.ERR_PLAN_NOT_FOUND);
                    return;
                }
                if (cachedPlan.MaxMemberCount == 1 && companions != null)
                {
                    context.AddFailure(AppMessage.ERR_PLAN_PERSONAL_JOIN);
                    return;
                }
                var weight = companions != null ? companions.Count + 1 : 1;
                if (weight > cachedPlan.MaxMemberWeight)
                {
                    context.AddFailure(AppMessage.ERR_PLAN_JOIN_WEIGHT);
                    return;
                }
                if (cachedPlan.MemberCount >= cachedPlan.MaxMemberCount)
                {
                    context.AddFailure(AppMessage.ERR_PLAN_JOIN_FULL);
                    return;
                }
                if (cachedPlan.MemberCount + weight > cachedPlan.MaxMemberCount)
                {
                    context.AddFailure(string.Format(AppMessage.ERR_PLAN_JOIN_MAX, cachedPlan.MaxMemberCount - cachedPlan.MemberCount));
                    return;
                }
                var account = await accountService.FindAsync(accountId) ?? throw new KeyNotFoundException(AppMessage.ERR_ACCOUNT_NOT_FOUND);
                if (account.GcoinBalance < cachedPlan.GcoinBudgetPerCapita * weight)
                {
                    context.AddFailure(AppMessage.ERR_BALANCE_NOT_ENOUGH);
                    return;
                };
            }).ForEach(c => c.Length(ValidationConstants.ACCOUNT_NAME_MIN_LENGTH,
                                     ValidationConstants.ACCOUNT_NAME_MAX_LENGTH)
                             .WithMessage(string.Format(AppMessage.ERR_PLAN_COMPANION_NAME_LENGTH,
                                                        ValidationConstants.ACCOUNT_NAME_MIN_LENGTH,
                                                        ValidationConstants.ACCOUNT_NAME_MAX_LENGTH)));
        }
    }
}
