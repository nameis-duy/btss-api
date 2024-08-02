namespace Application.DTOs.Staff_Admin
{
#pragma warning disable CS8618
    public class StaffCreate
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int? ProviderId { get; set; }
    }
}
