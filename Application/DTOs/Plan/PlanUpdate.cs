using Domain.JsonEntities;
using NetTopologySuite.Geometries;

namespace Application.DTOs.Plan
{
#pragma warning disable CS8618
    public class PlanUpdate
    {
        public int PlanId { get; set; }
        public string Name { get; set; }
        public DateTimeOffset DepartAt { get; set; }
        public TimeSpan TravelDuration { get; set; }
        public int PeriodCount { get; set; }
        public string DepartureAddress { get; set; }
        public Coordinate Departure { get; set; }
        public List<List<Event>> Schedule { get; set; }
        public HashSet<int> SavedProviderIds { get; set; }
        public List<SurchargeCreate> Surcharges { get; set; }
        public string? Note { get; set; }
        public int MaxMemberCount { get; set; }
        public int MaxMemberWeight { get; set; }
    }
}
