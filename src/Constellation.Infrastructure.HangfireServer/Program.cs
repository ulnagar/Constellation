using Constellation.Infrastructure.HangfireServer;

var builder = WebApplication.CreateBuilder();

Console.WriteLine($"ConnectionString Detected: {builder.Configuration.GetConnectionString("DefaultConnection")}");

builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.Run();