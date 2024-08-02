namespace Application.DTOs.Generic
{
#pragma warning disable CS8618
    public class AppConfig
    {
        public bool USE_FIXED_OTP { get; set; }
        public int DEFAULT_PRESTIGE_POINT { get; set; }
        public long MIN_TOPUP { get; set; }
        public long MAX_TOPUP { get; set; }
        public int BUDGET_ASSURED_PCT { get; set; }
        public int HOLIDAY_MEAL_UP_PCT { get; set; }
        public int HOLIDAY_LODGING_UP_PCT { get; set; }
        public int HOLIDAY_RIDING_UP_PCT { get; set; }
        public int ORDER_DATE_MIN_DIFF { get; set; }
        public int ORDER_CANCEL_DATE_DURATION { get; set; }
        public int MEMBER_REFUND_SELF_REMOVE_1_DAY_PCT { get; set; }
        public int ORDER_REFUND_CUSTOMER_CANCEL_1_DAY_PCT { get; set; }
        public int ORDER_REFUND_CUSTOMER_CANCEL_2_DAY_PCT { get; set; }
        public int PRODUCT_MAX_PRICE_UP_PCT { get; set; }
        public List<Holiday> HOLIDAYS {  get; set; }
    }
}
