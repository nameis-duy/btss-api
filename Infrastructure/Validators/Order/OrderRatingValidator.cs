using Application.DTOs.Order;
using Application.Interfaces.Services;
using Domain.Enums.Provider;
using FluentValidation;
using Infrastructure.Constants;

namespace Infrastructure.Validators.Order
{
    public class OrderRatingValidator : AbstractValidator<OrderRating>
    {
        public OrderRatingValidator(IOrderService orderService, IClaimService claimService, ITimeService timeService)
        {
            RuleFor(o => o.OrderId).CustomAsync(async (id, context, ct) =>
            {
                var order = await orderService.FindAsync(id);
                if (order == null)
                {
                    context.AddFailure(AppMessage.ERR_ORDER_NOT_FOUND);
                    return;
                }
                var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
                if (order.AccountId != accountId)
                {
                    context.AddFailure(AppMessage.ERR_AUTHORIZE);
                    return;
                }
                switch (order.CurrentStatus)
                {
                    case var t when t < OrderStatus.SERVED:
                        context.AddFailure(AppMessage.ERR_ORDER_RATING_NOT_SERVED);
                        return;
                    case OrderStatus.CANCELLED:
                        context.AddFailure(AppMessage.ERR_ORDER_RATING_CANCELED);
                        return;
                    case OrderStatus.FINISHED:
                        context.AddFailure(AppMessage.ERR_ORDER_RATING_FINISHED);
                        return;
                }
                if (order.Rating.HasValue)
                {
                    context.AddFailure(AppMessage.ERR_ORDER_RATING_RATED);
                    return;
                }
                var now = timeService.Now;
                
            });
            RuleFor(o => o.Rating).InclusiveBetween(ValidationConstants.ORDER_MIN_RATING,
                                                    ValidationConstants.ORDER_MAX_RATING)
                                  .WithMessage(string.Format(AppMessage.ERR_ORDER_RATING_VALUE,
                                               ValidationConstants.ORDER_MIN_RATING,
                                               ValidationConstants.ORDER_MAX_RATING));
            RuleFor(o => o.Comment).NotEmpty()
                                   .Length(ValidationConstants.ORDER_COMMENT_MIN_LENGTH,
                                           ValidationConstants.ORDER_COMMENT_MAX_LENGTH)
                                   .WithMessage(string.Format(AppMessage.ERR_ORDER_COMMENT_LENGTH,
                                                              ValidationConstants.ORDER_COMMENT_MIN_LENGTH,
                                                              ValidationConstants.ORDER_COMMENT_MAX_LENGTH))
                                   .When(o => o.Rating < ValidationConstants.ORDER_MIN_RATING_NO_COMMENT);
        }
    }
}
