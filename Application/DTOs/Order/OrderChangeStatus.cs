using Domain.Enums.Provider;

namespace Application.DTOs.Order
{
    public class OrderChangeStatus
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
    }
}
