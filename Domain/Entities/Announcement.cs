using Domain.Enums.Announcement;

namespace Domain.Entities
{
#pragma warning disable CS8618
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string? ImageUrl { get; set; }
        public AnnouncementType Type { get; set; }
        public AnnouncementLevel Level { get; set; }
        public DateTime CreatedAt { get; set; }
        //many - one
        public int? AccountId { get; set; }
        public virtual Account? Account { get; set; }
        public int? ProviderId { get; set; }
        public virtual Provider? Provider { get; set; }
        public int? PlanId { get; set; }
        public virtual Plan? Plan { get; set; }
        public int? OrderId { get; set; }
        public virtual Order? Order { get; set; }
    }
}
