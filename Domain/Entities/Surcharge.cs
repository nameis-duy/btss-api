namespace Domain.Entities
{
#pragma warning disable CS8618
    public class Surcharge
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public decimal Amount { get; set; }
        public bool AlreadyDivided { get; set; }
        public string? ImagePath { get; set; }
        //many - one
        public int PlanId { get; set; }
        public virtual Plan Plan { get; set; }
    }
}
