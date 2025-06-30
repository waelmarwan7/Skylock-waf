using Grad_Project_Dashboard_1.Models;
using Grad_Project_Dashboard_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Grad_Project_Dashboard_1.Controllers
{
    public class UserDomainsController : Controller
    {
        private readonly AppDbContext _context = new AppDbContext();
        private readonly IUserDomainService _domainService;

        public UserDomainsController(IUserDomainService domainService)
        {
            _domainService = domainService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.Users
                .Include(u => u.DomainNames)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
[HttpPost]
[ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDomain(int id)
        {
            Console.WriteLine("\n\n\n\n\n "+id);
            var (success, message) = await _domainService.DeleteDomainAsync(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDomain(string Domain, string IPAddress)
        {
            UserDomainName domain = new UserDomainName
            {
                Domain = Domain,
                IPAddress = IPAddress,
            };
            var (success, message) = await _domainService.AddDomainAsync(domain);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDomain(int Id, string Domain, string IPAddress)
        {
            UserDomainName domain = new UserDomainName
            {
                Id = Id,
                Domain = Domain,
                IPAddress = IPAddress,
            };
            var (success, message) = await _domainService.UpdateDomainAsync(domain);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}