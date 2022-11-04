using Constellation.Application.Models.Auth;
using Hangfire.Dashboard;

namespace Constellation.Presentation.Server.Infrastructure
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return context.GetHttpContext().User.IsInRole(AuthRoles.Admin);
        }
    }
}
