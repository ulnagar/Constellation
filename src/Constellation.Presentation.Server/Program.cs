using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Presentation.Server.Areas.API.Endpoints;
using Constellation.Presentation.Server.DebugTools;
using Constellation.Presentation.Server.Extensions;
using Constellation.Presentation.Server.Helpers.ExceptionHandlers;
using Constellation.Presentation.Server.Services;
using Hangfire;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Serilog.Events;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
LoggingConfiguration.SetupLogging(builder.Configuration, LogEventLevel.Debug);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddConstellationAuth(builder.Configuration)
    .AddConstellationHangfire(builder.Configuration)
    .AddConstellationPresentation();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<ICurrentUserService, CurrentUserService>();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Constellation.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(90);
    options.Cookie.IsEssential = true;
});
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("TileProxy", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Constellation/1.0");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddExceptionHandler<ConstellationExceptionHandler>();
builder.Services.AddProblemDetails();

builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = null);
builder.WebHost.UseStaticWebAssets();
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
});

WebApplication app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseExceptionHandler();
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}
app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

await app.SeedIdentityAsync();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapSmsEndpoints();
app.MapLissEndpoints();
app.MapTrackingEndpoints();
app.MapTileProxyEndpoints();

RouteGroupBuilder passkeys = app.MapGroup("/passkey")
    .DisableAntiforgery();
passkeys.MapPasskeyRegistration();
passkeys.MapPasskeyLogin();

app.MapDebugServices(builder.Services);
app.MapDebugAuth();
app.MapHangfireDashboardWithAuthorizationPolicy(
    AuthPolicies.IsSiteAdmin,
    "/hangfire",
    new DashboardOptions { DashboardTitle = "Hangfire Dashboard", AppPath = "/" });

app.MapGet("/debug/endpoints", (IEnumerable<EndpointDataSource> sources) =>
    sources
        .SelectMany(s => s.Endpoints)
        .Select(e => new
        {
            Name = e.DisplayName,
            Route = (e as RouteEndpoint)?.RoutePattern?.RawText,
            Methods = e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods
        })
);

await app.RunAsync();