using Domain.Enums.Provider;
using Infrastructure.Utilities;
using NetTopologySuite.Geometries;

namespace Infrastructure.Constants
{
    public class ValidationConstants
    {
        public const string TEST_ACCOUNT_PREFIX = "test-account";
        public readonly static Geometry REGION = GeoUtil.GetValidRegion();
        public const string PHONE_FORMAT = @"^84[0-9]{9}$";
        public const string IMAGE_SOURCE = "https://d38ozmgi8b70tu.cloudfront.net";
        public const int ACCOUNT_NAME_MIN_LENGTH = 4;
        public const int ACCOUNT_NAME_MAX_LENGTH = 30;
        public const int ACCOUNT_PWD_MIN_LENGTH = 8;
        public const int ACCOUNT_PWD_MAX_LENGTH = 20;
        public const int ADDRESS_MIN_LENGTH = 20;
        public const int ADDRESS_MAX_LENGTH = 120;
        public const int PLAN_NAME_MIN_LENGTH = 3;
        public const int PLAN_NAME_MAX_LENGTH = 30;
        public const int PLAN_GROUP_MEMBER_MIN = 2;
        public const int PLAN_GROUP_MEMBER_MAX = 20;
        public const int PLAN_PERIOD_MIN = 2;
        public const int PLAN_PERIOD_MAX = 30;
        public static readonly TimeSpan HALF_DAY_LENGTH = TimeSpan.FromHours(8);
        public static readonly TimeSpan FULL_DAY_LENGTH = TimeSpan.FromHours(16);
        public static readonly TimeSpan HALF_EVENING = TimeSpan.FromHours(20);
        public static readonly TimeSpan HALF_AFTERNOON = TimeSpan.FromHours(16);
        public const int EVENT_SHORT_MIN_LENGTH = 1;
        public const int EVENT_SHORT_MAX_LENGTH = 40;
        public const int EVENT_DESCRIPTION_MIN_LENGTH = 1;
        public const int EVENT_DESCRIPTION_MAX_LENGTH = 300;
        public static readonly TimeSpan EVENT_MIN_DURATION = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan EVENT_MAX_DURATION = FULL_DAY_LENGTH;
        public const decimal PLAN_SURCHARGE_MIN = 10000;
        public const decimal PLAN_SURCHARGE_MAX = 10000000;
        public const int PLAN_SURCHARGE_NOTE_MIN_LENGTH = 2;
        public const int PLAN_SURCHARGE_NOTE_MAX_LENGTH = 40;
        public const int ORDER_NOTE_MAX_LENGTH = 110;
        public const int DESTINATION_NAME_MIN_LENGTH = 6;
        public const int DESTINATION_NAME_MAX_LENGTH = 30;
        public const int DESTINATION_DESCRIPTION_MIN_LENGTH = 100;
        public const int DESTINATION_DESCRIPTION_MAX_LENGTH = 999;
        public const int DESTINATION_IMAGE_MIN = 1;
        public const int DESTINATION_IMAGE_MAX = 5;
        public const int PROVIDER_NAME_MIN_LENGTH = 6;
        public const int PROVIDER_NAME_MAX_LENGTH = 40;
        public const int PROVIDER_STANDARD_MIN = 1;
        public const int PROVIDER_STANDARD_MAX = 5;
        public static readonly ProviderType[] REQUIRE_STANDARD_TYPE = [ProviderType.RESTAURANT,
                                                                      ProviderType.HOTEL];
        public const int PRODUCT_NAME_MIN_LENGTH = 3;
        public const int PRODUCT_NAME_MAX_LENGTH = 30;
        public const decimal PRODUCT_PRICE_MIN = 10000;
        public const decimal PRODUCT_PRICE_MAX = 10000000;
        public const int PRODUCT_PARTYSIZE_MIN = 1;
        public const int PRODUCT_PARTYSIZE_MAX = 10;
        public const int PRODUCT_DESCRIPTION_MIN_LENGTH = 3;
        public const int PRODUCT_DESCRIPTION_MAX_LENGTH = 100;
        public static readonly ProviderType[] NO_PRODUCT_TYPE = [ProviderType.EMERGENCY,
                                                                ProviderType.REPAIR, 
                                                                ProviderType.GROCERY];
        public const int PLAN_PERSONAL_DEPART_DATE_MIN_DIFF = 1;
        public const int PLAN_DEPART_DATE_MIN_DIFF = 7;
        public const int PLAN_DEPART_DATE_MAX_DIFF = 30;
        public const int PLAN_CONTACT_MIN_COUNT = 1;
        public const int PLAN_CONTACT_MAX_COUNT = 5;
        public const int ORDER_ITEM_MIN_COUNT = 1;
        public const int ORDER_ITEM_MAX_COUNT = 10;
        public static readonly ProductType[] MEAL_PRODUCTS = [ProductType.BEVERAGE, ProductType.FOOD];
        public static readonly ProductType[] LODGING_PRODUCTS = [ProductType.ROOM, ProductType.CAMP];
        public static readonly ProductType[] RIDING_PRODUCTS = [ProductType.VEHICLE];
        public const int PLAN_SURCHARGE_MAX_COUNT = 10;
        public const int PLAN_NOTE_MAX_LENGTH = 999;
        public const int PLAN_MEMBER_WEIGHT_MIN = 1;
        public const int PLAN_MEMBER_FIXED_WEIGHT = 1; 
        public const double PLAN_MEMBER_WEIGHT_PCT = 0.334;
        public const int PLAN_MEMBER_WEIGHT_CAL_FLOOR = 6;
        public const int PLAN_VERIFY_MAX_METER_RADIUS = 10000;
        public const int PROVIDER_MAX_DISTANCE_METER_DIFF = 10000;
        public const int EMERGENCY_MAX_DISTANCE_METER_DIFF = 30000;
        public const int ORDER_COMMENT_MAX_LENGTH = 300;
        public const int ORDER_COMMENT_MIN_LENGTH = 10;
        public const int DESTINATION_COMMENT_MIN_LENGTH = 3;
        public const int DESTINATION_COMMENT_MAX_LENGTH = 300;
        public const int ORDER_MIN_RATING = 1;
        public const int ORDER_MAX_RATING = 5;
        public const int ORDER_MIN_RATING_NO_COMMENT = 4;
        //Config validate
        public const int MIN_DEFAULT_PRESTIGE_POINT = 0;
        public const int MAX_DEFAULT_PRESTIGE_POINT = 100;
        public const int MIN_TOPUP_VALID_VALUE = 0;
        public const int MAX_TOPUP_VALID_VALUE = 1000000;
        public const int MIN_BUDGET_ASSURED_PCT = 0;
        public const int MAX_BUDGET_ASSURED_PCT = 100;
        public const int MIN_HOLIDAY_MEAL_UP_PCT = 0;
        public const int MAX_HOLIDAY_MEAL_UP_PCT = 100;
        public const int MIN_HOLIDAY_LODGING_UP_PCT = 0;
        public const int MAX_HOLIDAY_LODGING_UP_PCT = 100;
        public const int MIN_HOLIDAY_RIDING_UP_PCT = 0;
        public const int MAX_HOLIDAY_RIDING_UP_PCT = 100;
        public const int MIN_ORDER_DATE_MIN_DIFF = 0;
        public const int MAX_ORDER_DATE_MIN_DIFF = 100;
    }
}
