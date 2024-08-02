using Domain.Enums.Plan;
using Domain.Enums.Provider;

namespace Application.DTOs.Order
{
#pragma warning disable CS8618
    public class OrderCreate
    {
        public int PlanId { get; set; }
        public Dictionary<int, int> Cart { get; set; }
        public EventType Type { get; set; }
        public Period Period { get; set; }
        public string? Note { get; set; }
        public HashSet<DateOnly> ServeDates { get; set; }
    }
}
