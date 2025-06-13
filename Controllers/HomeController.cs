using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        [HttpGet]
        public async Task<IActionResult> GetTiposPorCentro(int centroId)
        {
            var tipos = await _context.Resources
                .Where(r => r.CenterId == centroId)
                .Select(r => r.ResourceType)
                .Distinct()
                .Select(t => new { id = t.Id, nombre = t.NameSpanish })
                .ToListAsync();

            return Json(tipos);
        }
        [HttpGet]
        public async Task<IActionResult> GetRecursosPorCentroYTipo(int centroId, int tipoId)
        {
            var recursos = await _context.Resources
                .Where(r => r.CenterId == centroId && r.ResourceTypeId == tipoId)
                .Select(r => new { id = r.Id, nombre = r.NameSpanish })
                .ToListAsync();

            return Json(recursos);
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

            // Carga centros
            var centros = await _context.Centers
                .Select(c => new { c.Id, c.NameSpanish })
                .ToListAsync();
            ViewBag.Centros = new SelectList(centros, "Id", "NameSpanish");

            // Ya no necesitas cargar todos los tipos globales aquí,
            // se cargarán dinámicamente según el centro
            return View();
        }





    }
}

