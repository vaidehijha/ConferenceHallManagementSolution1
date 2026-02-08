using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Repository_ConferenceHallManagement.UtilityRepository; // Repository ka reference
using Models_ConferenceHallManagement.AppDbModels; // Models ka reference

namespace ConferenceHallManagement.Web.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly IEmployeeRepository _empRepo;

        // Constructor me Repository Inject ki
        public AccountController(IEmployeeRepository empRepo)
        {
            _empRepo = empRepo;
        }

        [HttpGet("Login")]
        public async Task<IActionResult> Login(string userId, string password)
        {
            // 1. Check karein user DB me hai ya nahi (Password ignored as per requirement)
            bool isValid = await _empRepo.AuthenticateUser(userId, password);

            if (!isValid)
            {
                return Redirect("/login?error=Invalid Employee ID");
            }

            // 2. User ki details layein (Name, Email, Mobile etc.)
            var userDetails = await _empRepo.GetEmployeeById(userId);
            if (userDetails == null) return Redirect("/login?error=User details not found");

            // 3. User ke Roles layein
            var roles = await _empRepo.GetEmployeeRoles(userId);

            // 4. Claims List banayein (Cookie ka data)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userDetails.Empname ?? "Unknown"),
                new Claim(ClaimTypes.NameIdentifier, userDetails.EightDigitEmpNo),
                new Claim(ClaimTypes.MobilePhone, userDetails.Cellno ?? ""),
                new Claim(ClaimTypes.Email, userDetails.Pgemail ?? ""),
                // Extra tracking claims
                new Claim("EmpImage", userDetails.Empimgguid?.ToString() ?? ""),
            };

            // 5. Roles ko Claims me add karein aur Primary Role decide karein
            string primaryRole = "User"; // Default role

            if (roles != null && roles.Any())
            {
                foreach (var role in roles)
                {
                    // MasterRole table se Role ka naam nikala (e.g. "SuperAdmin")
                    string roleName = role.MasterRole?.RoleName ?? "User";

                    // Cookie me Role add kiya
                    claims.Add(new Claim(ClaimTypes.Role, roleName));

                    // Redirection ke liye priority set ki (Taaki sahi dashboard par bhejein)
                    // Agar banda SuperAdmin hai, toh wo priority lega
                    if (roleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)) primaryRole = "SuperAdmin";
                    else if (roleName.Equals("CentralAdmin", StringComparison.OrdinalIgnoreCase) && primaryRole != "SuperAdmin") primaryRole = "CentralAdmin";
                    else if (roleName.Equals("RegionalAdmin", StringComparison.OrdinalIgnoreCase) && primaryRole != "SuperAdmin" && primaryRole != "CentralAdmin") primaryRole = "RegionalAdmin";
                    else if (roleName.Equals("StationAdmin", StringComparison.OrdinalIgnoreCase) && primaryRole == "User") primaryRole = "StationAdmin";
                }
            }
            else
            {
                // Agar DB me koi role nahi mila, toh default 'User'
                claims.Add(new Claim(ClaimTypes.Role, "User"));
            }

            // 6. Cookie Sign-In Process
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

            // 7. Role ke hisaab se Redirect karein
            // Note: Ye URL names tumhare Pages folder ke hisaab se hone chahiye
            switch (primaryRole)
            {
                case "SuperAdmin":
                    return Redirect("/super-admin-dashboard"); // Example URL
                case "CentralAdmin":
                    return Redirect("/central-admin-dashboard");
                case "RegionalAdmin":
                    return Redirect("/regional-admin-dashboard");
                case "StationAdmin":
                    return Redirect("/station-admin-dashboard");
                default:
                    return Redirect("/"); // Normal User goes to Home
            }
        }

        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/login");
        }
    }
}