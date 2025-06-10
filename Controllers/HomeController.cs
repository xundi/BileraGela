using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;

namespace Reservas.Controllers
{
    

    public class HomeController : Controller
    {
        private readonly BDContext _context;

        public HomeController(BDContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            int? tipoUsuario = HttpContext.Session.GetInt32("UserTypeId");

            return tipoUsuario switch
            {
                3 => RedirectToAction("PanelGestion", "Home"),    // Administrador
                2 => RedirectToAction("PanelValidador", "Home"),  // Validador de reservas
                1 => RedirectToAction("Panel", "Home"),           // Usuario normal
                _ => View()                                       // Si no está autenticado
            };
        }

        public IActionResult PanelGestion()
        {
            int? tipoUsuario = HttpContext.Session.GetInt32("UserTypeId");

            if (tipoUsuario != 3)
                return Unauthorized();

            return View();
        }

        public IActionResult PanelValidador()
        {
            int? tipoUsuario = HttpContext.Session.GetInt32("UserTypeId");

            if (tipoUsuario != 2)
                return Unauthorized();

            return View();
        }

        /*public IActionResult Panel()
        {
            int? tipoUsuario = HttpContext.Session.GetInt32("UserTypeId");

            if (tipoUsuario != 1)
                return Unauthorized();

            return View();
        }*/

        public async Task<IActionResult> Panel()
        {
            int tipoUsuario = HttpContext.Session.GetInt32("UserTypeId") ?? 0;
            if (tipoUsuario != 1)
                return Unauthorized();

            // PASO CLAVE: usamos un modelo fuerte
            var tipos = await _context.ResourceTypes.ToListAsync();

            ViewBag.TiposRecurso = tipos;

            return View();
        }




    }
}

