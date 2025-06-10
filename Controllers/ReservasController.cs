using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;
using Reservas.Models;


namespace Reservas.Controllers
{
    public class ReservasController : Controller
    {
        private readonly BDContext _context;

        public ReservasController(BDContext context)
        {
            _context = context;
        }

        // GET: ReservasController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ReservasController/Calendario
        public ActionResult Calendario()
        {
            return View();
        }
        // GET: ReservasController/SeleccionarTipo
        [HttpGet]
        public async Task<IActionResult> SeleccionarTipo()
        {
            var tipos = await _context.ResourceTypes
                .Select(t => new { t.Id, Nombre = t.NameSpanish })
                .ToListAsync();

            ViewBag.TiposRecurso = tipos;
            return View();
        }


        // GET: ReservasController/ObtenerReservas
        [HttpGet]
        public async Task<IActionResult> ObtenerReservas()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Resource)
                .Select(b => new
                {
                    id = b.Id,
                    title = $"{b.Usuario} - {b.Resource.NameSpanish}",
                    start = b.FechaInicio,
                    end = b.FechaFin,
                    color = b.Estado == "Confirmada" ? "green" :
                            b.Estado == "Pendiente" ? "orange" : "red",
                    usuario = b.Usuario,
                    sala = b.Resource.NameSpanish,
                    estado = b.Estado,
                    fechaCreacion = b.FechaCreacion.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Json(bookings);
        }

        // GET: Crear reserva (con tipos)
        [HttpGet]
        public async Task<IActionResult> Create(int tipoId)
        {
            if (tipoId == 0)
                return RedirectToAction(nameof(SeleccionarTipo));

            var tipos = await _context.ResourceTypes
                .Select(t => new { t.Id, Nombre = t.NameSpanish })
                .ToListAsync();
            ViewBag.TiposRecurso = new SelectList(tipos, "Id", "Nombre");

            var salas = await _context.Resources
                .Where(r => r.ResourceTypeId == tipoId)
                .Select(r => new { r.Id, r.NameSpanish })
                .ToListAsync();
            ViewBag.Salas = new SelectList(salas, "Id", "NameSpanish");

            return View();
        }


        // POST: Crear reserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                booking.FechaCreacion = DateTime.Now;
                booking.Estado = "Pendiente";

                var sala = await _context.Resources.FindAsync(booking.ResourceId);
                booking.Sala = sala?.NameSpanish ?? "";
                booking.Usuario = "UsuarioEjemplo"; // Sustituir por usuario real si tienes auth

                _context.Add(booking);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Calendario));
            }

            var tipos = await _context.ResourceTypes
                .Select(t => new { t.Id, Nombre = t.NameSpanish })
                .ToListAsync();

            ViewBag.TiposRecurso = new SelectList(tipos, "Id", "Nombre");
            return View(booking);
        }

        // AJAX: Obtener salas por tipo
        [HttpGet]
        public async Task<IActionResult> GetSalasPorTipo(int tipoId)
        {
            var salas = await _context.Resources
                .Where(r => r.ResourceTypeId == tipoId)
                .Select(r => new { id = r.Id, nombre = r.NameSpanish })
                .ToListAsync();

            return Json(salas);
        }
    }
}


// CRUD automático (lo que ya tenías)
  /*  public ActionResult Details(int id) => View();
    public ActionResult Edit(int id) => View();
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, IFormCollection collection)
    {
        try { return RedirectToAction(nameof(Index)); }
        catch { return View(); }
    }
    public ActionResult Delete(int id) => View();
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Delete(int id, IFormCollection collection)
    {
        try { return RedirectToAction(nameof(Index)); }
        catch { return View(); }
    }*/
    

