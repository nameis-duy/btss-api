using Domain.Enums.Others;

namespace Application.DTOs.Order
{
#pragma warning disable CS8618
    public class OrderCancel
    {
        public int OrderId { get; set; }
        public string Reason { get; set; }
        public string? OTP { get; set; }
        public MessageChannel? Channel { get; set; }
    }
}
