using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Identity.ProfileService;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Portal.Schools.Server.Services;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
LoggingConfiguration.SetupLogging(builder.Configuration, Serilog.Events.LogEventLevel.Debug);

var configuration = builder.Configuration;
var environment = builder.Environment;

// Register Infrastructure services
builder.Services
    .AddApplication()
    .AddInfrastructure(configuration, environment);

// Configure Authentication and Authorization
//builder.Services.AddDefaultIdentity<AppUser>()
//    .AddRoles<AppRole>()
//    .AddUserManager<UserManager<AppUser>>()
//    .AddRoleManager<RoleManager<AppRole>>()
//    .AddSignInManager<SignInManager<AppUser>>()
//    .AddEntityFrameworkStores<AppDbContext>();

//builder.Services.AddIdentityServer(opts =>
//{
//    opts.KeyManagement.KeyPath = "Keys";
//    opts.KeyManagement.RotationInterval = TimeSpan.FromDays(30);
//    opts.KeyManagement.PropagationTime = TimeSpan.FromDays(2);
//    opts.KeyManagement.RetentionDuration = TimeSpan.FromDays(7);
//})
//    .AddApiAuthorization<AppUser, AppDbContext>()
//    .AddProfileService<WASMAuthenticationProfileService>();

builder.Services.AddBff();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "cookie";
        options.DefaultChallengeScheme = "oidc";
        options.DefaultSignOutScheme = "oidc";
    })
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "__Host-blazor";
        options.Cookie.SameSite = SameSiteMode.Strict;
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://localhost:44350";

        options.ClientId = "schools";
        options.ClientSecret = "TESTSECRET";
        options.ResponseType = "code";
        options.ResponseMode = "query";

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        //options.Scope.Add("SCOPE.ACOS.Schools");

        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
    });

//builder.Services.ConfigureApplicationCookie(opts =>
//{
//    opts.Cookie.Name = "Constellation.Schools.Identity";
//    opts.ExpireTimeSpan = TimeSpan.FromHours(1);
//});

// Register the Current User Service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UsePathBase("/schools/");
app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

//app.UseIdentityServer();
app.UseAuthentication();
app.UseBff();
app.UseAuthorization();

app.MapBffManagementEndpoints();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
