namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application;
using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Jobs.AbsenceClassworkNotificationJob;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions;
using Constellation.Infrastructure.ExternalServices.AdobeConnect;
using Constellation.Infrastructure.ExternalServices.Canvas;
using Constellation.Infrastructure.ExternalServices.CESE;
using Constellation.Infrastructure.ExternalServices.Email;
using Constellation.Infrastructure.ExternalServices.LinkShortener;
using Constellation.Infrastructure.ExternalServices.NetworkStatistics;
using Constellation.Infrastructure.ExternalServices.SchoolRegister;
using Constellation.Infrastructure.ExternalServices.Sentral;
using Constellation.Infrastructure.ExternalServices.SMS;
using Constellation.Infrastructure.Idempotence;
using Constellation.Infrastructure.Identity.Authorization;
using Constellation.Infrastructure.Identity.ClaimsPrincipalFactories;
using Constellation.Infrastructure.Identity.MagicLink;
using Constellation.Infrastructure.Identity.ProfileService;
using Constellation.Infrastructure.Jobs;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Infrastructure.Persistence.ConstellationContext.Interceptors;
using Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;
using Constellation.Infrastructure.Persistence.TrackItContext;
using Constellation.Infrastructure.Services;
using Constellation.Infrastructure.Templates.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Scrutor;
using Serilog;
using System;
using System.Reflection;

public static class ServicesRegistration
{
    public static IServiceCollection AddStaffPortalInfrastructureComponents(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConstellationContext(configuration)
            .AddTrackItContext(configuration);

        services.AddStaffPortalAuthentication(configuration);

        services.AddExternalServiceGateways()
            .AddEmailTemplateEngine()
            .AddHangfireJobs();

        services.AddSingleton(Log.Logger);

        // Import Mediatr handlers from Application and Infrastructure projects
        services.AddMediatR(new[] { Constellation.Application.AssemblyReference.Assembly, Constellation.Infrastructure.AssemblyReference.Assembly});

        services.Decorate(typeof(INotificationHandler<>), typeof(IdempotentDomainEventHandler<>));

        services.AddApplication();

        // Add any other missing services as their interfaces
        services.Scan(selector =>
            selector.FromAssemblies(
                Constellation.Application.AssemblyReference.Assembly,
                Constellation.Infrastructure.AssemblyReference.Assembly)
            .AddClasses(false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface()
            .WithScopedLifetime());

        return services;
    }
    public static IServiceCollection AddHangfireServerInfrastructureComponents(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConstellationContext(configuration)
            .AddTrackItContext(configuration);

        services.AddExternalServiceGateways()
            .AddEmailTemplateEngine()
            .AddHangfireJobs();

        services.AddSingleton(Log.Logger);

        services.AddMediatR(new[] { Assembly.GetExecutingAssembly(), typeof(IAppDbContext).Assembly });

        services.AddApplication();

        return services;
    }

    public static IServiceCollection AddSchoolPortalInfrastructureComponents(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConstellationContext(configuration)
            .AddExternalServiceGateways()
            .AddEmailTemplateEngine();
        
        services.AddSchoolPortalAuthentication();

        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IGroupTutorialRepository, GroupTutorialRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<ITutorialEnrolmentRepository, TutorialEnrolmentRepository>();
        services.AddScoped<ITutorialRollRepository, TutorialRollRepository>();
        services.AddScoped<ITutorialTeacherRepository, TutorialTeacherRepository>();
        services.AddScoped<ISchoolContactRepository, SchoolContactRepository>();
        services.AddScoped<IStudentFamilyRepository, StudentFamilyRepository>();
        services.AddScoped<IFacultyRepository, FacultyRepository>();

        services.AddSingleton(Log.Logger);

        services.AddMediatR(new[] { Assembly.GetExecutingAssembly(), typeof(IAppDbContext).Assembly });

        services.AddApplication();

        return services;
    }

    public static IServiceCollection AddNewSchoolPortalAuthentication(IServiceCollection services)
    {
        services.AddDefaultIdentity<AppUser>()
            .AddRoles<AppRole>()
            .AddUserManager<UserManager<AppUser>>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddEntityFrameworkStores<AppDbContext>();

        // Due to IS5 stupidity, the subsite configuration must be lower case:
        // https://stackoverflow.com/questions/62563174/identityserver4-authorization-error-not-matching-redirect-uri

        services.AddIdentityServer(opts =>
            {
                opts.KeyManagement.KeyPath = "Keys";
                opts.KeyManagement.RotationInterval = TimeSpan.FromDays(30);
                opts.KeyManagement.PropagationTime = TimeSpan.FromDays(2);
                opts.KeyManagement.RetentionDuration = TimeSpan.FromDays(7);
            })
            .AddApiAuthorization<AppUser, AppDbContext>()
            .AddProfileService<WASMAuthenticationProfileService>();

        services.AddAuthentication()
            .AddIdentityServerJwt();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "Constellation.Schools.Identity";
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
        });

        return services;
    }

    public static IServiceCollection AddParentPortalInfrastructureComponents(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConstellationContext(configuration)
            .AddExternalServiceGateways()
            .AddEmailTemplateEngine();

        services.AddParentPortalAuthentication();

        services.AddSingleton(Log.Logger);

        services.AddMediatR(new[] { Assembly.GetExecutingAssembly(), typeof(IAppDbContext).Assembly });

        services.AddApplication();

        return services;
    }

    // Helper Methods

    internal static IServiceCollection AddHangfireJobs(this IServiceCollection services)
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
        services.AddScoped<IMandatoryTrainingReminderJob, MandatoryTrainingReminderJob>();
        services.AddScoped<IProcessOutboxMessagesJob, ProcessOutboxMessagesJob>();
        services.AddScoped<IGroupTutorialExpiryScanJob, GroupTutorialExpiryScanJob>();

        services.AddScoped(typeof(IJobDispatcherService<>), typeof(JobDispatcherService<>));

        return services;
    }

    internal static IServiceCollection AddExternalServiceGateways(this IServiceCollection services)
    {
        services.AddAdobeConnectExternalService();
        services.AddCanvasExternalService();
        services.AddCeseExternalService();
        services.AddLinkShortenerExternalService();
        services.AddNetworkStatisticsExternalService();
        services.AddSchoolRegisterExternalService();
        services.AddSentralExternalService();
        services.AddSMSExternalService();

        return services;
    }

    internal static IServiceCollection AddTrackItContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TrackItContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("TrackItConnection"));
        });

        services.AddScoped<ITrackItSyncJob, TrackItSyncJob>();

        return services;
    }

    internal static IServiceCollection AddConstellationContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();
        services.AddScoped<UpdateAuditableEntitiesInterceptor>();

        services.AddDbContext<AppDbContext>(
            (sp, options) =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

                options.EnableSensitiveDataLogging(true);

                options.AddInterceptors(new List<IInterceptor> {
                    sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>(),
                    sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>()
                });
            });

        services.AddScoped<IAppDbContext, AppDbContext>();
        services.AddScoped<AppDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddApplicationServices();

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
        services.AddEmailExternalService();

        services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

        return services;
    }

    internal static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAbsenceService, AbsenceService>();
        services.AddScoped<IActiveDirectoryActionsService, ActiveDirectoryActionsService>();
        services.AddScoped<IAdobeConnectRoomService, AdobeConnectRoomService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<ICasualService, CasualService>();
        services.AddSingleton<IClassMonitorCacheService, ClassMonitorCacheService>();
        services.AddScoped<ICourseOfferingService, CourseOfferingService>();
        services.AddScoped<ICoverService, CoverService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IEnrolmentService, EnrolmentService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped(typeof(IJobDispatcherService<>), typeof(JobDispatcherService<>));
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped(typeof(ILogHandler<>), typeof(LogHandler<>));
        services.AddScoped<IOperationService, OperationService>();
        services.AddScoped<IPDFService, PDFService>();
        services.AddScoped<ISchoolContactService, SchoolContactService>();
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IStudentService, StudentService>();

        return services;
    }

    internal static IServiceCollection AddStaffPortalAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<AppUser, AppRole>()
            .AddClaimsPrincipalFactory<StaffUserIdClaimsFactory>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddTransient<UserClaimsPrincipalFactory<AppUser, AppRole>, StaffUserIdClaimsFactory>();

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

        services.AddAuthorization(opt => opt.AddApplicationPolicies());

        services.AddScoped<IAuthorizationHandler, OwnsTrainingCompletionRecordByRoute>();
        services.AddScoped<IAuthorizationHandler, HasRequiredMandatoryTrainingModulePermissions>();
        services.AddScoped<IAuthorizationHandler, OwnsTrainingCompletionRecordByResource>();
        services.AddScoped<IAuthorizationHandler, IsCurrentTeacherAddedToTutorial>();
        services.AddScoped<IAuthorizationHandler, HasRequiredGroupTutorialModulePermissions>();

        var clientId = configuration["Authentication:Microsoft:ClientId"];
        var clientSecret = configuration["Authentication:Microsoft:ClientSecret"];

        if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
        {
            services.AddAuthentication()
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = configuration["Authentication:Microsoft:ClientId"];
                    options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
                });
        }

        return services;
    }

    internal static IServiceCollection AddSchoolPortalAuthentication(this IServiceCollection services)
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

    internal static IServiceCollection AddParentPortalAuthentication(this IServiceCollection services)
    {
        services.AddDefaultIdentity<AppUser>()
            .AddRoles<AppRole>()
            .AddUserManager<UserManager<AppUser>>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddPasswordlessLoginProvider();

        // Due to IS5 stupidity, the subsite configuration must be lower case:
        // https://stackoverflow.com/questions/62563174/identityserver4-authorization-error-not-matching-redirect-uri

        services.AddIdentityServer(opts =>
        {
            opts.KeyManagement.KeyPath = "Keys";
            opts.KeyManagement.RotationInterval = TimeSpan.FromDays(30);
            opts.KeyManagement.PropagationTime = TimeSpan.FromDays(2);
            opts.KeyManagement.RetentionDuration = TimeSpan.FromDays(7);
        })
            .AddApiAuthorization<AppUser, AppDbContext>()
            .AddProfileService<WASMAuthenticationProfileService>();

        services.AddAuthentication()
            .AddIdentityServerJwt();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "Constellation.Parents.Identity";
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
        });

        return services;
    }
}
