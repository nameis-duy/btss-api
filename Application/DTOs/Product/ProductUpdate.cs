using Domain.Enums.Provider;

namespace Application.DTOs.Product
{
#pragma warning disable CS8618
    public class ProductUpdate
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public ProductType Type { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int PartySize { get; set; }
        public List<Period> Periods { get; set; }
        public string Description { get; set; }
    }
}
