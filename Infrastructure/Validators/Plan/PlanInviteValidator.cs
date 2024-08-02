using Application.DTOs.Plan;
using Application.Interfaces.Services;
using Domain.Enums.Plan;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators.Plan
{
    public class PlanInviteValidator : AbstractValidator<PlanInvite>
    {
        public PlanInviteValidator(IPlanService planService,
                                   IAccountService accountService,
                                   IClaimService claimService)
        {
            RuleFor(i => i.PlanId).CustomAsync(async (planId, context, ct) =>
            {
                var inviteeId = context.InstanceToValidate.AccountId;
                var plan = await planService.GetAll(true)
                                            .Include(p => p.Members.Where(m => m.AccountId == inviteeId))
                                            .ThenInclude(m => m.Account)
                                            .Include(p => p.Account)
                                            .FirstOrDefaultAsync(p => p.Id == planId, ct);
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
                    context.AddFailure(AppMessage.ERR_PLAN_INVITE_STATUS_REGISTERING);
                    return;
                }
                if (plan.JoinMethod == JoinMethod.NONE)
                {
                    context.AddFailure(AppMessage.ERR_PLAN_INVITE_METHOD);
                    return;
                }
                if (plan.Members.Any(m => m.Status == MemberStatus.JOINED))
                {
                    context.AddFailure(nameof(PlanInvite.AccountId),
                                       string.Format(AppMessage.ERR_PLAN_INVITE_JOINED,
                                                     plan.Members[0].Account.Name));
                    return;
                }
                if (plan.Members.Any(m => m.Status == MemberStatus.INVITED))
                {
                    context.AddFailure(nameof(PlanInvite.AccountId),
                                       string.Format(AppMessage.ERR_PLAN_INVITE_INVITED,
                                                     plan.Members[0].Account.Name));
                    return;
                }
                if (plan.Members.Any(m => m.Status == MemberStatus.SELF_BLOCKED))
                {
                    context.AddFailure(nameof(PlanInvite.AccountId),
                                       string.Format(AppMessage.ERR_PLAN_INVITE_SELF_BLOCKED,
                                                     plan.Members[0].Account.Name));
                    return;
                }
            });
            RuleFor(i => i.AccountId).MustAsync(async (invite, accountId, ct) =>
            {
                return await accountService.GetAll().AnyAsync(a => a.Id == accountId && a.IsActive);
            }).WithMessage(AppMessage.ERR_ACCOUNT_NOT_FOUND);
        }
    }
}
