using Domain.Enums.Others;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using System.Security.Claims;

namespace API
{
    public class HangfireAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = ((AspNetCoreDashboardContext)context).HttpContext;
            var currentRole = httpContext.User.FindFirst(ClaimTypes.Role.ToString());
            if (currentRole is null) return true;
            return currentRole.Value == nameof(Role.ADMIN);
        }
    }
}
