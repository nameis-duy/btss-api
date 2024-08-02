using Domain.Enums.Provider;
using NetTopologySuite.Geometries;

namespace Application.DTOs.Provider
{
#pragma warning disable CS8618
    public class ProviderCreate
    {
        public string Name { get; set; }
        public ProviderType Type { get; set; }
        public int? Standard { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public Coordinate Coordinate { get; set; }
        public string ImageUrl { get; set; }
    }
}
