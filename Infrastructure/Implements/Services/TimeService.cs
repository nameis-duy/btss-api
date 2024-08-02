using Application.Interfaces.Services;
namespace Infrastructure.Implements.Services
{
    public class TimeService : ITimeService
    {
        public DateTime Now {
            get
            {
                return UtcNow.Add(additionalSpan);
            }
        }
        public DateTime UtcNow => DateTime.UtcNow;
        public TimeSpan AdditionalSpan => additionalSpan;
        private TimeSpan additionalSpan = TimeSpan.Zero;

        public void SetAdditionalTimeSpan(TimeSpan span)
        {
            additionalSpan = span;
        }
    }
}
