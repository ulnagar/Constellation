namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Attachments.Services;
using Constellation.Core.Models.Attachments.Services;

public static class ServicesRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAttachmentService, AttachmentService>();

        return services;
    }
}
