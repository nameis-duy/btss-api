using Domain.Enums.Provider;
using Infrastructure.Constants;

namespace Infrastructure.Utilities
{
    public static class TimeSpanUtil
    {
        public static Period GetPeriod(this TimeSpan timeOfDay)
        {
            if (timeOfDay > TimeSpan.FromDays(1)) throw new ArgumentException("TimeSpan value > 1 day, cannot determine");
            if (timeOfDay < TimeSpan.Zero) throw new ArgumentException("TimeSpan value < 0, cannot determine");
            return timeOfDay switch
            {
                var t when TimeSpan.Zero <= t && t < GlobalConstants.NOON_START => Period.MORNING,
                var t when GlobalConstants.NOON_START <= t && t < GlobalConstants.AFTERNOON_START => Period.NOON,
                var t when GlobalConstants.AFTERNOON_START <= t && t < GlobalConstants.EVENING_START => Period.AFTERNOON,
                _ => Period.EVENING,
            };
        }
        public static TimeSpan GetEndTimeOfDay(this Period period)
        {
            return period switch
            {
                Period.MORNING => GlobalConstants.NOON_START,
                Period.NOON => GlobalConstants.AFTERNOON_START,
                Period.AFTERNOON => GlobalConstants.EVENING_START,
                _ => GlobalConstants.EVENING_END
            };
        }
    }
}
