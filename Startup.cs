using System;
using HRwflow.Models;
using HRwflow.Models.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HRwflow
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static string ResourcePath { get; set; }

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            env.EnvironmentName = Environments.Production;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/home/error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "UseControllerAndAction",
                    pattern: "/{controller=Home}/{action=Main}/{*any}"
                );
                endpoints.MapControllerRoute(
                    name: "RedirectMain",
                    pattern: "/{controller=Home}/{*any}",
                    defaults: new { Controller = "Home", Action = "RedirectMain" }
                );
                endpoints.MapControllerRoute(
                    name: "RedirectMain-Home",
                    pattern: "{*any}",
                    defaults: new { Controller = "Home", Action = "RedirectMain" }
                );
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = "Session";
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromDays(1);
            });

            var configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", true, true)
               .Build();
            var connectionString
                = configuration.GetConnectionString("Production");

            var customerInfoService = new DbContextService<string, CustomerInfo>(
                    new CustomerInfoDbContext(connectionString));
            var teamService = new DbContextService<int, Team>(
                    new TeamDbContext(connectionString));
            var vacancyService = new DbContextService<int, Vacancy>(
                    new VacancyDbContext(connectionString));

            services.AddSingleton<IAuthService>(
                new AuthService(
                    new DbContextService<string, AuthInfo>(
                        new AuthInfoDbContext(connectionString))));

            services.AddSingleton<IStorageService<string, Customer>>(
                new DbContextService<string, Customer>(
                    new CustomerDbContext(connectionString)));

            services.AddSingleton<IStorageService<string, CustomerInfo>>(
                customerInfoService);

            services.AddSingleton(new WorkplaceService(
                customerInfoService, teamService, vacancyService));
        }
    }
}
