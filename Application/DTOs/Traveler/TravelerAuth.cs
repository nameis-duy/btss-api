using Domain.Enums.Others;

namespace Application.DTOs.Traveler
{
#pragma warning disable CS8618
    public class TravelerAuth
    {
        public MessageChannel Channel { get; set; }
        public string Phone { get; set; }
        public string OTP { get; set; }
        public string DeviceToken { get; set; }
    }
}
