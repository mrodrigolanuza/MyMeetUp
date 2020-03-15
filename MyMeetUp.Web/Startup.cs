using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using MyMeetUp.Web.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMeetUp.Web.Configuration;
using MyMeetUp.Web.Models;
using Microsoft.AspNetCore.Http;

namespace MyMeetUp.Web
{
    public class Startup
    {
        private readonly string _applicationName = "MyMeetUp";
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        //Development Configuration /////////////////////////////////////////////////////////////////
        public void ConfigureDevelopmentServices(IServiceCollection services) {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("MyMeetUpDb_Dev")));
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings_Dev"));
            ConfigureServices(services);
        }

        public void ConfigureDevelopment(IApplicationBuilder app, IWebHostEnvironment env) {
            env.ApplicationName = $"{_applicationName} [Development]";
            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
            Configure(app, env);
        }
        
        //Production Configuration /////////////////////////////////////////////////////////////////
        public void ConfigureProductionServices(IServiceCollection services) {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("MyMeetUpDb_Prd")));
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings_Prd"));
            ConfigureServices(services);
        }
        public void ConfigureProduction(IApplicationBuilder app, IWebHostEnvironment env) {
            env.ApplicationName = $"{_applicationName}";
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();      // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            Configure(app, env);
        }
        
        //Services /////////////////////////////////////////////////////////////////////////////////
        public void ConfigureServices(IServiceCollection services) {
            services.Configure<CookiePolicyOptions>(options =>
            {
                //Función lambda que determina si es necesario el consentimiento del usuario para cookies no necesarias. 
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddDefaultIdentity<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddHealthChecks();
        }

        //HTTP request pipeline configuration //////////////////////////////////////////////////////
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz");

                endpoints.MapControllerRoute(
                    name: "defaultAreas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
                endpoints.MapRazorPages();
            });
        }
    }
}
