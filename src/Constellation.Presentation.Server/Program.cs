using Constellation.Application.Interfaces.Repositories;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Presentation.Server.Infrastructure;
using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
LoggingConfiguration.SetupLogging(builder.Configuration, Serilog.Events.LogEventLevel.Debug);

// Add services to the container.
builder.Services.AddStaffPortalInfrastructureComponents(builder.Configuration);

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
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

app.Run();