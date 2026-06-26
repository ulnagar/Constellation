namespace Constellation.Presentation.Server.Extensions;

using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Identity;

public static class IdentitySeedingExtensions
{
    public static async Task SeedIdentityAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        try
        {
            RoleManager<AppRole> roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<AppRole>>();
            await IdentityDefaults.SeedRoles(roleManager);
        }
        catch
        {
            // ignored
        }
    }
}