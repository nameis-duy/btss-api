using Domain.Enums.Others;

namespace Infrastructure.Constants
{
    public class AuthConstants
    {
        public const OTPLength OTP_LENGTH = OTPLength.SIX;
        public const int ACCESS_TOKEN_LIFETIME_MINUTE = 60 * 24;
        public const int REFRESH_TOKEN_LIFETIME_MINUTE = 60 * 24 * 30;
        public const int TEST_AUTHORIZE_OTP = 123123;
        public const int TEST_CANCEL_ORDER_OTP = 456456;
        public const bool REFRESH_USE_HMACSHA512 = true;
    }
}
