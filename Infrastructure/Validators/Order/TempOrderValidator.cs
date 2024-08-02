using Domain.JsonEntities;
using FluentValidation;
using Infrastructure.Constants;

namespace Infrastructure.Validators.Order
{
    public class TempOrderValidator : AbstractValidator<TempOrder>
    {
        public TempOrderValidator()
        {
            RuleFor(o => o.Cart.Keys).Count(ValidationConstants.ORDER_ITEM_MIN_COUNT,
                                            ValidationConstants.ORDER_ITEM_MAX_COUNT)
                                     .WithMessage(string.Format(AppMessage.ERR_ORDER_PRODUCT_COUNT,
                                                                ValidationConstants.ORDER_ITEM_MIN_COUNT,
                                                                ValidationConstants.ORDER_ITEM_MAX_COUNT));
            //RuleFor(p => p.Cart).CustomAsync(async (cart, context, ct) =>
            //{
            //    var order = context.InstanceToValidate;
            //    var activeProducts = await productService.GetAll()
            //                                             .Include(p => p.Provider.Account)
            //                                             .Where(p => cart.Keys.Contains(p.Id))
            //                                             .ToListAsync(ct);
            //    if (activeProducts == null
            //        || activeProducts.Count != cart.Count
            //        || activeProducts.GroupBy(p => p.ProviderId).Count() != 1)
            //    {
            //        context.AddFailure(AppMessage.ERR_ORDER_PROVIDER_DIFF);
            //        return;
            //    }
            //    if (activeProducts.Any(p => !p.IsAvailable))
            //    {
            //        context.AddFailure(AppMessage.ERR_ORDER_PRODUCT_INACTIVE);
            //        return;
            //    }
            //    var eventType = (EventType)context.RootContextData[order.Guid.ToString()];
            //    switch (eventType)
            //    {
            //        case EventType.EAT:
            //            if (activeProducts.Any(p => !ValidationConstants.MEAL_PRODUCTS.Contains(p.Type)))
            //            {
            //                context.AddFailure(AppMessage.ERR_ORDER_PRODUCT_TYPE_MEAL);
            //                return;
            //            }
            //            if (activeProducts.Any(p => !p.Periods.Contains(order.Period)))
            //            {
            //                context.AddFailure(AppMessage.ERR_ORDER_PRODUCT_PERIOD);
            //                return;
            //            }
            //            break;
            //        case EventType.CHECKIN:
            //            if (activeProducts.Any(p => !ValidationConstants.LODGING_PRODUCTS.Contains(p.Type)))
            //            {
            //                context.AddFailure(AppMessage.ERR_ORDER_PRODUCT_TYPE_LODGING);
            //                return;
            //            }
            //            break;
            //        case EventType.VISIT:
            //            if (activeProducts.Any(p => !ValidationConstants.RIDING_PRODUCTS.Contains(p.Type)))
            //            {
            //                context.AddFailure(AppMessage.ERR_ORDER_PRODUCT_TYPE_RIDING);
            //                return;
            //            }
            //            break;
            //    }
            //    var destinationId = context.RootContextData[nameof(Domain.Entities.Plan.DestinationId)];
            //    var destination = await destinationService.FindAsync(destinationId);
            //    if (destination == null || !destination.IsVisible)
            //    {
            //        context.AddFailure(nameof(PlanCreate.DestinationId), AppMessage.ERR_DESTINATION_NOT_FOUND);
            //        return;
            //    }
            //    if (!activeProducts[0].Provider.Coordinate.IsWithinHaversineDistance(destination.Coordinate,
            //                                                                               ValidationConstants.PROVIDER_MAX_DISTANCE_METER_DIFF))
            //    {
            //        context.AddFailure(string.Format(AppMessage.ERR_PLAN_PROVIDER_DISTANCE,
            //                                         ValidationConstants.PROVIDER_MAX_DISTANCE_METER_DIFF));
            //        return;
            //    }
            //    var key = order.Guid.ToString();
            //    await cacheService.SetDataAsync(key, activeProducts, CacheConstants.DEFAULT_VALID_MINUTE);
            //});
            RuleFor(o => o.Period).IsInEnum().WithMessage(AppMessage.ERR_ORDER_PERIOD_INVALID);
            RuleFor(o => o.Note).MaximumLength(ValidationConstants.ORDER_NOTE_MAX_LENGTH)
                                .WithMessage(string.Format(AppMessage.ERR_ORDER_NOTE_LENGTH,
                                                           ValidationConstants.ORDER_NOTE_MAX_LENGTH))
                                .When(o => !string.IsNullOrEmpty(o.Note));
        }
    }
}
