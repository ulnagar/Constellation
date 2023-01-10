using Blazored.Modal;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Presentation.Portal.Schools.Pages.Auth;
using Constellation.Presentation.Portal.Schools.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Constellation.Presentation.Portal.Schools
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;

            LoggingConfiguration.SetupLogging(configuration, Serilog.Events.LogEventLevel.Debug);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSchoolPortalInfrastructureComponents(Configuration);

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddBlazoredModal();

            services.AddRazorPages();
            services.AddServerSideBlazor().AddCircuitOptions(opts =>
            {
                opts.DetailedErrors = true;
                //opts.DetailedErrors = _env.IsDevelopment();
            });

            services.AddHttpClient();

            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<AppUser>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // MUST BE CALLED FIRST
            // to ensure that other middleware takes notice of this setting.
            app.UsePathBase("/Portal/School");

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

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
