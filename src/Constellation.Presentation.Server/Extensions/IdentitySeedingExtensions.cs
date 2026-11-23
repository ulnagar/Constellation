namespace Constellation.Presentation.Server.Extensions;

using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.DependencyInjection;
using Core.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Serilog;

public static class IdentitySeedingExtensions
{
    public static async Task SeedIdentityAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILogger>();

        try
        {
            RoleManager<AppRole> roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<AppRole>>();
            UserManager<AppUser> userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<AppUser>>();

            await IdentityDefaults.SeedRoles(roleManager);
            await IdentityDefaults.SeedUsers(userManager);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred while seeding identity.");
        }
    }
}