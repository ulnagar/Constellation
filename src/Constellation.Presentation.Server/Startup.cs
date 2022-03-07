using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Presentation.Server.Helpers.DependencyInjection;
using Constellation.Presentation.Server.Infrastructure;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Filters;
using System;

namespace Constellation.Presentation.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/ClassMonitor.log", Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IClassMonitorJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/PermissionUpdate.log", Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IPermissionUpdateJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/LessonNotifications.log", Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day);
                    config.Filter.ByIncludingOnly(Matching.FromSource<ILessonNotificationsJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/AttendanceReports.log", Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IAttendanceReportJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/AbsenceMonitor.log", Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IAbsenceMonitorJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/RollMarkingReport.log", Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IRollMarkingReportJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/SchoolRegister.log", Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<ISchoolRegisterJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/UserManager.log", Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IUserManagerJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/EmailGateway.log", Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IEmailGateway>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/System.log", Serilog.Events.LogEventLevel.Debug, rollingInterval: RollingInterval.Day);
                    config.Filter.ByExcluding(Matching.FromSource<IHangfireJob>());
                })
                .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInfrastructure(Configuration);
#if DEBUG
            services.AddStandardAuthentication(Configuration);
#else
            services.AddMainAppAuthentication(Configuration);
#endif

            services.AddMvc().AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddRazorPages();
            //services.AddServerSideBlazor();

            services.AddHangfire(c => c.UseSqlServerStorage(Configuration.GetConnectionString("Hangfire")));
            GlobalConfiguration.Configuration.UseSqlServerStorage(Configuration.GetConnectionString("Hangfire"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            });
            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IUnitOfWork unitOfWork, IRecurringJobManager manager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
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

            var jobManager = new HangfireJobRegistration(unitOfWork, manager);
            jobManager.RegisterJobs();

            IdentityDefaults.SeedRoles(roleManager);
            IdentityDefaults.SeedUsers(userManager);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );

                //endpoints.MapBlazorHub();
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
