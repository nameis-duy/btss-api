using Domain.Enums.Destination;
using NetTopologySuite.Geometries;

namespace Application.DTOs.Destination
{
#pragma warning disable CS8618
    public class DestinationUpdate : DestinationCreate
    {
        public int DestinationId { get; set; }
    }
}
