using Application.DTOs.Order;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IOrderService : IGenericService<Order>
    {
        Task<Order> CreateOrderAsync(OrderCreate dto);
        IQueryable<Order> GetOrders();
        Task<Order> ChangeStatusAsync(OrderChangeStatus dto);
        Task<Order> CancelOrderAsync(OrderCancel dto);
        Task<bool> GetOrderCancelOTPAsync(OrderCancelOTP dto);
    }
}
