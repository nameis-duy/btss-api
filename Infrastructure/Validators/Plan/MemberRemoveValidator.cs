using Application.DTOs.Plan;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Plan;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators.Plan
{
    public class MemberRemoveValidator : AbstractValidator<MemberRemove>
    {
        public MemberRemoveValidator(IGenericService<PlanMember> planMemberService,
                                     IClaimService claimService)
        {
            RuleFor(m => m.PlanMemberId).CustomAsync(async (planMemberId, context, ct) =>
            {
                var planMember = await planMemberService.GetAll(true)
                                                        .Include(m => m.Plan.Account)
                                                        .Include(m => m.Account)
                                                        .FirstOrDefaultAsync(m => m.Id == planMemberId);
                if (planMember == null || planMember.Status != MemberStatus.JOINED)
                {
                    context.AddFailure(AppMessage.ERR_PLAN_MEMBER_NOT_FOUND);
                    return;
                }
                var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
                if (planMember.Plan.AccountId != accountId && planMember.AccountId != accountId)
                {
                    context.AddFailure(AppMessage.ERR_AUTHORIZE);
                    return;
                }
                if (planMember.Plan.AccountId == accountId)
                {
                    if (planMember.AccountId == accountId)
                    {
                        context.AddFailure(AppMessage.ERR_PLAN_REMOVE_HOST);
                        return;
                    } 
                    else if (planMember.Plan.Status != PlanStatus.REGISTERING)
                    {
                        context.AddFailure(AppMessage.ERR_PLAN_REMOVE_MEMBER_REGISTERING);
                        return;
                    }
                }
                else if (planMember.Plan.Status != PlanStatus.REGISTERING)
                {
                    context.AddFailure(AppMessage.ERR_PLAN_SELF_REMOVE_REGISTERING);
                    return;
                }
            });
        }
    }
}
