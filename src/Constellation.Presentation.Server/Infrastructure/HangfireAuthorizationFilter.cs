using Constellation.Application.Models.Identity;
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
