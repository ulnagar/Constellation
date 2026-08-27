namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Common.Behaviours;
using Constellation.Application.Domains.Attachments.Services;
using Constellation.Application.Domains.Contacts.Interfaces;
using Constellation.Application.Domains.Contacts.Services;
using Constellation.Application.Domains.Import.Models;
using Constellation.Application.Domains.Rollover.Repositories;
using Constellation.Core.Models.Attachments.Services;
using Constellation.Core.Models.Rollover.Repositories;
using Constellation.Core.Models.WorkFlow.Services;
using FluentValidation;

public static class ServicesRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add AutoMapper and AbstractValidator
        services.AddValidatorsFromAssembly(Constellation.Application.AssemblyReference.Assembly);

        // Add Mediatr
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(new[]
            {
                Constellation.Application.AssemblyReference.Assembly, Constellation.Core.AssemblyReference.Assembly
            });

            config.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehaviour<,>));
            config.AddOpenBehavior(typeof(RequestValidationPipelineBehaviour<,>));
        });

        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddSingleton<IRolloverRepository, RolloverRepository>();
        services.AddSingleton<IStudentFlagCacheService, StudentFlagCacheService>();
        services.AddScoped<ICaseService, CaseService>();
        
        // Add CSV Reader parser files
        services.AddScoped<SentralAwardReportCsvParser>();

        return services;
    }
}
