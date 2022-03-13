using Constellation.Application;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.Persistence;
using Constellation.Infrastructure.Persistence.Repositories;
using Constellation.Infrastructure.Templates.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Constellation.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            });

            services.AddScoped<IAppDbContext, AppDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Scan(scan => scan
                .FromAssemblyOf<IApplicationService>()

                .AddClasses(classes => classes.AssignableTo<IScopedService>())
                .AsMatchingInterface()
                .WithScopedLifetime()

                .AddClasses(classes => classes.AssignableTo<ITransientService>())
                .AsMatchingInterface()
                .WithTransientLifetime()

                .AddClasses(classes => classes.AssignableTo<ISingletonService>())
                .AsMatchingInterface()
                .WithSingletonLifetime()
            );

            services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

            services.AddApplication();

            return services;
        }

        public static IServiceCollection AddStandardAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<AppUser, AppRole>()
                .AddUserManager<UserManager<AppUser>>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddSignInManager<SignInManager<AppUser>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.LoginPath = new PathString("/Admin/Login");
                options.LogoutPath = new PathString("/Admin/Logout");
            });

            return services;
        }

        public static IServiceCollection AddMainAppAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<AppUser, AppRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromHours(7);
                options.LoginPath = new PathString("/Admin/Login");
                options.LogoutPath = new PathString("/Admin/Logout");
            });

            var clientId = configuration["Authentication:Microsoft:ClientId"];
            var clientSecret = configuration["Authentication:Microsoft:ClientSecret"];

            if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret)) {
                services.AddAuthentication()
                    .AddMicrosoftAccount(options =>
                    {
                        options.ClientId = configuration["Authentication:Microsoft:ClientId"];
                        options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
                    });
            }

            return services;
        }

        public static IServiceCollection AddParentPortalAuthentication(this IServiceCollection services)
        {
            services.AddIdentity<AppUser, AppRole>()
                .AddUserManager<UserManager<AppUser>>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddSignInManager<SignInManager<AppUser>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.LoginPath = new PathString("/Portal/Parents/Identity/Login");
                options.LogoutPath = new PathString("/Portal/Parents/Identity/Logout");
            });

            return services;
        }

        public static IServiceCollection AddSchoolPortalAuthentication(this IServiceCollection services)
        {
            services.AddIdentity<AppUser, AppRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.LoginPath = new PathString("/Portal/School/Auth/Login");
                options.LogoutPath = new PathString("/Portal/School/Auth/LogOut");
            });

            return services;
        }
    }
}
