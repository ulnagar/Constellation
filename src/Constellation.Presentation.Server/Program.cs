using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Identity.Authorization;
using Constellation.Infrastructure.Identity.ClaimsPrincipalFactories;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Presentation.Server.Helpers.HtmlGenerator;
using Constellation.Presentation.Server.Infrastructure;
using Constellation.Presentation.Server.Services;
using Constellation.Presentation.Server.Shared.Models;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
LoggingConfiguration.SetupLogging(builder.Configuration, Serilog.Events.LogEventLevel.Debug);

// Add application services
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment);

// Configuration Authentication and Authorization
builder.Services.AddIdentity<AppUser, AppRole>()
            .AddClaimsPrincipalFactory<StaffUserIdClaimsFactory>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

builder.Services.AddTransient<UserClaimsPrincipalFactory<AppUser, AppRole>, StaffUserIdClaimsFactory>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "Constellation.Staff.Identity";
    options.ExpireTimeSpan = TimeSpan.FromHours(7);
    options.LoginPath = new PathString("/Admin/Login");
    options.LogoutPath = new PathString("/Admin/Logout");
});

builder.Services.AddAuthorization(opt => opt.AddApplicationPolicies());

builder.Services.AddScoped<IAuthorizationHandler, OwnsTrainingCompletionRecordByRoute>();
builder.Services.AddScoped<IAuthorizationHandler, HasRequiredMandatoryTrainingModulePermissions>();
builder.Services.AddScoped<IAuthorizationHandler, OwnsTrainingCompletionRecordByResource>();
builder.Services.AddScoped<IAuthorizationHandler, IsCurrentTeacherAddedToTutorial>();
builder.Services.AddScoped<IAuthorizationHandler, HasRequiredGroupTutorialModulePermissions>();

// Register Current User Service
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Register Hangfire
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

builder.Services.AddRazorPages();
builder.Services.AddMvc()
    .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddDateOnlyTimeOnlyStringConverters();

builder.Services.Replace(ServiceDescriptor.Singleton<IHtmlGenerator, CustomHtmlGenerator>());

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.AreaPageViewLocationFormats.Add("/Pages/Shared/PartialViews/{0}/{0}" + RazorViewEngine.ViewExtension);
    options.AreaPageViewLocationFormats.Add("/Pages/Shared/PartialViews/{1}/{1}" + RazorViewEngine.ViewExtension);
});

builder.Services.AddSingleton<RolloverService>();

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