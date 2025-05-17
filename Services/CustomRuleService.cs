// Services/CustomRuleService.cs
using System.Text;
using System.Text.Json;
using Grad_Project_Dashboard_1.Models;
using Microsoft.EntityFrameworkCore;
namespace Grad_Project_Dashboard_1.Services
{
    public class CustomRuleService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string ApiBaseUrl = "https://try.hackmeagain.tech";

        public CustomRuleService(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<CustomRule>> GetAllRulesAsync()
        {
            return await _context.CustomRules.ToListAsync();
        }

        public async Task<(bool success, string message)> CreateRuleAsync(CustomRule rule)
        {
            var (apiSuccess, apiMessage) = await SendRuleToExternalApi("Add-Rule", rule);
            if (!apiSuccess) return (false, apiMessage);

            rule.CreatedAt = DateTime.UtcNow;
            rule.UpdatedAt = DateTime.UtcNow;
            _context.CustomRules.Add(rule);
            await _context.SaveChangesAsync();

            return (true, "Rule created successfully");
        }

        public async Task<(bool success, string message)> UpdateRuleAsync(CustomRule rule)
        {
            var existingRule = await _context.CustomRules.FindAsync(rule.Id);
            if (existingRule == null) return (false, "Rule not found");

            var (apiSuccess, apiMessage) = await SendRuleToExternalApi("Add-Rule", rule);
            if (!apiSuccess) return (false, apiMessage);

            existingRule.Name = rule.Name;
            existingRule.Method = rule.Method;
            existingRule.Regex = rule.Regex;
            existingRule.Location = rule.Location;
            existingRule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Rule updated successfully");
        }

        public async Task<(bool success, string message)> DeleteRuleAsync(int id)
        {
            var rule = await _context.CustomRules.FindAsync(id);
            if (rule == null) return (false, "Rule not found");

            var (apiSuccess, apiMessage) = await SendDeleteToExternalApi(id);
            if (!apiSuccess) return (false, apiMessage);

            _context.CustomRules.Remove(rule);
            await _context.SaveChangesAsync();

            return (true, "Rule deleted successfully");
        }

        private async Task<(bool success, string message)> SendRuleToExternalApi(string endpoint, CustomRule rule)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var ruleData = new
                {
                    id = rule.Id + 1000,
                    method = rule.Method.ToUpper(),
                    location = rule.Location.ToLower(),
                    regex = rule.Regex
                };

                var json = JsonSerializer.Serialize(ruleData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/{endpoint}")
                {
                    Content = content,
                    Headers = { { "Host", "34.72.112.1" } }
                };

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"API returned {response.StatusCode}: {errorContent}");
                }

                return (true, "Success");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async Task<(bool success, string message)> SendDeleteToExternalApi(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{ApiBaseUrl}/Delete-Rule/{id}")
                {
                    Headers = { { "Host", "34.72.112.1" } }
                };

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"API returned {response.StatusCode}: {errorContent}");
                }

                return (true, "Success");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}