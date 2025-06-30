using System.Net.Http;
using System.Text;
using System.Text.Json;
using Grad_Project_Dashboard_1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
namespace Grad_Project_Dashboard_1.Services
{
    public interface IUserDomainService
    {
        Task<List<UserDomainName>> GetAllDomainsAsync();
        Task<(bool success, string message)> AddDomainAsync(UserDomainName domain);
        Task<(bool success, string message)> UpdateDomainAsync(UserDomainName domain);
        Task<(bool success, string message)> DeleteDomainAsync(int id);
        Task<(bool success, string message)> ToggleGlobalAIAsync(bool enabled);
    }
    public class UserDomainService : IUserDomainService
    {
        private readonly AppDbContext _context = new AppDbContext();
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserDomainService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor; // Add this field

    public UserDomainService(
        HttpClient httpClient,
        ILogger<UserDomainService> logger,
        IHttpContextAccessor httpContextAccessor) // Add this parameter
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor; // Initialize the field
    }

        public async Task<List<UserDomainName>> GetAllDomainsAsync()
        {
            try
            {
                return await _context.UserDomainNames.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get domains from database");
                return new List<UserDomainName>();
            }
        }

public async Task<(bool success, string message)> AddDomainAsync(UserDomainName domain)
{
    try
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(domain.Domain))
            return (false, "Domain is required");
        if (string.IsNullOrWhiteSpace(domain.IPAddress))
            return (false, "IP Address is required");

        // Set UserId
        var userId = int.Parse(_httpContextAccessor.HttpContext.User
            .FindFirst(ClaimTypes.NameIdentifier).Value);
        domain.UserId = userId;

        // Add to database
        _context.UserDomainNames.Add(domain);
        await _context.SaveChangesAsync();

        // 1. Send domain to external API (Add-Domain)
        var (addSuccess, addMessage) = await SendDomainToExternalApi("Add-Domain", domain);
        if (!addSuccess) return (false, addMessage);

        return (true, "Domain added successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to add domain");
        return (false, $"Failed to add domain: {ex.Message}");
    }
}

public async Task<(bool success, string message)> UpdateDomainAsync(UserDomainName domain)
{
    try
    {
        var existingDomain = await _context.UserDomainNames.FindAsync(domain.Id);
        if (existingDomain == null) return (false, "Domain not found");

        // Track what changed
        bool domainChanged = existingDomain.Domain != domain.Domain;
        bool ipChanged = existingDomain.IPAddress != domain.IPAddress;

        // Update fields in database
        existingDomain.Domain = domain.Domain;
        existingDomain.IPAddress = domain.IPAddress;
        await _context.SaveChangesAsync();

        // Conditionally call APIs based on what changed
        if (domainChanged || ipChanged)
        {
            // Only call Add-Domain if domain or IP changed
            var (addSuccess, addMessage) = await SendDomainToExternalApi("Add-Domain", domain);
            if (!addSuccess) return (false, addMessage);
        }

        // If nothing changed, just return success
        return (true, "Domain updated successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Failed to update domain with ID {domain.Id}");
        return (false, $"Failed to update domain: {ex.Message}");
    }
}

// private async Task<(bool success, string message)> ToggleAIAsync(int id ,bool enabled)
// {
//     try
//     {
//         var toggleData = new
//         {
//             @switch = enabled ? "true" : "false"
//         };

//         var json = JsonSerializer.Serialize(toggleData);
//         var content = new StringContent(json, Encoding.UTF8, "application/json");

//         var request = new HttpRequestMessage(HttpMethod.Post, "/Toggle-AI")
//         {
//             Content = content
//         };
//         request.Headers.Add("Host", "34.72.112.1");
//         request.Headers.Add("Accept", "application/json");

//         var response = await _httpClient.SendAsync(request);
//         if (!response.IsSuccessStatusCode)
//         {
//             var errorContent = await response.Content.ReadAsStringAsync();
//             return (false, $"Toggle-AI API returned {response.StatusCode}: {errorContent}");
//         }

//         return (true, "Success");
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, $"Failed to toggle AI for User {id}");
//         return (false, ex.Message);
//     }
// }
public async Task<(bool success, string message)> ToggleGlobalAIAsync(bool enabled)
{
    try
    {
        
        Console.WriteLine(enabled);
        var toggleData = new
        {
            @switch = enabled ? "true" : "false"
        };

        var json = JsonSerializer.Serialize(toggleData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/Toggle-AI")
        {
            Content = content
        };
        request.Headers.Add("Host", "34.72.112.1");
        request.Headers.Add("Accept", "application/json");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, $"API returned {response.StatusCode}: {errorContent}");
        }

        return (true, "Success");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to toggle global AI status");
        return (false, ex.Message);
    }
}
        public async Task<(bool success, string message)> DeleteDomainAsync(int id)
        {
            try
            {
                var domain = await _context.UserDomainNames.FindAsync(id);
                if (domain == null) return (false, "Domain not found");

                // Send delete to external API first
                var (apiSuccess, apiMessage) = await SendDeleteDomainToExternalApi(domain);
                if (!apiSuccess) return (false, apiMessage);

                // Delete from database
                _context.UserDomainNames.Remove(domain);
                await _context.SaveChangesAsync();

                return (true, "Domain deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete domain with ID {id}");
                return (false, $"Failed to delete domain: {ex.Message}");
            }
        }

private async Task<(bool success, string message)> SendDomainToExternalApi(string endpoint, UserDomainName domain)
{
    try
    {
        var domainData = new
        {
            domain = domain.Domain,
            backend = domain.IPAddress
        };
        
        var json = JsonSerializer.Serialize(domainData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        request.Headers.Add("Host", "34.72.112.1");
        request.Headers.Add("Accept", "application/json");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, $"Add-Domain API returned {response.StatusCode}: {errorContent}");
        }

        return (true, "Success");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Failed to send domain to external API. Domain: {domain.Domain}");
        return (false, ex.Message);
    }
}

        private async Task<(bool success, string message)> SendDeleteDomainToExternalApi(UserDomainName domain)
        {
            try
            {
                var domainData = new
                {
                    domain = domain.Domain
                };

                var json = JsonSerializer.Serialize(domainData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, "Delete-Domain")
                {
                    Content = content
                };
                request.Headers.Add("Host", "34.72.112.1");
                request.Headers.Add("Accept", "application/json");

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"API returned {response.StatusCode}: {errorContent}");
                }

                return (true, "Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send delete domain request to external API. Domain: {domain.Domain}");
                return (false, ex.Message);
            }
        }
    }
}