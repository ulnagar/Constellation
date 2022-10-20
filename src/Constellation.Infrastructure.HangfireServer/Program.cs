using Constellation.Infrastructure.HangfireServer;

var builder = WebApplication.CreateBuilder();

builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.Run();