using NetTopologySuite.Geometries;

namespace Application.DTOs.Plan
{
#pragma warning disable CS8618
    public class PlanVerify
    {
        public int PlanId { get; set; }
        public Coordinate Coordinate { get; set; }
    }
}
