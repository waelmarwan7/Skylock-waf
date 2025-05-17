// Services/GeoIPControlsService.cs
using Grad_Project_Dashboard_1.Models;
using Grad_Project_Dashboard_1.Services;
using System.Net.Http;

public class GeoIPControlsService
{
    private readonly List<GeoIPControlsClass> _rules = new();
    private readonly HttpClient _client;
    private readonly GCloudManager _gCloudManager;

    public GeoIPControlsService(HttpClient client, GCloudManager gCloudManager)
    {
        _client = client;
        _gCloudManager = gCloudManager;
    }

    public void AddRule(GeoIPControlsClass rule)
    {
        Console.WriteLine("===== GeoIP Rule Info =====");
        Console.WriteLine($"Rule Name  : {rule.RuleName}");
        Console.WriteLine($"IPs        : {rule.IPs}");
        Console.WriteLine($"Countries  : {rule.Countries}");
        Console.WriteLine($"Priority   : {(rule.Priority.HasValue ? rule.Priority.ToString() : "None")}");
        Console.WriteLine($"Enabled    : {rule.Enabled}");
        Console.WriteLine("============================");

        string email = "admin@example.com"; // Replace with actual user email later
        string action = rule.Enabled ? "deny" : "allow";
        string priority = rule.Priority?.ToString() ?? "1000";

        _gCloudManager.ApplyFirewallRule(rule.IPs, email, action, rule.RuleName, priority);
        _rules.Add(rule);
    }

    public void EditRule(GeoIPControlsClass rule)
    {
        Console.WriteLine("=== Editing Rule ===");

        string email = "admin@example.com";
        string action = rule.Enabled ? "deny" : "allow";
        string priority = rule.Priority?.ToString() ?? "1000";

        // First delete the old rule
        _gCloudManager.DeleteFirewallRule(rule.RuleName);

        // Then re-create it with the updated data
        _gCloudManager.ApplyFirewallRule(rule.IPs, email, action, rule.RuleName, priority);

        // Update internal list
        var existing = _rules.FirstOrDefault(r => r.RuleName == rule.RuleName);
        if (existing != null)
        {
            existing.IPs = rule.IPs;
            existing.Countries = rule.Countries;
            existing.Priority = rule.Priority;
            existing.Enabled = rule.Enabled;
        }

        Console.WriteLine("=== Rule successfully updated ===");
    }

    public void DeleteRule(string ruleName)
    {
        Console.WriteLine("=== Deleting Rule ===");

        _gCloudManager.DeleteFirewallRule(ruleName);
        _rules.RemoveAll(r => r.RuleName == ruleName);

        Console.WriteLine("=== Deletion completed ===");
    }

    public List<GeoIPControlsClass> GetAllRules()
    {
        return _rules;
    }
}
