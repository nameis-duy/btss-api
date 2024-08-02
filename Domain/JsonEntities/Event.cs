using Domain.Enums.Plan;

namespace Domain.JsonEntities
{
#pragma warning disable CS8618
    public class Event
    {
        public EventType Type { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsStarred { get; set; }
        public TempOrder? TempOrder { get; set; }
    }
}
