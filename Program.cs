using Grad_Project_Dashboard_1.Models;
using Grad_Project_Dashboard_1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Identity;

namespace Grad_Project_Dashboard_1
{
    public class Program
    {
        public static void Main(string[] args)
        {

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Data Source=localhost\\SQLEXPRESS;Initial Catalog=DashBoard2;Trusted_Connection=True;Integrated Security=True;TrustServerCertificate=True;")));

// HTTP Client and Domain Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IDomainProvider, ClaimDomainProvider>();

// Configure HTTP clients with dynamic base address
builder.Services.AddHttpClient<RateLimitingService>((provider, client) =>
{
    var domainProvider = provider.GetRequiredService<IDomainProvider>();
    client.BaseAddress = new Uri($"https://{domainProvider.GetCurrentDomain()}/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

builder.Services.AddHttpClient<CustomRuleService>((provider, client) =>
{
    var domainProvider = provider.GetRequiredService<IDomainProvider>();
    client.BaseAddress = new Uri($"https://{domainProvider.GetCurrentDomain()}/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

// Repeat similar pattern for other services
builder.Services.AddHttpClient<RequestLogsService>((provider, client) => 
{
    var domainProvider = provider.GetRequiredService<IDomainProvider>();
    client.BaseAddress = new Uri($"https://{domainProvider.GetCurrentDomain()}/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

builder.Services.AddHttpClient<Api_ResponseService>((provider, client) => 
{
    var domainProvider = provider.GetRequiredService<IDomainProvider>();
    client.BaseAddress = new Uri($"https://{domainProvider.GetCurrentDomain()}/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

// Other services
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

// Add HTTP Client for GeoIPControlsService
builder.Services.AddHttpClient<GeoIPControlsService>((provider, client) =>
{
    var domainProvider = provider.GetRequiredService<IDomainProvider>();
    client.BaseAddress = new Uri($"https://{domainProvider.GetCurrentDomain()}/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Register GeoIPControlsService
builder.Services.AddScoped<GeoIPControlsService>();

builder.Services.AddSingleton<UserSignup>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
        }
    }
}
