using Domain.Enums.Destination;
using Domain.JsonEntities;
using NetTopologySuite.Geometries;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
#pragma warning disable CS8618
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UnaccentName { get; set; }
        public string Description { get; set; }
        public List<string> ImagePaths { get; set; }
        public string Address { get; set; }
        [Column(TypeName = "geography (point)")]
        public Point Coordinate { get; set; }
        public List<Season> Seasons { get; set; }
        public Topographic Topographic { get; set; }
        public List<Activity> Activities { get; set; }
        public int? Rating { get; set; }
        public bool IsVisible { get; set; } = true;
        //search vector
        public NpgsqlTsVector NameVector { get; set; }
        //many - one
        public int ProvinceId { get; set; }
        public virtual Province Province { get; set; }
        //one - many
        public virtual HashSet<Plan> Plans { get; set; }
        public virtual HashSet<DestinationComment> Comments { get; set; }
    }
}
