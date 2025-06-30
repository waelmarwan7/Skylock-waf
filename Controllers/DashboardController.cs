using Microsoft.AspNetCore.Mvc;
using Grad_Project_Dashboard_1.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
namespace Grad_Project_Dashboard_1.Controllers
{
    public class DashboardController : Controller
    {
        // Controllers/HomeController.cs

        private readonly Api_ResponseService _apiService;

        public DashboardController(Api_ResponseService apiService)
        {
            _apiService = apiService;
        }


        public async Task<IActionResult> Index()
        {


            return View();


        }

        [HttpGet]
        public async Task<IActionResult> RequestServer()
        {
            var apiResponse2 = await _apiService.GetSecurityDataAsync();

            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(apiResponse2));



            return Json(apiResponse2);
        }






        [HttpGet("Dashboard/TopBlocked")]
        public async Task<IActionResult> TopBlocked()
        {
            var data = await _apiService.GetTopBlockedAsync();
            return Json(data);
        }



    }
}




