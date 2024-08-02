using Domain.Enums.Destination;
using NetTopologySuite.Geometries;

namespace Application.DTOs.Destination
{
#pragma warning disable CS8618
    public class DestinationCreate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public HashSet<string> ImageUrls { get; set; }
        public string Address { get; set; }
        public Coordinate Coordinate { get; set; }
        public HashSet<Season> Seasons { get; set; }
        public Topographic Topographic { get; set; }
        public HashSet<Activity> Activities { get; set; }
        public int ProvinceId { get; set; }
    }
}
