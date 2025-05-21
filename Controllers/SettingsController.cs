using Microsoft.AspNetCore.Mvc;
using Grad_Project_Dashboard_1.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Grad_Project_Dashboard_1.Controllers
{
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public SettingsController(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Json(new { success = false, message = "Username cannot be empty" });
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Start transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                user.Username = username;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Username updated successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Error updating username: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(
            string currentPassword,
            string newPassword,
            string confirmPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                return Json(new { success = false, message = "All password fields are required" });
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "New passwords do not match" });
            }

            if (newPassword.Length < 6)
            {
                return Json(new { success = false, message = "Password must be at least 6 characters" });
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (!VerifyPassword(user, currentPassword))
            {
                return Json(new { success = false, message = "Current password is incorrect" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                user.Password = _passwordHasher.HashPassword(user, newPassword);
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Password updated successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Error updating password: {ex.Message}" });
            }
        }

        private async Task<User> GetCurrentUserAsync()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return null;

            return await _context.Users
                .AsTracking() // Ensure entity is tracked
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        private bool VerifyPassword(User user, string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.Password,
                providedPassword);

            return result == PasswordVerificationResult.Success;
        }

        public class PasswordUpdateModel
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
            return View(); 
        }
    }
}