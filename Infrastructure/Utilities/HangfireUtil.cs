using Hangfire;

namespace Infrastructure.Utilities
{
    public static class HangfireUtil
    {
        public static string HourInterval(int hour)
        {
            return $"0 */{hour} * * *";
        }
        public static string DayInterval(int day, int startHour = 0)
        {
            return $"0 {startHour} */{day} * *";
        }
    }
}
