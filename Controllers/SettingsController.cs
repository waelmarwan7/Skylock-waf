using Microsoft.AspNetCore.Mvc;
using Grad_Project_Dashboard_1.Services;
using Grad_Project_Dashboard_1.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grad_Project_Dashboard_1.Controllers
{
    public class SettingsController : Controller
    {

    [HttpGet]
        public async Task<IActionResult> Index()
        {
            
            return View(); 
        }
        
    }
}
