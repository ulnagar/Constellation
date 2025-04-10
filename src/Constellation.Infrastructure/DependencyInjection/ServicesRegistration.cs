namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Clock;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Infrastructure.ExternalServices.Teams;
using Constellation.Infrastructure.Idempotence;
using Constellation.Infrastructure.Identity.Authorization;
using Constellation.Infrastructure.Jobs;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Infrastructure.Persistence.ConstellationContext.Interceptors;
using Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;
using Constellation.Infrastructure.Persistence.TrackItContext;
using Constellation.Infrastructure.Services;
using Constellation.Infrastructure.Templates.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Scrutor;
using Serilog;

public static class ServicesRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Add Logging
        services.AddSingleton(Log.Logger);

        // Add IOptions
        services.AddOptions<AppConfiguration>();
        services.Configure<AppConfiguration>(configuration.GetSection(AppConfiguration.Section));
        services.AddOptions<ParentPortalConfiguration>();
        services.Configure<ParentPortalConfiguration>(configuration.GetSection(ParentPortalConfiguration.Section));

        // Add Constellation Context

        services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();
        services.AddScoped<UpdateAuditableEntitiesInterceptor>();
        services.AddScoped<CreateAuditLogEntitiesInterceptor>();

        services.AddDbContext<AppDbContext>(
            (sp, options) =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b =>
                    {
                        b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                        b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        b.CommandTimeout(120); // Increased command timeout to allow migrations to complete. May not be necessary after Term 1 2024.
                    });

                options.EnableSensitiveDataLogging(true);

                options.AddInterceptors(new List<IInterceptor> {
                    sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>(),
                    sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>(),
                    sp.GetRequiredService<CreateAuditLogEntitiesInterceptor>()
                });
            });

        services.AddScoped<IAppDbContext, AppDbContext>();
        services.AddScoped<AppDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add TrackIt Context

        services.AddDbContext<TrackItContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("TrackItConnection"));
        });

        services.AddScoped<ITrackItSyncJob, TrackItSyncJob>();

        // Add Hangfire Services

        //services.Scan(selector =>
        //    selector
        //        .FromAssemblies(Constellation.Application.AssemblyReference.Assembly)
        //        .RegisterHandlers(typeof(INotificationHandler<>)));

        services.Decorate(typeof(INotificationHandler<>), typeof(IdempotentDomainEventHandler<>));

        // Add Hangfire Jobs

        services.AddScoped<IAbsenceMonitorJob, AbsenceMonitorJob>();
        services.AddScoped<IAbsenceProcessingJob, AbsenceProcessingJob>();
        services.AddScoped<IAssignmentSubmissionJob, AssignmentSubmissionJob>();
        services.AddScoped<IAttachmentManagementJob, AttachmentManagementJob>();
        services.AddScoped<IAttendanceReportJob, AttendanceReportJob>();
        services.AddScoped<IClassMonitorJob, ClassMonitorJob>();
        services.AddScoped<IGroupTutorialExpiryScanJob, GroupTutorialExpiryScanJob>();
        services.AddScoped<ILessonNotificationsJob, LessonNotificationsJob>();
        services.AddScoped<ILessonUpdateJob, LessonUpdateJob>();
        services.AddScoped<IMandatoryTrainingScanJob, MandatoryTrainingScanJob>();
        services.AddScoped<ICanvasAccessAuditJob, CanvasAccessAuditJob>();
        services.AddScoped<IProcessOutboxMessagesJob, ProcessOutboxMessagesJob>();
        services.AddScoped<IProcessScheduledReportsJob, ProcessScheduledReportsJob>();
        services.AddScoped<IRollMarkingReportJob, RollMarkingReportJob>();
        services.AddScoped<ISchoolRegisterJob, SchoolRegisterJob>();
        services.AddScoped<ISentralAttendancePercentageSyncJob, SentralAttendancePercentageSyncJob>();
        services.AddScoped<ISentralAwardSyncJob, SentralAwardSyncJob>();
        services.AddScoped<ISentralFamilyDetailsSyncJob, SentralFamilyDetailsSyncJob>();
        services.AddScoped<ISentralPhotoSyncJob, SentralPhotoSyncJob>();
        services.AddScoped<ISentralReportSyncJob, SentralReportSyncJob>();
        services.AddScoped<ITrackItSyncJob, TrackItSyncJob>();
        services.AddScoped<IUserManagerJob, UserManagerJob>();
        services.AddScoped<ISentralComplianceScanJob, SentralComplianceScanJob>();

        services.AddScoped(typeof(IJobDispatcherService<>), typeof(JobDispatcherService<>));

        // Add External Service gateways

        services.AddCanvasExternalService(configuration);
        services.AddDoEDataServicesGateway(configuration);
        services.AddEmailExternalService(configuration);
        services.AddLinkShortenerExternalService(configuration, environment);
        services.AddNetworkStatisticsExternalService(configuration);
        services.AddSentralExternalService(configuration);
        services.AddSMSExternalService(configuration, environment);
        services.AddTeamsExternalService(configuration);

        // Add Email Template Engine

        services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

        // Add repositories

        services.Scan(selector =>
            selector.FromAssemblies(
                Constellation.Application.AssemblyReference.Assembly,
                Constellation.Infrastructure.AssemblyReference.Assembly)
            .AddClasses(classes => classes.InNamespaceOf<UnitOfWork>(), false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface()
            .WithScopedLifetime());

        // Add remaining services

        services.Scan(selector =>
            selector.FromAssemblies(
                Constellation.Application.AssemblyReference.Assembly,
                Constellation.Infrastructure.AssemblyReference.Assembly)
            .AddClasses(classes => classes.InNamespaceOf<ActiveDirectoryActionsService>(), false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface()
            .WithScopedLifetime());

        // Explicitly register transient IDateTimeProvider

        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, OwnsTrainingCompletionRecordByRoute>();
        services.AddScoped<IAuthorizationHandler, HasRequiredMandatoryTrainingModulePermissions>();
        services.AddScoped<IAuthorizationHandler, OwnsTrainingCompletionRecordByResource>();
        services.AddScoped<IAuthorizationHandler, IsCurrentTeacherAddedToTutorial>();
        services.AddScoped<IAuthorizationHandler, HasRequiredGroupTutorialModulePermissions>();
        services.AddScoped<IAuthorizationHandler, IsAssignedToActionByResource>();
        services.AddScoped<IAuthorizationHandler, IsInGroupAllowedToEditWorkFlows>();
        services.AddScoped<IAuthorizationHandler, IsAssignedToActionByRoute>();
        services.AddScoped<IAuthorizationHandler, HasActiveParentRecord>();
        services.AddScoped<IAuthorizationHandler, HasActiveContactAssignmentToCurrentPartnerSchool>();
        services.AddScoped<IAuthorizationHandler, HasAdminUserPrivileges>();
        services.AddScoped<IAuthorizationHandler, HasActiveStaffRecord>();
        services.AddScoped<IAuthorizationHandler, IsActiveStudent>();

        return services;
    }
}
