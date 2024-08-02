using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
#pragma warning disable CS8618
    public class StatisticalData
    {
        [Key]
        public string Key { get; set; }
        [Column(TypeName = "jsonb")]
        public string Value { get; set; }
    }
}
