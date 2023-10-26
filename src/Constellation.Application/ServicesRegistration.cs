namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Attachments.Services;
using Constellation.Application.Rollover.Repositories;
using Constellation.Core.Models.Attachments.Services;
using Constellation.Core.Models.Rollover.Repositories;

public static class ServicesRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAttachmentService, AttachmentService>();

        services.AddSingleton<IRolloverRepository, RolloverRepository>();

        return services;
    }
}
