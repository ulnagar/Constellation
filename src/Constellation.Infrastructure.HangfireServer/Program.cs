using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.HangfireServer;
using Serilog;

var builder = WebApplication.CreateBuilder();

builder.Host.UseSerilog();

LoggingConfiguration.SetupLogging(builder.Configuration, Serilog.Events.LogEventLevel.Debug);

builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.Run();