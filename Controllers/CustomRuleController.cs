
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