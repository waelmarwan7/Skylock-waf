using Grad_Project_Dashboard_1.Models;
using Grad_Project_Dashboard_1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace Grad_Project_Dashboard_1
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services tdotnet tool install --global dotnet-efo the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });
            builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Data Source=DESKTOP-UH84CRS\\SQLEXPRESS;Initial Catalog=DashBoard2;Trusted_Connection=True;Integrated Security=True;TrustServerCertificate=True;")));

            ///////////////////////////////////////
            ///
            // In ConfigureServices method
            // Register Api_ResponseService with HttpClient

            // In your HttpClient configuration


            builder.Services.AddHttpClient(); // ✅ injects HttpClient where needed
            builder.Services.AddScoped<GeoIPControlsService>();
            //            builder.Services.AddSingleton<GeoIPControlsService>();

            builder.Services.AddHttpClient<RateLimitingService>(client =>
            {
                client.BaseAddress = new Uri("https://try.hackmeagain.tech/"); // optional default base
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            builder.Services.AddHttpClient<CustomRuleService>(client =>
            {
                client.BaseAddress = new Uri("https://try.hackmeagain.tech/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });
            builder.Services.AddScoped<CustomRuleService>();

            builder.Services.AddHttpClient<RequestLogsService>(client =>
            {
                client.BaseAddress = new Uri("https://try.hackmeagain.tech/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });

            builder.Services.AddHttpClient<Api_ResponseService>(client =>
            {
                client.BaseAddress = new Uri("https://try.hackmeagain.tech/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });
            builder.Services.AddSingleton<GCloudManager>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return new GCloudManager(config);
            });

            builder.Services.AddSingleton<GCloudCleanupService>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return new GCloudCleanupService(config);
            });

            builder.Services.AddSingleton<UserSignup>();
       //       builder.Services.AddScoped<RequestLogsService>();
            ///////////////////////////////////////
            ///
            /// 
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseCors(builder =>
        builder.WithOrigins("https://try.hackmeagain.tech/")
           .AllowAnyHeader()
           .AllowAnyMethod());


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
