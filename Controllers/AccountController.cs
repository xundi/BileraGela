using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Asegúrate de tener esto para .Include
using Reservas.Context;
using Reservas.Models;
using Reservas.Models.ViewModels;
using System.Security.Claims;

namespace Reservas.Controllers
{
    // Cambio account controller
    public class AccountController : Controller
    {
        private readonly BDContext _context;

        public AccountController(BDContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 🔎 Incluimos el tipo de usuario
            var user = await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Dni.ToLower() == model.Dni.ToLower());

            if (user == null)
            {
                ModelState.AddModelError("", "DNI no encontrado.");
                return View(model);
            }

            // ✅ Guardar tipo de usuario y ID en sesión
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetInt32("UserTypeId", user.UserTypeId);

            // ✅ Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Dni),
                new Claim(ClaimTypes.Role, user.UserType?.Name ?? "Usuario")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // 👈 Limpieza adicional
            return RedirectToAction("Login", "Account");
        }
    }
}
