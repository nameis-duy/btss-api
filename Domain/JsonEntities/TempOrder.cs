using Domain.Enums.Provider;

namespace Domain.JsonEntities
{
#pragma warning disable CS8618
    public class TempOrder
    {
        public int ProviderId { get; set; }
        public Dictionary<int, int> Cart { get; set; }
        public Period Period { get; set; }
        public string? Note { get; set; }
        public decimal Total { get; set; }
    }
}
