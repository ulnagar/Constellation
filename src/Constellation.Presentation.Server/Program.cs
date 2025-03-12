using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions.Services;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Identity.Authorization;
using Constellation.Infrastructure.Identity.ClaimsPrincipalFactories;
using Constellation.Infrastructure.Identity.MagicLink;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Presentation.Server.Helpers.HtmlGenerator;
using Constellation.Presentation.Server.Infrastructure;
using Constellation.Presentation.Server.Services;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
LoggingConfiguration.SetupLogging(builder.Configuration, Serilog.Events.LogEventLevel.Debug);

// Add application services
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment);

// Configuration Authentication and Authorization
builder.Services.AddIdentity<AppUser, AppRole>()
    .AddClaimsPrincipalFactory<CustomUserPropertiesClaimsFactory>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddPasswordlessLoginProvider();

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
    options.Cookie.Name = "Constellation.Identity";
    options.ExpireTimeSpan = TimeSpan.FromHours(7);
    options.LoginPath = new PathString("/Auth/Login");
    options.LogoutPath = new PathString("/Auth/Logout");
});

builder.Services
    .AddAuthorization(opt => opt.AddApplicationPolicies())
    .AddAuthorizationPolicies();

// Register Current User Service
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Register Hangfire
builder.Services.AddHangfire((provider, configuration) => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire"), new SqlServerStorageOptions
    {
        PrepareSchemaIfNecessary = true,
        EnableHeavyMigrations = true,

        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));
GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });

builder.Services.AddRazorPages()
    .AddSessionStateTempDataProvider()
    .AddApplicationPart(Constellation.Presentation.Shared.AssemblyReference.Assembly)
    .AddApplicationPart(Constellation.Presentation.Staff.AssemblyReference.Assembly)
    .AddApplicationPart(Constellation.Presentation.Schools.AssemblyReference.Assembly)
    .AddApplicationPart(Constellation.Presentation.Parents.AssemblyReference.Assembly)
    .AddApplicationPart(Constellation.Presentation.Students.AssemblyReference.Assembly);

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Constellation.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

builder.Services.AddMemoryCache();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMvc(options =>
    {
        options.ModelBinderProviders.Insert(0, new StronglyTypedIdBinderProvider());
        options.ModelBinderProviders.Insert(0, new PositionEnumBinderProvider());
    })
    .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.Replace(ServiceDescriptor.Singleton<IHtmlGenerator, CustomHtmlGenerator>());

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.AreaPageViewLocationFormats.Add("/Pages/Shared/PartialViews/{0}/{0}" + RazorViewEngine.ViewExtension);
    options.AreaPageViewLocationFormats.Add("/Pages/Shared/PartialViews/{1}/{1}" + RazorViewEngine.ViewExtension);

    options.AreaPageViewLocationFormats.Add("/Areas/{2}/Pages/Shared/PartialViews/{0}/{0}" + RazorViewEngine.ViewExtension);
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
});

builder.WebHost.UseStaticWebAssets();

WebApplication app = builder.Build();

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

using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    try
    {
        UserManager<AppUser> userManager = services.GetRequiredService<UserManager<AppUser>>();
        RoleManager<AppRole> roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        await IdentityDefaults.SeedRoles(roleManager);
        await IdentityDefaults.SeedUsers(userManager);

        IWebHostEnvironment env = services.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment())
        {
            //await IdentityDefaults.SeedTestUsers(userManager);
        }
    }
    catch
    {
        // ignored
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

app.UseSession();

app.Map("/services", hostBuilder => hostBuilder.Run(async context =>
{
    StringBuilder sb = new();
    sb.Append("<h1>Registered Services</h1>");
    sb.Append("<table><thead>");
    sb.Append("<tr><th>Type</th><th>Lifetime</th><th>Instance</th></tr>");
    sb.Append("</thead></tbody>");
    foreach (ServiceDescriptor svc in builder.Services)
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