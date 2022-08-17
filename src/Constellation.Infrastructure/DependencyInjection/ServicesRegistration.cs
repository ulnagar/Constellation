using Constellation.Application;
using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.GatewayConfigurations;
using Constellation.Infrastructure.Gateways;
using Constellation.Infrastructure.Jobs;
using Constellation.Infrastructure.Persistence;
using Constellation.Infrastructure.Persistence.Repositories;
using Constellation.Infrastructure.Persistence.TrackIt;
using Constellation.Infrastructure.Services;
using Constellation.Infrastructure.Templates.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServicesRegistration
    {
        public static IServiceCollection AddHangfireJobs(this IServiceCollection services)
        {
            services.AddScoped<IAbsenceClassworkNotificationJob, AbsenceClassworkNotificationJob>();
            services.AddScoped<IAbsenceMonitorJob, AbsenceMonitorJob>();
            services.AddScoped<IAbsenceProcessingJob, AbsenceProcessingJob>();
            services.AddScoped<IAttendanceReportJob, AttendanceReportJob>();
            services.AddScoped<IClassMonitorJob, ClassMonitorJob>();
            services.AddScoped<ILessonNotificationsJob, LessonNotificationsJob>();
            services.AddScoped<IPermissionUpdateJob, PermissionUpdateJob>();
            services.AddScoped<IRollMarkingReportJob, RollMarkingReportJob>();
            services.AddScoped<ISchoolRegisterJob, SchoolRegisterJob>();
            services.AddScoped<ISentralAwardSyncJob, SentralAwardSyncJob>();
            services.AddScoped<ISentralFamilyDetailsSyncJob, SentralFamilyDetailsSyncJob>();
            services.AddScoped<ISentralPhotoSyncJob, SentralPhotoSyncJob>();
            services.AddScoped<ISentralReportSyncJob, SentralReportSyncJob>();
            services.AddScoped<IUserManagerJob, UserManagerJob>();

            return services;
        }

        public static IServiceCollection AddExternalServiceGateways(this IServiceCollection services)
        {
            services.AddTransient<IAdobeConnectGatewayConfiguration, AdobeConnectGatewayConfiguration>();
            services.AddScoped<IAdobeConnectGateway, AdobeConnectGateway>();

            services.AddTransient<ICanvasGatewayConfiguration, CanvasGatewayConfiguration>();
            services.AddScoped<ICanvasGateway, CanvasGateway>();

            services.AddTransient<ILinkShortenerGatewayConfiguration, LinkShortenerGatewayConfiguration>();
            services.AddScoped<ILinkShortenerGateway, LinkShortenerGateway>();

            services.AddTransient<INetworkStatisticsGatewayConfiguration, NetworkStatisticsGatewayConfiguration>();
            services.AddScoped<INetworkStatisticsGateway, NetworkStatisticsGateway>();

            services.AddTransient<ISentralGatewayConfiguration, SentralGatewayConfiguration>();
            services.AddScoped<ISentralGateway, SentralGateway>();

            services.AddTransient<ISMSGatewayConfiguration, SMSGatewayConfiguration>();
            services.AddScoped<ISMSGateway, SMSGateway>();

            return services;
        }

        public static IServiceCollection AddTrackItContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TrackItContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("TrackItConnection"));
            });

            services.AddScoped<ITrackItSyncJob, TrackItSyncJob>();

            return services;
        }

        public static IServiceCollection AddConstallationContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            });

            services.AddScoped<IAppDbContext, AppDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        // To be consolidated:
        // 1. Move all email to a queue
        // 2. Create an email queue worker to send emails
        // 3. Register the worker, the razor enginge, and the gateway services as a single operation
        // This would allow for separation of concerns and decoupling of the engine from presentation projects that don't require it
        // Cannot be done as a BackgroundService without requiring the engine to be injected everywhere that queues work for the service.
        // Seems to need to be a manual database queue that all front-ends can add to (possibly via Mediator?)
        // Then a separate (possibly Hangfire) processor that simply pops work from the queue and processes it.
        // Problem is that the database has to be generic enough to take all types of emails.
        // Maybe I could use a serialized string for the data, and include a type call in the queue?
        // e.g. EmailType = "StudentAbsenceNotification", Data = "{ to: [{emailname, email1}, {emailname, email2}]}" etc?
        // But would need to figure out how to separately deal with attachments.
        // Or, pull back and include the type and the reference, so the processor would compile the entire email itself
        // e.g. EmailType = "StudentAbsenceNotification", Data = "{ studentId: 432109876, absenceId: xxxxx }"
        public static IServiceCollection AddEmailTemplateEngine(this IServiceCollection services)
        {
            services.AddTransient<IEmailGatewayConfiguration, EmailGatewayConfiguration>();
            services.AddScoped<IEmailGateway, EmailGateway>();

            services.AddScoped<IEmailService, EmailService>();
            
            services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConstallationContext(configuration);

            services.AddTrackItContext(configuration);

            services.AddSingleton(Log.Logger);

            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddHangfireJobs()
                .AddExternalServiceGateways()
                .AddEmailTemplateEngine();

            //services.Scan(scan => scan
            //    .FromAssemblyOf<IApplicationService>()

            //    .AddClasses(classes => classes.AssignableTo<IScopedService>())
            //    .AsMatchingInterface()
            //    .WithScopedLifetime()

            //    .AddClasses(classes => classes.AssignableTo<ITransientService>())
            //    .AsMatchingInterface()
            //    .WithTransientLifetime()

            //    .AddClasses(classes => classes.AssignableTo<ISingletonService>())
            //    .AsMatchingInterface()
            //    .WithSingletonLifetime()
            //);

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
                options.Cookie.Name = "Constellation.Fallback.Identity";
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
                options.Cookie.Name = "Constellation.Staff.Identity";
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
                options.Cookie.Name = "Constellation.Parents.Identity";
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
                options.Cookie.Name = "Constellation.Schools.Identity";
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.LoginPath = new PathString("/Portal/School/Auth/Login");
                options.LogoutPath = new PathString("/Portal/School/Auth/LogOut");
            });

            return services;
        }

        public static IServiceCollection AddParentPortalInfrastructureComponents(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConstallationContext(configuration);

            services.AddIdentity<AppUser, AppRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddIdentityServer()
                .AddApiAuthorization<AppUser, AppDbContext>();

            services.AddAuthentication()
                .AddIdentityServerJwt();

            services.AddSingleton(Log.Logger);

            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddApplication();

            return services;
        }
    }
}
