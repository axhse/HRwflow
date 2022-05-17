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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // env.EnvironmentName = Environments.Production;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/home/error");
                // The default HSTS value is 30 days. You may want to change this for production
                // scenarios, see https://aka.ms/aspnetcore-hsts.
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

        // This method gets called by the runtime. Use this method to add services to the container.
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

            {
                // FIXME: TEMP
                var devConnection
                    = "Server=localhost\\SQLEXPRESS;Database=Hrwflow;Trusted_Connection=True;";
                Environment.SetEnvironmentVariable("ConnectionString", devConnection);
            }

            var connectionString = Environment.GetEnvironmentVariable("ConnectionString");

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
