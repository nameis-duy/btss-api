namespace Application.DTOs.Traveler
{
#pragma warning disable CS8618
    public class TravelerCreate
    {
        public string Name { get; set; }
        public bool IsMale { get; set; }
        public string? AvatarUrl { get; set; }
        public string DeviceToken { get; set; }
    }
    
}
