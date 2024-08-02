namespace Domain.Entities
{
#pragma warning disable CS8618
    public class OrderDetail
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
        //many - one
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
