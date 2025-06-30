using Grad_Project_Dashboard_1.Models;
using Grad_Project_Dashboard_1.Services;
using System.Net.Http;
using Microsoft.Extensions.Logging;

public class GeoIPControlsService
{
    private readonly List<GeoIPControlsClass> _rules = new();
    private readonly HttpClient _client;
    private readonly GCloudManager _gCloudManager;
    private readonly ILogger<GeoIPControlsService> _logger;

    public GeoIPControlsService(
        HttpClient client, 
        GCloudManager gCloudManager,
        ILogger<GeoIPControlsService> logger)
    {
        _client = client;
        _gCloudManager = gCloudManager;
        _logger = logger;
    }

    public void AddRule(GeoIPControlsClass rule)
    {
        try
        {
            if (rule == null || string.IsNullOrWhiteSpace(rule.RuleName))
                throw new ArgumentException("Invalid rule data");

            _logger.LogInformation("===== Adding GeoIP Rule =====");
            _logger.LogInformation($"Rule Name: {rule.RuleName}");
            _logger.LogInformation($"IPs: {rule.IPs}");
            _logger.LogInformation($"Countries: {rule.Countries}");
            _logger.LogInformation($"Priority: {rule.Priority?.ToString() ?? "None"}");
            _logger.LogInformation($"Enabled: {rule.Enabled}");

            string email = "admin@example.com";
            string action = rule.Enabled ? "deny" : "allow";
            string priority = rule.Priority?.ToString() ?? "1000";

            _gCloudManager.ApplyFirewallRule(
                rule.IPs, 
                email, 
                action, 
                rule.RuleName, 
                priority);

            _rules.Add(rule);
            _logger.LogInformation("===== Rule Added Successfully =====");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding rule");
            throw;
        }
    }

    public void EditRule(GeoIPControlsClass rule)
    {
        try
        {
            if (rule == null || string.IsNullOrWhiteSpace(rule.RuleName))
                throw new ArgumentException("Invalid rule data");

            _logger.LogInformation($"=== Editing Rule: {rule.RuleName} ===");

            string email = "admin@example.com";
            string action = rule.Enabled ? "deny" : "allow";
            string priority = rule.Priority?.ToString() ?? "1000";

            // Delete old rule
            _gCloudManager.DeleteFirewallRule(rule.RuleName);

            // Create updated rule
            _gCloudManager.ApplyFirewallRule(rule.IPs, email, action, rule.RuleName, priority);

            // Update local list
            var existing = _rules.FirstOrDefault(r => r.RuleName == rule.RuleName);
            if (existing != null)
            {
                existing.IPs = rule.IPs;
                existing.Countries = rule.Countries;
                existing.Priority = rule.Priority;
                existing.Enabled = rule.Enabled;
            }

            _logger.LogInformation("=== Rule Updated Successfully ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error editing rule: {rule?.RuleName}");
            throw;
        }
    }

    public void DeleteRule(string ruleName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ruleName))
                throw new ArgumentNullException(nameof(ruleName));

            _logger.LogInformation($"=== Deleting Rule: {ruleName} ===");

            if (!_rules.Any(r => r.RuleName == ruleName))
            {
                _logger.LogWarning($"Rule '{ruleName}' not found in local collection");
            }

            _gCloudManager.DeleteFirewallRule(ruleName);
            _rules.RemoveAll(r => r.RuleName == ruleName);

            _logger.LogInformation($"=== Rule '{ruleName}' Deleted Successfully ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting rule: {ruleName}");
            throw;
        }
    }

    public List<GeoIPControlsClass> GetAllRules()
    {
        try
        {
            return _rules.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all rules");
            throw;
        }
    }
}