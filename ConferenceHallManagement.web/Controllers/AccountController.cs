using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using BLL_ConferenceHallManagement;
using ConferenceHallManagement.Web.Services;
using Models_ConferenceHallManagement.Extensions;
using System.Security.Claims;

namespace ConferenceHallManagement.Web.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly IBllAuthenticationService _authService;

        public AccountController(IBllAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpGet("Login")]
        public async Task<IActionResult> LoginGet([FromQuery] string userId, [FromQuery] string password)
        {
            return await ProcessLogin(userId, password);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginPost([FromForm] string userId, [FromForm] string password)
        {
            var result = await ProcessLogin(userId, password);
            
            // For POST, return JSON
            if (result is JsonResult)
                return result;
            
            return result;
        }

        private async Task<IActionResult> ProcessLogin(string userId, string password)
        {
            try
            {
                // 1. Validate and authenticate user
                var loginResult = await _authService.LoginAsync(userId, password);
                
                if (!loginResult.Success)
                {
                    return Redirect($"/login?error={Uri.EscapeDataString(loginResult.Message)}");
                }

                // 2. Convert user session to claims
                var claims = ClaimsHelper.BuildClaimsFromSession(loginResult.UserSession!);

                // 3. Cookie Sign-In
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // 4. Return success with redirect URL
                var redirectUrl = GetRedirectByRole(loginResult.UserSession!.GetPrimaryRoleName());
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
                return Redirect($"/login?error={Uri.EscapeDataString("Login failed: " + ex.Message)}");
            }
        }

        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/login");
        }

        private string GetRedirectByRole(string primaryRole)
        {
            return primaryRole switch
            {
                "Super Admin" => "/",
                "Central Admin" => "/",
                "Regional Admin" => "/",
                "Station Admin" => "/",
                _ => "/"
            };
        }
    }
}