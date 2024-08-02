namespace Domain.Entities
{
#pragma warning disable CS8618
    public class DestinationComment
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        //many - one
        public int AccountId { get; set; }
        public virtual Account Account { get; set; }
        public int DestinationId { get; set; }
        public virtual Destination Destination { get; set; }
    }
}
