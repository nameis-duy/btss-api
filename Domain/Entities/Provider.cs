using Domain.Enums.Provider;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
#pragma warning disable CS8618
    public class Provider
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ProviderType Type { get; set; }
        public int? Standard { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        
        [Column(TypeName = "geography (point)")]
        [JsonConverter(typeof(GeometryConverter))]
        public Point Coordinate { get; set; }
        public string ImagePath { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; } = true;
        //one - many
        public virtual HashSet<Product> Products { get; set; }
        public virtual HashSet<Order>? Orders { get; set; }
        public virtual HashSet<Transaction> Transactions { get; set; }
        //one - one
        public virtual Account? Account { get; set; }
    }
}
