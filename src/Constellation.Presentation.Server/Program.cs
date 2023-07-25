using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Presentation.Server.Helpers.HtmlGenerator;
using Constellation.Presentation.Server.Infrastructure;
using Constellation.Presentation.Server.Services;
using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
LoggingConfiguration.SetupLogging(builder.Configuration, Serilog.Events.LogEventLevel.Debug);

// Add services to the container.
builder.Services.AddStaffPortalInfrastructureComponents(builder.Configuration);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddHangfire((provider, configuration) => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));
GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });

builder.Services.AddValidatorsFromAssemblyContaining<IAppDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddMvc()
    .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddDateOnlyTimeOnlyStringConverters();

builder.Services.Replace(ServiceDescriptor.Singleton<IHtmlGenerator, CustomHtmlGenerator>());

builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("app");
    try
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        await IdentityDefaults.SeedRoles(roleManager);
        await IdentityDefaults.SeedUsers(userManager);

        var env = services.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment())
        {
            //await IdentityDefaults.SeedTestUsers(userManager);
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to seed users and roles.");
    }
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    AppPath = "/",
    DashboardTitle = "Hangfire Dashboard",
    Authorization = new[]
    {
        new HangfireAuthorizationFilter()
    }
});

app.MapRazorPages();
app.MapControllers();

app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.Map("/services", hostBuilder => hostBuilder.Run(async context =>
{
    var sb = new StringBuilder();
    sb.Append("<h1>Registered Services</h1>");
    sb.Append("<table><thead>");
    sb.Append("<tr><th>Type</th><th>Lifetime</th><th>Instance</th></tr>");
    sb.Append("</thead></tbody>");
    foreach (var svc in builder.Services)
    {
        sb.Append("<tr>");
        sb.Append($"<td>{svc.ServiceType.FullName}</td>");
        sb.Append($"<td>{svc.Lifetime}</td>");
        sb.Append($"<td>{svc.ImplementationType?.FullName}</td>");
        sb.Append("</tr>");
    }
    sb.Append("</tbody></table>");
    await context.Response.WriteAsync(sb.ToString());
}));

app.Run();