using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.DependencyInjection;
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
                })
                .WriteTo.File("logs/System.log", Serilog.Events.LogEventLevel.Debug, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInfrastructure(Configuration);

            services.AddMvc().AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddHangfire(c => {
                c.UseSqlServerStorage(Configuration.GetConnectionString("Hangfire"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });
            });
            services.AddHangfireServer();

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IRecurringJobManager jobManager)
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
                AppPath = null,
                DashboardTitle = "Hangfire Dashboard",
                Authorization = new[]
                {
                    new HangfireAuthorizationFilter()
                }
            });

            jobManager.AddOrUpdate<IClassMonitorJob>(nameof(IClassMonitorJob), (job) => job.StartJob(), "* 7-15 * * 1-5", TimeZoneInfo.Local);
            jobManager.AddOrUpdate<IPermissionUpdateJob>(nameof(IPermissionUpdateJob), (job) => job.StartJob(), "*/5 7-15 * * 1-5", TimeZoneInfo.Local);
            jobManager.AddOrUpdate<ISchoolRegisterJob>(nameof(ISchoolRegisterJob), (job) => job.StartJob(), "15 18 1 * *", TimeZoneInfo.Local);
            jobManager.AddOrUpdate<IAbsenceMonitorJob>(nameof(IAbsenceMonitorJob), (job) => job.StartJob(), "0 13 * * 1-6", TimeZoneInfo.Local);
            jobManager.AddOrUpdate<IRollMarkingReportJob>(nameof(IRollMarkingReportJob), (job) => job.StartJob(), "0 17 * * 1-5", TimeZoneInfo.Local);

            // LessonNotifications for Term 1 2022 (specific dates)
            jobManager.AddOrUpdate<ILessonNotificationsJob>($"{nameof(ILessonNotificationsJob)} - 21/02/22", (job) => job.StartJob(), "0 10 21 2 1", TimeZoneInfo.Local);
            jobManager.AddOrUpdate<ILessonNotificationsJob>($"{nameof(ILessonNotificationsJob)} - 07/03/22", (job) => job.StartJob(), "0 10 7 3 1", TimeZoneInfo.Local);
            jobManager.AddOrUpdate<ILessonNotificationsJob>($"{nameof(ILessonNotificationsJob)} - 21/03/22", (job) => job.StartJob(), "0 10 21 3 1", TimeZoneInfo.Local);
            jobManager.AddOrUpdate<ILessonNotificationsJob>($"{nameof(ILessonNotificationsJob)} - 04/04/22", (job) => job.StartJob(), "0 10 4 4 1", TimeZoneInfo.Local);

            // AttendanceReports for Term 1 2022 (specific dates)
            jobManager.AddOrUpdate<IAttendanceReportJob>($"{nameof(IAttendanceReportJob)} - 14/02/22", (job) => job.StartJob(), "0 13 14 2 1", TimeZoneInfo.Local);
            jobManager.AddOrUpdate<IAttendanceReportJob>($"{nameof(IAttendanceReportJob)} - 28/02/22", (job) => job.StartJob(), "0 13 28 2 1", TimeZoneInfo.Local);
            jobManager.AddOrUpdate<IAttendanceReportJob>($"{nameof(IAttendanceReportJob)} - 14/03/22", (job) => job.StartJob(), "0 13 14 3 1", TimeZoneInfo.Local);
            jobManager.AddOrUpdate<IAttendanceReportJob>($"{nameof(IAttendanceReportJob)} - 28/03/22", (job) => job.StartJob(), "0 13 28 3 1", TimeZoneInfo.Local);

            IdentityDefaults.SeedRoles(roleManager);
            IdentityDefaults.SeedUsers(userManager);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );

                endpoints.MapBlazorHub();
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
