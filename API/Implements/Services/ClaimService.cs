using Application.Interfaces.Services;
using Infrastructure.Constants;

namespace API.Implements.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IHttpContextAccessor accessor;

        public ClaimService(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public T GetClaim<T>(string claimType, T defaultValue)
        {
            try
            {
                var claim = accessor.HttpContext!.User.FindFirst(claimType);
                if (claim == null) return defaultValue;
                var type = typeof(T);
                if (type.IsEnum) return (T)Enum.Parse(type, claim.Value);
                return (T)Convert.ChangeType(claim.Value, type);
            }
            catch
            {
                return defaultValue;
            }

        }

        public string GetIpAddress()
        {
            var ip = accessor.HttpContext!.Connection.RemoteIpAddress;
            return ip == null ? GlobalConstants.DEFAULT_IP : ip.ToString();
        }

        public string GetUniqueRequestId()
        {
            return accessor.HttpContext!.TraceIdentifier;
        }
    }
}
