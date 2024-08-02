using Domain.Enums.Others;

namespace Application.DTOs.Traveler
{
#pragma warning disable CS8618
    public class TravelerRequestOTP
    {
        public string Phone { get; set; }
        public MessageChannel Channel { get; set; }
    }
}
