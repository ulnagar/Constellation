using Constellation.Application.Common.Behaviours;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Identity.MagicLink;
using Constellation.Infrastructure.Identity.ProfileService;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Infrastructure.Persistence.ConstellationContext.Interceptors;
using Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;
using Constellation.Infrastructure.Services;
using Constellation.Portal.Parents.Server.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Scrutor;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
LoggingConfiguration.SetupLogging(builder.Configuration, Serilog.Events.LogEventLevel.Debug);

builder.Services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();
builder.Services.AddScoped<UpdateAuditableEntitiesInterceptor>();

builder.Services.AddDbContext<AppDbContext>((sp, opt) =>
{
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(Constellation.Infrastructure.AssemblyReference.Assembly.FullName));

    opt.EnableSensitiveDataLogging(true);

    opt.AddInterceptors(new List<IInterceptor>
    {
        sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>(),
        sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>()
    });
});

builder.Services.AddScoped<IAppDbContext, AppDbContext>();
builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddDefaultIdentity<AppUser>()
    .AddRoles<AppRole>()
    .AddUserManager<UserManager<AppUser>>()
    .AddRoleManager<RoleManager<AppRole>>()
    .AddSignInManager<SignInManager<AppUser>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddPasswordlessLoginProvider();

builder.Services.AddIdentityServer(opts =>
{
    opts.KeyManagement.KeyPath = "Keys";
    opts.KeyManagement.RotationInterval = TimeSpan.FromDays(30);
    opts.KeyManagement.PropagationTime = TimeSpan.FromDays(2);
    opts.KeyManagement.RetentionDuration = TimeSpan.FromDays(7);
})
    .AddApiAuthorization<AppUser, AppDbContext>()
    .AddProfileService<WASMAuthenticationProfileService>();

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.Cookie.Name = "Constellation.Parents.Identity";
    opts.ExpireTimeSpan = TimeSpan.FromHours(1);
});

builder.Services.AddSingleton(Log.Logger);

builder.Services.AddMediatR(new[] { Constellation.Application.AssemblyReference.Assembly, Constellation.Infrastructure.AssemblyReference.Assembly });

builder.Services.AddAutoMapper(Constellation.Application.AssemblyReference.Assembly);

builder.Services.AddValidatorsFromAssembly(Constellation.Application.AssemblyReference.Assembly);
builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(BusinessValidationBehaviour<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

// Register the Current User Service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Search for an register all the Repository classes that are located at
// Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
builder.Services.Scan(selector =>
    selector.FromAssemblies(
        Constellation.Application.AssemblyReference.Assembly,
        Constellation.Infrastructure.AssemblyReference.Assembly)
    .AddClasses(classes => classes.InNamespaceOf<UnitOfWork>(), false)
    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
    .AsMatchingInterface()
    .WithScopedLifetime());

// Search for and register all the Services classes that are located at
// Constellation.Infrastructure.Services
builder.Services.Scan(selector =>
    selector.FromAssemblies(
        Constellation.Application.AssemblyReference.Assembly,
        Constellation.Infrastructure.AssemblyReference.Assembly)
    .AddClasses(classes => classes.InNamespaceOf<AuthService>(), false)
    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
    .AsMatchingInterface()
    .WithScopedLifetime());

builder.Services.AddEmailTemplateEngine(builder.Configuration);

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

app.UsePathBase("/parents");
app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
