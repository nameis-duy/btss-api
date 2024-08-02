using AppAny.HotChocolate.FluentValidation;
using Application.DTOs.Order;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using HotChocolate.Authorization;
using Infrastructure.Validators.Order;

namespace API.GraphQL.Mutations
{
    public partial class Mutation
    {
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<Order> CreateOrderAsync([Service] IOrderService orderService,
                                                  [UseFluentValidation, UseValidator<OrderCreateValidator>] OrderCreate dto)
        {
            return await orderService.CreateOrderAsync(dto);
        }

        [Authorize(Roles = [nameof(Role.STAFF), nameof(Role.PROVIDER)])]
        public async Task<Order> ChangeOrderStatusAsync([Service] IOrderService orderService,
                                                        [UseFluentValidation, UseValidator<OrderChangeStatusValidator>] OrderChangeStatus dto)
        {
            return await orderService.ChangeStatusAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.STAFF), nameof(Role.PROVIDER)])]
        public async Task<bool> GetOrderCancelOTPAsync([Service] IOrderService orderService, [UseFluentValidation, UseValidator<OrderCancelOTPValidator>] OrderCancelOTP dto)
        {
            return await orderService.GetOrderCancelOTPAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER), nameof(Role.STAFF), nameof(Role.PROVIDER)])]
        public async Task<Order> CancelOrderAsync([Service] IOrderService orderService,
                                                  [UseFluentValidation, UseValidator<OrderCancelValidator>] OrderCancel dto)
        {
            return await orderService.CancelOrderAsync(dto);
        }
    }
}
