using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Grad_Project_Dashboard_1.Models;
using Grad_Project_Dashboard_1.Services;

namespace Grad_Project_Dashboard_1.Controllers
{
    public class AccountController : Controller
    {

        private readonly AppDbContext _context = new AppDbContext();
        private readonly UserSignup _userSignup;

        // Inject both dependencies through constructor
        public AccountController(UserSignup userSignup)
        {
            _userSignup = userSignup;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
// maged's modification
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (ModelState.IsValid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

        if (user != null && user.Password == model.Password)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("IPAddress", user.IPAddress ?? ""),
                new Claim("IPInstance", user.IPInstance ?? "") // Make sure this exists in your User model
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            if (model.DeleteAfterLogin)
            {
                return RedirectToAction("Remove", new {email = user.Email });
            }

            return RedirectToAction("Index", "Dashboard");
        }
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
    }
    return View(model);
}

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password, // In production, hash this password
                    IPAddress = model.IPAddress,
                    DomainName = model.DomainName

                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Post", new { email = user.Email, ip = user.IPAddress , id = user.Id });
                //return RedirectToAction("Login");
                // should return to login page agian !!!
            }

            return View(model);
        }
        /// <summary>
        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        // Signup Cloud wise
        [HttpGet]
        public async Task<IActionResult> Post(string Email , string IPAddress , int id)
        {
            try
            {
                var result = await _userSignup.ProcessSignupAsync(Email, IPAddress , id);

                if (result.Success)
                {
                    return Ok(result);
                }
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SignupResult
                {
                    Success = false,
                    Message = "An unexpected error occurred during signup",
                    UserName = Email.Split('@')[0].ToLower()
                });
            }
        }


        // Clear up Cloud Wise
[HttpGet]
public async Task<IActionResult> Remove()
{
    try
    {
        // Get current user's email from claims
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new CleanupResult 
            {
                Success = false,
                Message = "User not authenticated"
            });
        }

        // Find user in database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return BadRequest(new CleanupResult 
            {
                Success = false,
                Message = "User not found"
            });
        }

        // Cleanup resources
        var result = await _userSignup.CleanupUserResourcesAsync(email);

        // Remove user from database
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Sign out the user
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new CleanupResult
        {
            Success = false,
            Message = "An unexpected error occurred during cleanup"
        });
    }
}


    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        // Sign out of authentication
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        return RedirectToAction("Index", "Home");
    }

        
    }
}