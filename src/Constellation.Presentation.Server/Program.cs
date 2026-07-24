using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Auth;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Identity.Authorization;
using Constellation.Infrastructure.Identity.ClaimsPrincipalFactories;
using Constellation.Infrastructure.Identity.MagicLink;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Presentation.Server.Areas.API.Endpoints;
using Constellation.Presentation.Server.Helpers.ExceptionHandlers;
using Constellation.Presentation.Server.Helpers.HtmlGenerator;
using Constellation.Presentation.Server.Helpers.Identity;
using Constellation.Presentation.Server.Infrastructure;
using Constellation.Presentation.Server.Services;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using System.Globalization;
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

builder.Services
    .AddAuthorization(opt => opt.AddApplicationPolicies())
    .AddAuthorizationPolicies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "Constellation.Identity";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(90);
    options.SlidingExpiration = true;
    options.LoginPath = new PathString("/Auth/Login");
    options.LogoutPath = new PathString("/Auth/Logout");

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    })
    .AddOpenIdConnect(options =>
    {
#if DEBUG
        options.RequireHttpsMetadata = false; // DEVELOPMENT ONLY
#endif
        var oidcConfig = builder.Configuration.GetSection("OpenIDConnectSettings");

        options.Authority = oidcConfig["Authority"];
        options.ClientId = oidcConfig["ClientId"];
        options.ClientSecret = oidcConfig["ClientSecret"];

        options.CallbackPath = "/signin-oidc";
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = async context =>
            {
                await IdentityHelpers.SyncUserWithIdentity(context);
            }
        };
    });

// Register Current User Service
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<ICurrentUserService, CurrentUserService>();

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

builder.Services.AddTransient<HangfireAuthorizationFilter>();

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
    options.IdleTimeout = TimeSpan.FromMinutes(90);
    options.Cookie.IsEssential = true;
});

builder.Services.AddMemoryCache();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMvc(options =>
{
    options.ModelBinderProviders.Insert(0, new StudentFlagBinderProvider());
    options.ModelBinderProviders.Insert(0, new StronglyTypedIdBinderProvider());
    options.ModelBinderProviders.Insert(0, new StringEnumerationBinderProvider());
    options.ModelBinderProviders.Insert(0, new PositionEnumBinderProvider());
    options.ModelBinderProviders.Insert(0, new CanvasCourseCodeBinderProvider());
    options.ModelBinderProviders.Insert(0, new ContactPositionBinderProvider());
    options.ModelBinderProviders.Insert(0, new AssetNumberBinderProvider());
    options.ModelBinderProviders.Insert(0, new RecipientGroupBinderProvider());
    options.ModelBinderProviders.Insert(0, new AuthPermissionBinderProvider());
    options.ModelBinderProviders.Insert(0, new MessageRecipientListBinderProvider());
});

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

// Required for the TileProxyController
builder.Services.AddHttpClient("TileProxy", client =>
{
    // OSM requires a proper User-Agent — anonymous requests may be blocked
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Constellation/1.0");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddExceptionHandler<ConstellationExceptionHandler>();
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

if (!app.Environment.IsProduction())
{
    //app.UseDeveloperExceptionPage();
    app.UseExceptionHandler();
    app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
}
else
{
    app.UseExceptionHandler();
    app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    try
    {
        RoleManager<AppRole> roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        await IdentityDefaults.SeedRoles(roleManager);
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

app.MapRazorPages();
app.MapControllers();

app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.UseSession();

app.MapSmsEndpoints();
app.MapLissEndpoints();
app.MapTrackingEndpoints();
app.MapTileProxyEndpoints();

app.Map("/debug/services", hostBuilder => hostBuilder.Run(async context =>
{
    StringBuilder sb = new();
    sb.Append("<h1>Registered Services</h1>");
    sb.Append("<table><thead>");
    sb.Append("<tr><th>Type</th><th>Lifetime</th><th>Instance</th></tr>");
    sb.Append("</thead></tbody>");
    foreach (ServiceDescriptor svc in builder.Services)
    {
        sb.Append("<tr>");
        sb.Append(CultureInfo.InvariantCulture, $"<td>{svc.ServiceType.FullName}</td>");
        sb.Append(CultureInfo.InvariantCulture, $"<td>{svc.Lifetime}</td>");
        sb.Append(CultureInfo.InvariantCulture, $"<td>{svc.ImplementationType?.FullName}</td>");
        sb.Append("</tr>");
    }
    sb.Append("</tbody></table>");
    await context.Response.WriteAsync(sb.ToString());
}));

app.UseEndpoints(endpoints =>
{
    endpoints.MapHangfireDashboardWithAuthorizationPolicy(
        AuthPolicies.IsSiteAdmin, 
        "/hangfire",
        new DashboardOptions() { DashboardTitle = "Hangfire Dashboard", AppPath = "/" });
});

await app.RunAsync();