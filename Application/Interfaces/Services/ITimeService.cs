namespace Application.Interfaces.Services
{
    public interface ITimeService
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
        TimeSpan AdditionalSpan { get; }
        void SetAdditionalTimeSpan(TimeSpan span);
    }
}
