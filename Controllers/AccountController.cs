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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

                if (user != null && user.Password == model.Password) // In production, use password hashing
                {
                    // creating session
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("IPAddress", user.IPAddress ?? "")
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);


                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));


                    // Check if the delete checkbox was checked
                    if (model.DeleteAfterLogin)  // This will be true if checkbox was checked
                    {
                        // Delete account logic
                        //_context.Users.Remove(user);
                        //await _context.SaveChangesAsync();

                        // Redirect to confirmation page
                        return RedirectToAction("Remove", new {email = user.Email });
                    }

                    // Normal login logic for when checkbox wasn't checked
                    // (Your existing login success code)
                    //return RedirectToAction("Index", "Home");
                    return RedirectToAction("Post", new { email = user.Email, ip = user.IPAddress });
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

                return RedirectToAction("Post", new { email = user.Email, ip = user.IPAddress });
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
        public async Task<IActionResult> Post(string Email , string IPAddress)
        {
            try
            {
                var result = await _userSignup.ProcessSignupAsync(Email, IPAddress);

                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
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
        public async Task<IActionResult> Remove( string Email)
        {

            try
            {
                var result = await _userSignup.CleanupUserResourcesAsync(Email);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CleanupResult
                {
                    Success = false,
                    Message = "An unexpected error occurred during cleanup",
                    UserName = Email.Split('@')[0].ToLower()
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        
    }
}