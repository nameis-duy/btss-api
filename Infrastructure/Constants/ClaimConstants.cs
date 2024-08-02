using System.Security.Claims;

namespace Infrastructure.Constants
{
    public partial class ClaimConstants
    {
        public const string ID = "id";
        public const string REG_DATE = "reg_date";
        public const string PROVIDER_ID = "provider_id";
        public const string PHONE = ClaimTypes.MobilePhone;
        public const string ROLE = ClaimTypes.Role;
    }
}
