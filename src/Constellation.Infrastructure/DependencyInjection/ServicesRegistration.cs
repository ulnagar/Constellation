namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Common.Behaviours;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.ExternalServices.AdobeConnect;
using Constellation.Infrastructure.Idempotence;
using Constellation.Infrastructure.Jobs;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Infrastructure.Persistence.ConstellationContext.Interceptors;
using Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;
using Constellation.Infrastructure.Persistence.TrackItContext;
using Constellation.Infrastructure.Services;
using Constellation.Infrastructure.Templates.Services;
using FluentValidation;
using MediatR;
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

        // Add AutoMapper and AbstractValidator

        services.AddAutoMapper(Constellation.Application.AssemblyReference.Assembly);
        services.AddValidatorsFromAssembly(Constellation.Application.AssemblyReference.Assembly);

        // Add Mediatr

        services.AddMediatR(new[] {
            Constellation.Application.AssemblyReference.Assembly,
            Constellation.Infrastructure.AssemblyReference.Assembly,
            Constellation.Core.AssemblyReference.Assembly });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(BusinessValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

        // Add IOptions
        services.AddOptions<AppConfiguration>();
        services.Configure<AppConfiguration>(configuration.GetSection(AppConfiguration.Section));

        // Add Constellation Context

        services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();
        services.AddScoped<UpdateAuditableEntitiesInterceptor>();
        services.AddScoped<CreateAuditLogEntitiesInterceptor>();

        services.AddDbContext<AppDbContext>(
            (sp, options) =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

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

        services.Scan(selector =>
            selector
                .FromAssemblies(Constellation.Application.AssemblyReference.Assembly)
                .RegisterHandlers(typeof(INotificationHandler<>)));

        services.Decorate(typeof(INotificationHandler<>), typeof(IdempotentDomainEventHandler<>));

        // Add Hangfire Jobs

        services.AddScoped<IAbsenceMonitorJob, AbsenceMonitorJob>();
        services.AddScoped<IAbsenceProcessingJob, AbsenceProcessingJob>();
        services.AddScoped<IAttendanceReportJob, AttendanceReportJob>();
        services.AddScoped<IClassMonitorJob, ClassMonitorJob>();
        services.AddScoped<IGroupTutorialExpiryScanJob, GroupTutorialExpiryScanJob>();
        services.AddScoped<ILessonNotificationsJob, LessonNotificationsJob>();
        services.AddScoped<IMandatoryTrainingReminderJob, MandatoryTrainingReminderJob>();
        services.AddScoped<IPermissionUpdateJob, PermissionUpdateJob>();
        services.AddScoped<IProcessOutboxMessagesJob, ProcessOutboxMessagesJob>();
        services.AddScoped<IRollMarkingReportJob, RollMarkingReportJob>();
        services.AddScoped<ISchoolRegisterJob, SchoolRegisterJob>();
        services.AddScoped<ISentralAwardSyncJob, SentralAwardSyncJob>();
        services.AddScoped<ISentralFamilyDetailsSyncJob, SentralFamilyDetailsSyncJob>();
        services.AddScoped<ISentralPhotoSyncJob, SentralPhotoSyncJob>();
        services.AddScoped<ISentralReportSyncJob, SentralReportSyncJob>();
        services.AddScoped<ITrackItSyncJob, TrackItSyncJob>();
        services.AddScoped<IUserManagerJob, UserManagerJob>();

        services.AddScoped(typeof(IJobDispatcherService<>), typeof(JobDispatcherService<>));

        // Add External Service gateways

        services.AddAdobeConnectExternalService(configuration);
        services.AddCanvasExternalService(configuration);
        services.AddDoEDataServicesGateway(configuration);
        services.AddEmailExternalService(configuration);
        services.AddLinkShortenerExternalService(configuration, environment);
        services.AddNetworkStatisticsExternalService(configuration);
        services.AddSentralExternalService(configuration);
        services.AddSMSExternalService(configuration, environment);

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
            .AddClasses(classes => classes.InNamespaceOf<AuthService>(), false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface()
            .WithScopedLifetime());

        // Explicitly register transient IDateTimeProvider

        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
