using Application.DTOs.Plan;
using Application.Interfaces.Services;
using Domain.Enums.Plan;
using FluentValidation;
using Infrastructure.Constants;
using Infrastructure.Utilities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Infrastructure.Validators.Plan
{
    public class PlanVerifyValidator : AbstractValidator<PlanVerify>
    {
        public PlanVerifyValidator(IPlanService planService,
                                   IClaimService claimService,
                                   ITimeService timeService)
        {
            RuleFor(v => v.PlanId).CustomAsync(async (id, context, ct) =>
            {
                var plan = await planService.GetAll(true)
                                            .Include(p => p.Destination)
                                            .FirstOrDefaultAsync(p => p.Id == id);
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
                switch (plan.Status)
                {
                    case PlanStatus.PENDING:
                    case PlanStatus.REGISTERING:
                        context.AddFailure(AppMessage.ERR_PLAN_VERIFY_PRE_READY);
                        return;
                    case PlanStatus.READY:
                        var arrivedAt = plan.UtcDepartAt + plan.TravelDuration;
                        if (arrivedAt > timeService.Now)
                        {
                            var unlockTime = arrivedAt + plan.Offset;
                            context.AddFailure(string.Format(AppMessage.ERR_PLAN_VERIFY_DEPART_TIME,
                                                             $"{unlockTime:HH\\:mm dd/MM/yy}"));
                            return;
                        }
                        return;
                    case PlanStatus.VERIFIED:
                    case PlanStatus.COMPLETED:
                        context.AddFailure(AppMessage.ERR_PLAN_VERIFY_VERIFIED);
                        return;
                    
                    case PlanStatus.FLAWED:
                        context.AddFailure(AppMessage.ERR_PLAN_VERIFY_FLAWED);
                        return;
                    case PlanStatus.CANCELED:
                        context.AddFailure(AppMessage.ERR_PLAN_VERIFY_CANCELED);
                        return;
                }
                var position = context.InstanceToValidate.Coordinate.Adapt<Point>();
                var distance = position.HaversineDistance(plan.Destination.Coordinate);
                if (!position.IsWithinHaversineDistance(plan.Destination.Coordinate,
                                                        ValidationConstants.PLAN_VERIFY_MAX_METER_RADIUS))
                {
                    
                    context.AddFailure(nameof(PlanVerify.Coordinate),AppMessage.ERR_PLAN_VERIFY_OUT_RANGE);
                    return;
                }
            });
        }
    }
}
