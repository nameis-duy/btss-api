using Domain.Enums.Others;

namespace Application.DTOs.Order
{
    public class OrderCancelOTP
    {
        public int OrderId { get; set; }
        public MessageChannel Channel { get; set; }
    }
}
