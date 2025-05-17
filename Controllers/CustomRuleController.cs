// using Azure.Core;
// using Grad_Project_Dashboard_1.Models;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System.Net.Http;
// using System.Text;
// using System.Text.Json;

// namespace Grad_Project_Dashboard_1.Controllers
// {
//     public class CustomRuleController : Controller
//     {
//         private readonly AppDbContext _context = new AppDbContext();
//         private readonly IHttpClientFactory _httpClientFactory;
//         private const string ApiBaseUrl = "https://try.hackmeagain.tech";

//         public CustomRuleController( IHttpClientFactory httpClientFactory)
//         {
//             _httpClientFactory = httpClientFactory;
//         }

//         public IActionResult Index()
//         {
//             List<CustomRule> customs = _context.CustomRules.ToList();
//             return View(customs);
//         }

//         [HttpPost]
//         public async Task<IActionResult> CreateRule(CustomRule rule)
//         {
//             //if (!ModelState.IsValid)
//             //{
//             //    TempData["ErrorMessage"] = "Invalid rule data";
//             //    return RedirectToAction("Index");
//             //}

//             // First try to create in external API
//             var (success, message) = await SendRuleToExternalApi("Add-Rule", rule);
//             if (!success)
//             {
//                 TempData["ErrorMessage"] = $"Failed to create rule in API: {message}";
//                 return RedirectToAction("Index");
//             }

//             // Only if API succeeds, save to local database
//             rule.CreatedAt = DateTime.UtcNow;
//             rule.UpdatedAt = DateTime.UtcNow;
//             _context.CustomRules.Add(rule);
//             await _context.SaveChangesAsync();

//             TempData["SuccessMessage"] = "Rule created successfully";
//             return RedirectToAction("Index");
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Delete(int id)
//         {
//             var rule = await _context.CustomRules.FindAsync(id);
//             if (rule == null)
//             {
//                 TempData["ErrorMessage"] = "Rule not found";
//                 return RedirectToAction(nameof(Index));
//             }

//             // First try to delete in external API
//             var (success, message) = await SendDeleteToExternalApi(id);
//             if (!success)
//             {
//                 TempData["ErrorMessage"] = $"Failed to delete rule in API: {message}";
//                 return RedirectToAction(nameof(Index));
//             }

//             // Only if API succeeds, delete from local database
//             _context.CustomRules.Remove(rule);
//             await _context.SaveChangesAsync();

//             TempData["SuccessMessage"] = "Rule deleted successfully";
//             return RedirectToAction(nameof(Index));
//         }

//         [HttpPost("/CustomRule/UpdateRule")]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> UpdateRule([FromForm] CustomRule rule)
//         {
//             //if (!ModelState.IsValid)
//             //{
//             //    TempData["ErrorMessage"] = "Invalid rule data";
//             //    return RedirectToAction(nameof(Index));
//             //}

//             var existingRule = await _context.CustomRules.FindAsync(rule.Id);
//             if (existingRule == null)
//             {
//                 TempData["ErrorMessage"] = "Rule not found";
//                 return RedirectToAction(nameof(Index));
//             }

//             // First try to update in external API
//             var (success, message) = await SendRuleToExternalApi("Add-Rule", rule);
//             if (!success)
//             {
//                 TempData["ErrorMessage"] = $"Failed to update rule in API: {message}";
//                 return RedirectToAction(nameof(Index));
//             }

//             // Only if API succeeds, update local database
//             existingRule.Name = rule.Name;
//             existingRule.Method = rule.Method;
//             existingRule.Regex = rule.Regex;
//             existingRule.Location = rule.Location;
//             existingRule.UpdatedAt = DateTime.UtcNow;

//             await _context.SaveChangesAsync();

//             TempData["SuccessMessage"] = "Rule updated successfully";
//             return RedirectToAction(nameof(Index));
//         }

//         private async Task<(bool success, string message)> SendRuleToExternalApi(string endpoint, CustomRule rule)
//         {
//             try
//             {
//                 var client = _httpClientFactory.CreateClient();
//                 var ruleData = new
//                 {
//                     id = rule.Id+1000,
//                     method = rule.Method.ToUpper(),
//                     location = rule.Location.ToLower(),
//                     regex = rule.Regex
//                 };

//                 var json = JsonSerializer.Serialize(ruleData);
//                 var content = new StringContent(json, Encoding.UTF8, "application/json");

//                 // Create request with Host header
//                 var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/{endpoint}")
//                 {
//                     Content = content,
//                     Headers = 
//                     {
//                         { "Host", "34.72.112.1" }
//                     }
//                 };

//                 var response = await client.SendAsync(request);

//                 if (!response.IsSuccessStatusCode)
//                 {
//                     var errorContent = await response.Content.ReadAsStringAsync();
//                     return (false, $"API returned {response.StatusCode}: {errorContent}");
//                 }

//                 return (true, "Success");
//             }
//             catch (Exception ex)
//             {
//                 return (false, ex.Message);
//             }
//         }

//         private async Task<(bool success, string message)> SendDeleteToExternalApi(int id)
//         {
//             try
//             {
//                 var client = _httpClientFactory.CreateClient();

//                 // Create request with Host header
//                 var request = new HttpRequestMessage(HttpMethod.Delete, $"{ApiBaseUrl}/Delete-Rule/{id}")
//                 {
//                     Headers =
//                     {
//                         { "Host", "34.72.112.1" }
//                     }
//                 };

//                 var response = await client.SendAsync(request);

//                 if (!response.IsSuccessStatusCode)
//                 {
//                     var errorContent = await response.Content.ReadAsStringAsync();
//                     return (false, $"API returned {response.StatusCode}: {errorContent}");
//                 }

//                 return (true, "Success");
//             }
//             catch (Exception ex)
//             {
//                 return (false, ex.Message);
//             }
//         }

//                 private bool RuleExists(int id)
//                 {
//                     return _context.CustomRules.Any(e => e.Id == id);
//                 }
//     }
// }

// Controllers/CustomRuleController.cs
using Azure.Core;
using Grad_Project_Dashboard_1.Models;
using Grad_Project_Dashboard_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;
namespace Grad_Project_Dashboard_1.Controllers
{
    public class CustomRuleController : Controller
    {
        private readonly CustomRuleService _ruleService;

        public CustomRuleController(CustomRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        public async Task<IActionResult> Index()
        {
            var rules = await _ruleService.GetAllRulesAsync();
            return View(rules);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRule(CustomRule rule)
        {
            var (success, message) = await _ruleService.CreateRuleAsync(rule);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _ruleService.DeleteRuleAsync(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/CustomRule/UpdateRule")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRule([FromForm] CustomRule rule)
        {
            var (success, message) = await _ruleService.UpdateRuleAsync(rule);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}