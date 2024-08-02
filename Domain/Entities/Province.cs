using Domain.Enums.Destination;

namespace Domain.Entities
{
#pragma warning disable CS8618
    public class Province
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public Region Region { get; set; }
        //one - many
        public virtual HashSet<Destination> Destinations { get; set; }
    }
}
