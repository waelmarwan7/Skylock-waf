using Microsoft.AspNetCore.Mvc;
using Grad_Project_Dashboard_1.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Grad_Project_Dashboard_1.Services;

namespace Grad_Project_Dashboard_1.Controllers
{
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context = new AppDbContext();
        private readonly UserSignup _userSignup;

        // Inject both dependencies through constructor
        public SettingsController(UserSignup userSignup)
        {
            _userSignup = userSignup;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new SettingsViewModel
            {
                Username = user.Username
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUsername(UsernameUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return current username in the view
                var settingsView = new SettingsViewModel
                {
                    Username = model.Username
                };
                return View("Index", settingsView);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                user.Username = model.Username;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Username updated successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating username: " + ex.Message;
                return View("Index", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(PasswordUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return current username in the view
                var currentUser = await GetCurrentUserAsync();
                var settingsView = new SettingsViewModel
                {
                    Username = currentUser?.Username
                };
                return View("Index", settingsView);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (!VerifyPassword(user, model.CurrentPassword))
                {
                    TempData["ErrorMessage"] = "Current password is incorrect";
                    return View("Index", model);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    TempData["ErrorMessage"] = "New passwords do not match";
                    return View("Index", model);
                }

                user.Password = model.NewPassword;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Password updated successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating password: " + ex.Message;
                return View("Index", model);
            }
        }

        private bool VerifyPassword(User user, string providedPassword)
        {
            return user.Password == providedPassword;
        }


[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Unsubscribe()
{
    // Just show the loading page - the actual unsubscription will happen via JavaScript
    return View("UnsubscribeLoading");
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Remove()
{
    try
    {
        Console.WriteLine("this is me hshs");
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        // Cleanup resources
        var result = await _userSignup.CleanupUserResourcesAsync(user.Email);

        // Remove user from database
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Sign out the user
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var redirectUrl = Url.Action("Login", "Account", new { message = "You have successfully unsubscribed." });
        return Json(new { success = true, redirectUrl });
    }
    catch (Exception ex)
    {
        return Json(new { 
            success = false, 
            message = "Error during unsubscription: " + ex.Message,
            redirectUrl = Url.Action("Index", "Settings") 
        });
    }
}

        private async Task<User> GetCurrentUserAsync()
        {
            // First try to get the user ID from the NameIdentifier claim
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // If not found, try alternative claim names (some schemes use different names)
            if (string.IsNullOrEmpty(userIdString))
            {
                userIdString = User.FindFirst("sub")?.Value; // Alternative claim name
            }

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                // Log the claims for debugging
                Console.WriteLine("Available claims:");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"{claim.Type}: {claim.Value}");
                }
                return null;
            }

            return await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

    }
}