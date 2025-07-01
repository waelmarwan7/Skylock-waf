using Microsoft.AspNetCore.Mvc;
using Grad_Project_Dashboard_1.Services;
using Grad_Project_Dashboard_1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
namespace Grad_Project_Dashboard_1.Controllers

{
    public class RateLimitingController : Controller
    {
        private readonly RateLimitingService _rateLimitService;

        public RateLimitingController(RateLimitingService rateLimitService)
        {
            _rateLimitService = rateLimitService; // In production, use dependency injection
        }

        [HttpGet]

     
        public async Task<IActionResult> Index()
        {
          
            
            return View();


        
        }


        [HttpPost]
        public async Task<ActionResult> Submit([FromBody]RateLimitingClass model)
        {
            if (!ModelState.IsValid)
                return  BadRequest(ModelState);

            bool success = await _rateLimitService.SendRateLimitSettingsAsync(model);

            Console.WriteLine(success);
        string Message = success ? "Settings sent successfully." : "Failed to send settings.";

          return Ok(new { Message });
        }
    }
}
