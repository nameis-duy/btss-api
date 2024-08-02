using Vonage.Verify;

namespace Infrastructure.Utilities
{
    public static class VonageUtil
    {
        public static bool IsSuccessVerifyResponse(this VerifyResponse response)
        {
            return response.Status switch
            {
                "0" => true,
                _ => false,
            };
        }
        public static bool IsSuccessVerifyCheckResponse(this VerifyCheckResponse response)
        {
            return response.Status switch { "0" => true, _ => false, };
        }
    }
}
