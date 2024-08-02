using Domain.Enums.Provider;

namespace Domain.JsonEntities
{
    public class OrderTrace
    {
        public OrderStatus Status { get; set; }
        public string? Description { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsClientAction { get; set; }
    }
}
