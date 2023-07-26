using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.HangfireServer;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService()
        ? AppContext.BaseDirectory : default
};

var builder = WebApplication.CreateBuilder(options);

builder.Host.UseSerilog();
LoggingConfiguration.SetupLogging(builder.Configuration, Serilog.Events.LogEventLevel.Debug);

builder.Services.RegisterServices(builder.Configuration, builder.Environment);
builder.Services.AddHostedService<Worker>();

SelectPdf.GlobalProperties.HtmlEngineFullPath = @"q:\inetpub\wwwroot\hangfire\Select.Html.dep";

builder.Host.UseWindowsService();

var app = builder.Build();

app.Run();