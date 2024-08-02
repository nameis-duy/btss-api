using NetTopologySuite.Geometries;

namespace Application.DTOs.Traveler
{
#pragma warning disable CS8618
    public class TravelerUpdate
    {
        public string Name { get; set; }
        public bool IsMale { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Address { get; set; }
        public Coordinate? Coordinate { get; set; }
    }
}
