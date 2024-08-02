using Domain.Enums.Plan;
using Domain.JsonEntities;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
#pragma warning disable CS8618
    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PlanStatus Status { get; set; }
        public JoinMethod JoinMethod { get; set; }
        public DateTime? UtcRegCloseAt { get; set; }
        public DateTime UtcDepartAt { get; set; }
        public DateTime UtcStartAt { get; set; }
        public DateTime UtcEndAt { get; set; }
        public TimeSpan Offset { get; set; }
        public TimeSpan TravelDuration { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int PeriodCount { get; set; }
        public string DepartureAddress { get; set; }
        [Column(TypeName = "geography (point)")]
        public Point Departure { get; set; }
        //json
        [Column(TypeName = "jsonb")]
        public string Schedule { get; set; }
        //json
        public string? Note { get; set; }
        public int MemberCount { get; set; }
        public int MaxMemberCount { get; set; }
        public int MaxMemberWeight { get; set; }
        public decimal GcoinBudgetPerCapita { get; set; }
        public decimal DisplayGcoinBudget { get; set; }
        public decimal ActualGcoinBudget { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublished { get; set; }
        //many - one
        public int AccountId { get; set; }
        public virtual Account Account { get; set; }
        public int DestinationId { get; set; }
        public virtual Destination Destination { get; set; }
        [ForeignKey(nameof(SourceId))]
        public int? SourceId { get; set; }
        public virtual Plan? Source { get; set; }
        //one - many
        public virtual List<PlanMember> Members { get; set; }
        public virtual HashSet<Order> Orders { get; set; }
        public virtual HashSet<Plan> Copies { get; set; }
        public virtual HashSet<Surcharge> Surcharges { get; set; }
        public virtual HashSet<Announcement> Announcements { get; set; }
        public virtual HashSet<PlanSavedProvider> SavedProviders { get; set; }
        //public HashSet<PlanRating>? Ratings { get; set; }
    }
}
