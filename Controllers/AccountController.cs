using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Grad_Project_Dashboard_1.Models;
using Grad_Project_Dashboard_1.Services;
using System.Text.Json;

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
        public IActionResult Login(string message = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                TempData["SuccessMessage"] = message;
            }
            return View();
        }

        public IActionResult Loading()
        {
            return View("LoadingPage");
        }

// maged's modification
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (ModelState.IsValid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.IPInstance !="null-ip");

        if (user != null && user.Password == model.Password)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // This is the crucial line
                new Claim("IPInstance", user.IPInstance)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity));

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
        // Store the registration data in TempData to use after showing loading page
        TempData["RegisterModel"] = JsonSerializer.Serialize(model);
        return View("LoadingPage");
    }
    return View(model);
}

[HttpGet]
public async Task<IActionResult> ProcessRegistration()
{
    if (!TempData.ContainsKey("RegisterModel"))
    {
        TempData["ErrorMessage"] = "Registration data not found. Please try again.";
        return RedirectToAction("Register");
    }

    try
    {
        var model = JsonSerializer.Deserialize<RegisterViewModel>(TempData["RegisterModel"].ToString());
        Console.WriteLine("after model");
        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            Password = model.Password,
        };Console.WriteLine("after user");

        _context.Users.Add(user);
        Console.WriteLine("after adding user");
        await _context.SaveChangesAsync();
        Console.WriteLine("before cloud1");
        // Process the signup
        var result = await _userSignup.ProcessSignupAsync(user.Email, user.Id);
        
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Registration successful! Please login with your credentials.";
            return RedirectToAction("Login");
        }
        
        TempData["ErrorMessage"] = "Registration failed. Please try again.";
        return RedirectToAction("Register");
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = "An error occurred during registration. Please try again.";
        return RedirectToAction("Register");
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