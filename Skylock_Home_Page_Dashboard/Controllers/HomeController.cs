using Microsoft.AspNetCore.Mvc;
namespace Grad_Project_Dashboard_1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
