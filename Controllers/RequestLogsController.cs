using Microsoft.AspNetCore.Mvc;
using Grad_Project_Dashboard_1.Services;
using Grad_Project_Dashboard_1.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grad_Project_Dashboard_1.Controllers
{
    public class RequestLogsController : Controller
    {
        private readonly RequestLogsService _logService;

        public RequestLogsController(RequestLogsService logService)
        {
            _logService = logService;
        }

    [HttpGet]
        public async Task<IActionResult> Index()
        {
            

            return View(); // Make sure your view expects List<LogEntry>
        }
        
         [HttpGet]
        public async Task<IActionResult> RequestServer()
        {
  var logs = await _logService.GetLogsFromFile("Requests");
   

    // ✅ Print logs to console
    


           return Json(logs); // Make sure your view expects List<LogEntry>
        }
    }
}
