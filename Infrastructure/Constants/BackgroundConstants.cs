using Infrastructure.Implements.Services;

namespace Infrastructure.Constants
{
    public class BackgroundConstants
    {
        public const int HOURS_NEXT_CANCEL = 8;
        public const int HOURS_NOTIFY_PLAN_CANCEL = 4;
        public const int DAYS_NOTIFY_PLAN_DEPART = 1;
        public const int MAX_DATE_NOTIFY_DEPART = 3;
        public const string VERIFY_PLAN_COMPLETE_RECURRING_JOB = "VERIFY_PLAN_COMPLETE";
        public const int DAYS_VERIFY_PLAN_COMPLETE = 1;
        public const int DAYS_FINISH_ORDER = 7;
        public const int CANCEL_PLAN_DAYS_AFTER = 1;
        public const int DAYS_FINISH_PLAN = 3;
        public const int MONTHS_UPDATE_PRODUCT = 1;
        public const int TRENDING_DESTINATIONS_CALCULATE_DAY_DURATION = 30;
        public const string TRENDING_DESTINATIONS = "TRENDING_DESTINATIONS";
    }
}
