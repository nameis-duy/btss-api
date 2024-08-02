namespace Application.DTOs.Plan
{
#pragma warning disable CS8618
    public class SurchargeCreate
    {
        public string Note { get; set; }
        public decimal Amount { get; set; }
        public bool AlreadyDivided { get; set; }
    }
}
