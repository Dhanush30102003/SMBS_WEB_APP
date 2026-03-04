using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMBS_SERVER.Services;
using System.Security.Claims;
using AuthService = SMBS_SERVER.Services.AuthenticationService;

namespace SMBS_SERVER.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly JwtService _jwtService;

        public AuthController(AuthService authService, JwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            Console.WriteLine("==== MVC LOGIN HIT ====");
            Console.WriteLine($"USERNAME: '{username}'");
            Console.WriteLine($"PASSWORD: '{password}'");


            var result = _authService.Login(username, password, tenantId: 1);

            Console.WriteLine($"RESULT: {result.Success}");
            Console.WriteLine($"MESSAGE: {result.Message}");

            if (!result.Success)
            {
                ViewBag.Error = result.Message;
                return View();
            }

            var user = result.User;

            // 🔹 MAP RoleID → Role Name (MATCHES [Authorize(Roles="Admin")])
            string roleName = user.RoleID switch
            {
                1 => "Admin",
                2 => "Manager",
                3 => "Cashier",
                _ => "User"
            };

            var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
    new Claim(ClaimTypes.Name, user.UserName),
    new Claim(ClaimTypes.Role, roleName),   // ✅ FIX
    new Claim("TenantID", user.TenantID.ToString())
};


            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
   CookieAuthenticationDefaults.AuthenticationScheme,   // ✅ MUST BE COOKIE
   new ClaimsPrincipal(identity),
   new AuthenticationProperties
   {
       IsPersistent = true,
       ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
   }
);


            var token = _jwtService.GenerateToken(user);

            Response.Cookies.Append("SMBS_TOKEN", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            ); Response.Cookies.Delete("SMBS_TOKEN");

            return RedirectToAction("Login", "Auth");
        }
    }
}
