namespace Infrastructure.Constants
{
    public class GlobalConstants
    {
        public const int VND_CONVERT_RATE = 1000;
        public const string DEFAULT_IP = "127.0.0.1";
        public const string REGION_GEOJSON_PATH = "region.geojson";
        public const string IMAGE_SOURCE = "https://d38ozmgi8b70tu.cloudfront.net/";
        public const string CONFIG_PATH = "appconfig.json";
        public const string IMAGE_THUMB_SIZE = "100x100";
        public const string DEFAULT_BRAND_NAME = "BTSS";
        public const int PLAN_REG_DATE_DIFF = 3;
        public const decimal BUDGET_ASSURANCE_RATE = 1.1m;
        public static readonly TimeSpan MORNING_START = TimeSpan.FromHours(6);
        public static readonly TimeSpan NOON_START = TimeSpan.FromHours(10);
        public static readonly TimeSpan AFTERNOON_START = TimeSpan.FromHours(14);
        public static readonly TimeSpan EVENING_START = TimeSpan.FromHours(18);
        public static readonly TimeSpan EVENING_END = TimeSpan.FromHours(22);
        public const int PRESTIGE_POINT_EARN_ON_PLAN_COMPLETE = 3;
    }
}
