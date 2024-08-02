using Domain.Enums.Plan;
using Domain.Enums.Provider;
using Domain.JsonEntities;

namespace Domain.Entities
{
#pragma warning disable CS8618
    public class Order
    {
        public int Id { get; set; }
        public List<DateOnly> ServeDates { get; set; }
        public EventType Type { get; set; }
        public Period Period { get; set; }
        public string? Note { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public decimal Deposit { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus CurrentStatus { get; set; }
        //json
        public List<OrderTrace> Traces { get; set; }
        //many - one
        public int PlanId { get; set; }
        public virtual Plan Plan { get; set; }
        public int AccountId { get; set; }
        public virtual Account Account { get; set; }
        public int ProviderId { get; set; }
        public virtual Provider Provider { get; set; }
        //one - many
        public virtual HashSet<OrderDetail> Details { get; set; }
        public virtual HashSet<Transaction> Transactions { get; set; }
        public virtual HashSet<Announcement> Announcements { get; set; }
    }
}
