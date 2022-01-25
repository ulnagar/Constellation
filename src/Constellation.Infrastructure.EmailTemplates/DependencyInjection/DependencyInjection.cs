using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Constellation.Infrastructure.Templates.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDocumentTemplates(this IServiceCollection services)
        {
            var diagnosticSource = new DiagnosticListener("Constellation");
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddSingleton<DiagnosticListener>(diagnosticSource);

            return services;
        }
    }
}
