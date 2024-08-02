using Domain.Enums.Provider;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

namespace Domain.JsonEntities
{
#pragma warning disable CS8618
    public class Contact
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string? Address { get; set; }
        [JsonConverter(typeof(GeometryConverter))]
        public Point? Coordinate { get; set; }
        public ProviderType Type { get; set; }
        public string? ImagePath { get; set; }
    }
}
