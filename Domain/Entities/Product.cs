using Domain.Enums.Provider;

namespace Domain.Entities
{
#pragma warning disable CS8618
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public ProductType Type { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public int PartySize { get; set; }
        public bool IsAvailable { get; set; } = true;
        public List<Period> Periods { get; set; }
        //many - one
        public int ProviderId { get; set; }
        public virtual Provider Provider { get; set; }
        //one - many
        public virtual HashSet<OrderDetail> OrderDetails { get; set; }
    }
}
