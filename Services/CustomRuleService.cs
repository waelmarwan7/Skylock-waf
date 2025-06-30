// Services/CustomRuleService.cs
using System.Text;
using System.Text.Json;
using Grad_Project_Dashboard_1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Grad_Project_Dashboard_1.Services
{
    public class CustomRuleService
    {
        private readonly AppDbContext _context = new AppDbContext();
        private readonly HttpClient _httpClient;
        private readonly IDomainProvider _domainProvider;
        private readonly ILogger<CustomRuleService> _logger;

        public CustomRuleService(

            HttpClient httpClient,
            IDomainProvider domainProvider,
            ILogger<CustomRuleService> logger)
        {
            _httpClient = httpClient;
            _domainProvider = domainProvider;
            _logger = logger;
        }

        public async Task<List<CustomRule>> GetAllRulesAsync()
        {
            try
            {
                return await _context.CustomRules.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rules from database");
                return new List<CustomRule>();
            }
        }

        public async Task<CustomRule> GetRuleByIdAsync(int id)
        {
            try
            {
                return await _context.CustomRules.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get rule with ID {id}");
                return null;
            }
        }

        public async Task<(bool success, string message)> CreateRuleAsync(CustomRule rule)
        {
            try
            {
                rule.CreatedAt = DateTime.UtcNow;
                rule.UpdatedAt = DateTime.UtcNow;
                _context.CustomRules.Add(rule);
                await _context.SaveChangesAsync();
                CustomRule lastUser = _context.CustomRules
                                    .OrderByDescending(u => u.Id)
                                    .FirstOrDefault();

                // Send to external API
                var (apiSuccess, apiMessage) = await SendRuleToExternalApi("Create-Rule" , lastUser);
                if (!apiSuccess) return (false, apiMessage);

                

                return (true, "Rule created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create rule");
                return (false, $"Failed to create rule: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdateRuleAsync(CustomRule rule)
        {
            try
            {
                var existingRule = await _context.CustomRules.FindAsync(rule.Id);
                if (existingRule == null) return (false, "Rule not found");

                var (apiSuccess, apiMessage) = await SendRuleToExternalApi("Create-Rule", rule);
                if (!apiSuccess) return (false, apiMessage);

                existingRule.Name = rule.Name;
                existingRule.Method = rule.Method;
                existingRule.Regex = rule.Regex;
                existingRule.Location = rule.Location;
                existingRule.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return (true, "Rule updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update rule with ID {rule.Id}");
                return (false, $"Failed to update rule: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> DeleteRuleAsync(int id)
        {
            try
            {
                var rule = await _context.CustomRules.FindAsync(id);
                if (rule == null) return (false, "Rule not found");

                var (apiSuccess, apiMessage) = await SendDeleteToExternalApi(id);
                if (!apiSuccess) return (false, apiMessage);

                _context.CustomRules.Remove(rule);
                await _context.SaveChangesAsync();

                return (true, "Rule deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete rule with ID {id}");
                return (false, $"Failed to delete rule: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> ToggleRuleStatusAsync(int id, bool isActive)
        {
            try
            {
                var rule = await _context.CustomRules.FindAsync(id);
                if (rule == null) return (false, "Rule not found");

                var (apiSuccess, apiMessage) = await SendToggleToExternalApi(id, isActive);
                if (!apiSuccess) return (false, apiMessage);

                rule.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return (true, $"Rule {(isActive ? "activated" : "deactivated")} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to toggle rule status. ID: {id}, Status: {isActive}");
                return (false, $"Failed to toggle rule status: {ex.Message}");
            }
        }

        private async Task<(bool success, string message)> SendRuleToExternalApi(string endpoint, CustomRule rule)
        {
            try
            {
                var domain = _domainProvider.GetCurrentDomain();
                var ruleData = new
                {
                    id = rule.Id+1000,
                    method = rule.Method.ToUpper(),
                    location = rule.Location.ToLower(),
                    regex = rule.Regex,
                    name = rule.Name
                };

                var json = JsonSerializer.Serialize(ruleData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                Console.WriteLine(json);

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
                    return (false, $"API returned {response.StatusCode}: {errorContent}");
                }

                return (true, "Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send rule to external API. Rule ID: {rule.Id}");
                return (false, ex.Message);
            }
        }

        private async Task<(bool success, string message)> SendDeleteToExternalApi(int id)
        {
            try
            {
                var domain = _domainProvider.GetCurrentDomain();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"Delete-Rule/{id+1000}");
                request.Headers.Add("Host", "34.72.112.1");

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
                _logger.LogError(ex, $"Failed to send delete request to external API. Rule ID: {id}");
                return (false, ex.Message);
            }
        }

        private async Task<(bool success, string message)> SendToggleToExternalApi(int id, bool isActive)
        {
            try
            {
                var domain = _domainProvider.GetCurrentDomain();
                var endpoint = isActive ? "Activate-Rule" : "Deactivate-Rule";
                var request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/{id + 1000}");
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
                _logger.LogError(ex, $"Failed to send toggle request to external API. Rule ID: {id}");
                return (false, ex.Message);
            }
        }
    }
}
