using Application.DTOs.Generic;
using Application.DTOs.Order;
using Application.DTOs.Plan;
using Application.Interfaces.Services;
using Domain.Enums.Plan;
using FluentValidation;
using Infrastructure.Constants;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Validators.Order
{
    public class OrderCreateValidator : AbstractValidator<OrderCreate>
    {
        public OrderCreateValidator(IPlanService planService,
                                    IClaimService claimService,
                                    ITimeService timeService,
                                    IDestinationService destinationService,
                                    IProductService productService,
                                    ICacheService cacheService,
                                    IOptionsSnapshot<AppConfig> snapshot)
        {
            RuleFor(o => o.PlanId).CustomAsync(async (planId, context, ct) =>
            {
                var plan = await planService.GetAll(true)
                                            .Include(p => p.Destination)
                                            .FirstOrDefaultAsync(p => p.Id == planId, ct);
                if (plan == null)
                {
                    context.AddFailure(AppMessage.ERR_PLAN_NOT_FOUND);
                    return;
                }
                context.RootContextData[nameof(PlanCreate.DestinationId)] = plan.DestinationId;
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
                        context.AddFailure(AppMessage.ERR_ORDER_PLAN_PRE_READY);
                        return;
                    case PlanStatus.FLAWED:
                    case PlanStatus.COMPLETED:
                        context.AddFailure(AppMessage.ERR_ORDER_PLAN_POST_READY);
                        return;
                    case PlanStatus.CANCELED:
                        context.AddFailure(AppMessage.ERR_ORDER_PLAN_CANCEL);
                        return;
                }
                var now = timeService.Now;
                if (plan.UtcDepartAt < now)
                {
                    context.AddFailure(AppMessage.ERR_ORDER_PLAN_STARTED);
                    return;
                }
                var order = context.InstanceToValidate;
                var minAt = now.AddDays(snapshot.Value.ORDER_DATE_MIN_DIFF);
                if (minAt > plan.UtcEndAt)
                {
                    context.AddFailure(string.Format(AppMessage.ERR_ORDER_OUT_OF_TIME, snapshot.Value.ORDER_DATE_MIN_DIFF));
                    return;
                }
                minAt = minAt> plan.UtcStartAt ? minAt : plan.UtcStartAt;
                var minDate = DateOnly.FromDateTime(minAt.Add(plan.Offset));
                if (order.ServeDates.Any(d => d < minDate || d > plan.EndDate))
                {
                    context.AddFailure($"{nameof(OrderCreate.ServeDates)}",
                                        string.Format(AppMessage.ERR_ORDER_SERVE_DATE,
                                                      $"{minDate:dd/MM/yy}",
                                                      $"{plan.EndDate:dd/MM/yy}"));
                    return;
                }
                if (order.ServeDates.Min() == plan.StartDate)
                {
                    var minPeriod = (plan.UtcStartAt + plan.Offset).TimeOfDay.GetPeriod();
                    if (order.Period < minPeriod)
                    {
                        context.AddFailure(AppMessage.ERR_EVENT_ORDER_PERIOD);
                        return;
                    }
                }
                if (order.ServeDates.Max() == plan.EndDate)
                {
                    var maxPeriod = (plan.UtcEndAt + plan.Offset).TimeOfDay.GetPeriod();
                    if (order.Period > maxPeriod)
                    {
                        context.AddFailure(AppMessage.ERR_EVENT_ORDER_PERIOD);
                        return;
                    }
                }
            });
            RuleFor(o => o.Cart.Keys).Count(ValidationConstants.ORDER_ITEM_MIN_COUNT,
                                            ValidationConstants.ORDER_ITEM_MAX_COUNT)
                                     .WithMessage(string.Format(AppMessage.ERR_ORDER_PRODUCT_COUNT,
                                                                ValidationConstants.ORDER_ITEM_MIN_COUNT,
                                                                ValidationConstants.ORDER_ITEM_MAX_COUNT));
            RuleFor(p => p.Cart).CustomAsync(async (cart, context, ct) =>
            {
                var order = context.InstanceToValidate;
                var activeProducts = await productService.GetAll()
                                                         .Include(p => p.Provider.Account)
                                                         .Where(p => cart.Keys.Contains(p.Id))
                                                         .ToListAsync(ct);
                if (activeProducts == null
                    || activeProducts.Count != cart.Count
                    || activeProducts.GroupBy(p => p.ProviderId).Count() != 1)
                {
                    context.AddFailure(AppMessage.ERR_ORDER_PROVIDER_DIFF);
                    return;
                }
                if (activeProducts.Any(p => !p.IsAvailable))
                {
                    context.AddFailure(AppMessage.ERR_ORDER_PRODUCT_INACTIVE);
                    return;
                }
                switch (order.Type)
                {
                    case EventType.EAT:
                        if (activeProducts.Any(p => !ValidationConstants.MEAL_PRODUCTS.Contains(p.Type)))
                        {
                            context.AddFailure(AppMessage.ERR_ORDER_PRODUCT_TYPE_MEAL);
                            return;
                        }
                        if (activeProducts.Any(p => !p.Periods.Contains(order.Period)))
                        {
                            context.AddFailure(AppMessage.ERR_ORDER_PRODUCT_PERIOD);
                            return;
                        }
                        break;
                    case EventType.CHECKIN:
                        if (activeProducts.Any(p => !ValidationConstants.LODGING_PRODUCTS.Contains(p.Type)))
                        {
                            context.AddFailure(AppMessage.ERR_ORDER_PRODUCT_TYPE_LODGING);
                            return;
                        }
                        break;
                    case EventType.VISIT:
                        if (activeProducts.Any(p => !ValidationConstants.RIDING_PRODUCTS.Contains(p.Type)))
                        {
                            context.AddFailure(AppMessage.ERR_ORDER_PRODUCT_TYPE_RIDING);
                            return;
                        }
                        break;
                    default:
                        context.AddFailure(nameof(OrderCreate.Type), AppMessage.ERR_ORDER_TYPE_INVALID);
                        return;
                }
                Domain.Entities.Destination? destination;
                if (context.RootContextData.TryGetValue(nameof(Domain.Entities.Plan.DestinationId), out var destinationId))
                {
                    destination = await destinationService.FindAsync(destinationId);
                } else
                {
                    var plan = await planService.GetAll(true)
                                            .Include(p => p.Destination)
                                            .FirstOrDefaultAsync(p => p.Id == order.PlanId, ct);
                    if (plan == null)
                    {
                        context.AddFailure(nameof(OrderCreate.PlanId), AppMessage.ERR_PLAN_NOT_FOUND);
                        return;
                    }
                    destination = plan.Destination;
                }
                if (destination == null || !destination.IsVisible)
                {
                    context.AddFailure(nameof(PlanCreate.DestinationId), AppMessage.ERR_DESTINATION_NOT_FOUND);
                    return;
                }
                if (!activeProducts[0].Provider.Coordinate.IsWithinHaversineDistance(destination.Coordinate,
                                                                                           ValidationConstants.PROVIDER_MAX_DISTANCE_METER_DIFF))
                {
                    context.AddFailure(string.Format(AppMessage.ERR_PLAN_PROVIDER_DISTANCE,
                                                     ValidationConstants.PROVIDER_MAX_DISTANCE_METER_DIFF));
                    return;
                }
                var key = string.Format(CacheConstants.ORDER_DETAILS_FORMAT, claimService.GetUniqueRequestId());
                await cacheService.SetDataAsync(key, activeProducts, CacheConstants.DEFAULT_VALID_MINUTE);
            });
            RuleFor(o => o.Period).IsInEnum().WithMessage(AppMessage.ERR_ORDER_PERIOD_INVALID);
            RuleFor(o => o.Note).MaximumLength(ValidationConstants.ORDER_NOTE_MAX_LENGTH)
                                .WithMessage(string.Format(AppMessage.ERR_ORDER_NOTE_LENGTH,
                                                           ValidationConstants.ORDER_NOTE_MAX_LENGTH))
                                .When(o => !string.IsNullOrEmpty(o.Note));
        }
    }
}
