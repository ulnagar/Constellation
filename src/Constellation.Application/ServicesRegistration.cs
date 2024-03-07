namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Attachments.Services;
using Constellation.Application.Common.Behaviours;
using Constellation.Application.Rollover.Repositories;
using Constellation.Core.Models.Attachments.Services;
using Constellation.Core.Models.Rollover.Repositories;
using FluentValidation;
using MediatR;

public static class ServicesRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add AutoMapper and AbstractValidator

        services.AddAutoMapper(Constellation.Application.AssemblyReference.Assembly);
        services.AddValidatorsFromAssembly(Constellation.Application.AssemblyReference.Assembly);

        // Add Mediatr
        services.AddMediatR(new[]
        {
            Constellation.Application.AssemblyReference.Assembly,
            Constellation.Core.AssemblyReference.Assembly
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingPipelineBehaviour<,>));
        //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestLoggingPipelineBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationPipelineBehaviour<,>));

        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddSingleton<IRolloverRepository, RolloverRepository>();

        return services;
    }
}
