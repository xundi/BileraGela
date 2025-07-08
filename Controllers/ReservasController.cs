using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;
using Reservas.Models;
using System.Security.Claims;


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

        [HttpGet]
        public async Task<IActionResult> Create(int recursoId)
        {
           
            var booking = new Booking
            {
                ResourceId = recursoId,
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddHours(1)
            };

            return View(booking);
        }



        // GET: Crear reserva (con tipos)
        /*[HttpGet]
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
        }*/

        // GET: Crear reserva directamente con recurso seleccionado
        /* [HttpGet]
         public IActionResult CreateDesdeRecurso(int recursoId)
         {
             var booking = new Booking
             {
                 ResourceId = recursoId
             };

             return View("Create", booking);
         }
        */



        // POST: Crear reserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            ModelState.Remove("Resource");
            ModelState.Remove("User");
            ModelState.Remove("Sala");
            ModelState.Remove("Usuario");

            // ✅ Obtener ID del usuario autenticado por su DNI
            var dniUsuario = User.Identity?.Name;
            var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Dni == dniUsuario);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Usuario no encontrado");
                var tipos = await _context.ResourceTypes
                    .Select(t => new { t.Id, Nombre = t.NameSpanish })
                    .ToListAsync();
                ViewBag.TiposRecurso = new SelectList(tipos, "Id", "Nombre");
                return View(booking);
            }

            booking.UserId = usuario.Id;

            if (ModelState.IsValid)
            {
                booking.FechaCreacion = DateTime.Now;
                booking.Estado = "Pendiente";
                booking.Usuario = dniUsuario ?? "Desconocido";

                var sala = await _context.Resources.FindAsync(booking.ResourceId);
                booking.Sala = sala?.NameSpanish ?? "Desconocida";

                _context.Add(booking);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "✅ Reserva guardada con éxito.";


                return RedirectToAction(nameof(MisReservas));
            }

            var tiposFallback = await _context.ResourceTypes
                .Select(t => new { t.Id, Nombre = t.NameSpanish })
                .ToListAsync();
            ViewBag.TiposRecurso = new SelectList(tiposFallback, "Id", "Nombre");
            return View(booking);
        }




        
        // GET: Reservas/MisReservas
        [HttpGet]
        public async Task<IActionResult> MisReservas(string sortOrder)
        {
            var dni = User.Identity?.Name;

            // ViewBag para controlar orden actual en la vista
            ViewBag.SortFechaInicio = String.IsNullOrEmpty(sortOrder) ? "fechainicio_desc" : "";
            ViewBag.SortFechaCreacion = sortOrder == "fechacreacion" ? "fechacreacion_desc" : "fechacreacion";
            ViewBag.SortEstado = sortOrder == "estado" ? "estado_desc" : "estado";
            ViewBag.SortRecurso = sortOrder == "recurso" ? "recurso_desc" : "recurso";
            ViewBag.SortTipo = sortOrder == "tipo" ? "tipo_desc" : "tipo";
            ViewBag.SortCentro = sortOrder == "centro" ? "centro_desc" : "centro";
            ViewBag.SortFin = sortOrder == "fechafin" ? "fechafin_desc" : "fechafin";


            var reservas = _context.Bookings
                .Include(b => b.Resource)
                    .ThenInclude(r => r.ResourceType)
                .Include(b => b.Resource.Center)
                .Where(b => b.Usuario == dni);

            // Orden dinámico
            reservas = sortOrder switch
            {
                "fechainicio_desc" => reservas.OrderByDescending(r => r.FechaInicio),
                "fechacreacion" => reservas.OrderBy(r => r.FechaCreacion),
                "fechacreacion_desc" => reservas.OrderByDescending(r => r.FechaCreacion),
                "estado" => reservas.OrderBy(r => r.Estado),
                "estado_desc" => reservas.OrderByDescending(r => r.Estado),
                "recurso" => reservas.OrderBy(r => r.Resource.NameSpanish),
                "recurso_desc" => reservas.OrderByDescending(r => r.Resource.NameSpanish),
                "tipo" => reservas.OrderBy(r => r.Resource.ResourceType.NameSpanish),
                "tipo_desc" => reservas.OrderByDescending(r => r.Resource.ResourceType.NameSpanish),
                "centro" => reservas.OrderBy(r => r.Resource.Center.NameSpanish),
                "centro_desc" => reservas.OrderByDescending(r => r.Resource.Center.NameSpanish),
                "fechafin" => reservas.OrderBy(r => r.FechaFin),
                "fechafin_desc" => reservas.OrderByDescending(r => r.FechaFin),
                _ => reservas.OrderBy(r => r.FechaInicio),
            };

            return View(await reservas.ToListAsync());
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


