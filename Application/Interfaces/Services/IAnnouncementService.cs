using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IAnnouncementService : IGenericService<Announcement>
    {
        IQueryable<Announcement> GetAnnouncements();
        Task PushToDevicesAsync(Announcement announcement, params string?[] deviceTokens);
    }
}
