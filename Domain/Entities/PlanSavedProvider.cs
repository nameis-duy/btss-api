namespace Domain.Entities
{
#pragma warning disable CS8618
    public class PlanSavedProvider
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public Plan Plan { get; set; }
        public int ProviderId { get; set; }
        public Provider Provider { get; set; }
    }
}
