using Application.DTOs.Order;
using Application.Interfaces.Services;
using Domain.Enums.Others;
using Domain.Enums.Provider;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Infrastructure.Utilities;

namespace Infrastructure.Validators.Order
{
    public class OrderChangeStatusValidator : AbstractValidator<OrderChangeStatus>
    {
        public OrderChangeStatusValidator(IOrderService orderService,
                                          IClaimService claimService,
                                          ITimeService timeService)
        {
            RuleFor(o => o.OrderId)
                .NotEmpty()
                .CustomAsync(async (id, context, ct) =>
                {
                    var dto = context.InstanceToValidate;
                    var order = await orderService.GetAll(true)
                                                  .Include(o => o.Provider.Account)
                                                  .Include(o => o.Plan)
                                                  .FirstOrDefaultAsync(o => o.Id == id, ct);
                    if (order is null)
                    {
                        context.AddFailure(AppMessage.ERR_ORDER_NOT_FOUND);
                        return;
                    }
                    var currentRole = claimService.GetClaim(ClaimTypes.Role, Role.STAFF);
                    switch (currentRole)
                    {
                        case Role.PROVIDER:
                            var providerId = claimService.GetClaim(ClaimConstants.PROVIDER_ID, -1);
                            if (order.ProviderId != providerId) context.AddFailure(AppMessage.ERR_AUTHORIZE);
                            return;
                        case Role.STAFF:
                            if (order.Provider.Account is not null) context.AddFailure(AppMessage.ERR_AUTHORIZE);
                            return;
                    }
                    switch (order.CurrentStatus)
                    {
                        case OrderStatus.RESERVED:
                            if (dto.Status != OrderStatus.PREPARED)
                            {
                                context.AddFailure(AppMessage.ERR_ORDER_STATUS_INVALID);
                                return;
                            }
                            break;
                        case OrderStatus.PREPARED:
                            if (dto.Status != OrderStatus.SERVED)
                            {
                                context.AddFailure(AppMessage.ERR_ORDER_STATUS_INVALID);
                                return;
                            }
                            var now = timeService.Now;
                            var lastServeDate = order.ServeDates.Max();
                            var endTimeOfDay = order.Period.GetEndTimeOfDay();
                            var unlockAt = lastServeDate.ToDateTime(TimeOnly.MinValue).Add(endTimeOfDay);
                            if (now.Add(order.Plan.Offset) < unlockAt)
                            {
                                context.AddFailure(string.Format(AppMessage.ERR_ORDER_SERVE_INVALID, $"{unlockAt:dd/MM/yy HH:mm}"));
                                return;
                            }
                            break;
                        default:
                            context.AddFailure(AppMessage.ERR_ORDER_STATUS_INVALID);
                            return;
                    }
                });
        }
    }
}
