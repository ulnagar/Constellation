namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.ExternalServices.SMS;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Polly;
using System.Net.Http.Headers;
using System.Runtime;

public static class SmsServicesRegistration
{
    public static IServiceCollection AddSmsExternalService(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<SMSGatewayConfiguration>();
        services.Configure<SMSGatewayConfiguration>(configuration.GetSection(SMSGatewayConfiguration.Section));

        services.AddHttpClient<Gateway>(client =>
        {
            client.BaseAddress = new Uri($"https://{configuration[SMSGatewayConfiguration.Section + ":Host"]}/{SMSGatewayConfiguration.Section + "Version"}/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        })
            .AddTransientHttpErrorPolicy(policy =>
                policy.WaitAndRetryAsync(5, attempt => TimeSpan.FromSeconds(5)));

        services.AddScoped<ISMSGateway, Gateway>();

        services.AddScoped<ISMSService, Service>();

        return services;
    }
}