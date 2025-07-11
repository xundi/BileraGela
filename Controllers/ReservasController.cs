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
        public async Task<IActionResult> SeleccionarRecurso()
        {
            var tipos = await _context.ResourceTypes.ToListAsync();
            ViewBag.TiposRecurso = tipos;
            return View();
        }




        // GET: Reservas/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? centroId, int? tipoId, int? recursoId)
        {
            if (recursoId == null)
            {
                TempData["Error"] = "⚠️ Debes seleccionar un recurso válido.";
                return RedirectToAction("SeleccionarRecurso"); // O redirige donde prefieras
            }

            var recurso = await _context.Resources
                .Include(r => r.Center)
                .Include(r => r.ResourceType)
                .FirstOrDefaultAsync(r => r.Id == recursoId);

            if (recurso == null)
                return NotFound();

            var booking = new Booking
            {
                ResourceId = recurso.Id,
                Sala = recurso.NameSpanish,
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddHours(1) // Por ejemplo, una hora más tarde
            };

            return View(booking);
        }








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

            if (booking.FechaFin <= booking.FechaInicio)
            {
                ModelState.AddModelError("", "❌ La fecha de fin no puede ser menor o igual a la de inicio.");
                return View(booking);
            }


            if (ModelState.IsValid)
            {
                booking.FechaCreacion = DateTime.Now;
                booking.Estado = "Pendiente";
                booking.Usuario = dniUsuario ?? "Desconocido";

                var sala = await _context.Resources.FindAsync(booking.ResourceId);
                booking.Sala = sala?.NameSpanish ?? "Desconocida";

                // Validar solapamiento (NO permitir doble reserva misma sala/día/hora)
                bool hayConflicto = await _context.Bookings.AnyAsync(b =>
                    b.ResourceId == booking.ResourceId &&
                    b.Id != booking.Id &&
                    b.FechaInicio < booking.FechaFin &&
                    b.FechaFin > booking.FechaInicio
                );

                if (hayConflicto)
                {
                    ModelState.AddModelError("", "❌ Ya existe una reserva para esta sala en ese horario.");
                    return View(booking);
                }


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
            ViewBag.CurrentSort = sortOrder;
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
        // GET: Reservas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.Id)
                return NotFound();

            ModelState.Remove("Resource");
            ModelState.Remove("User");
            ModelState.Remove("Sala");
            ModelState.Remove("Usuario");

            if (booking.FechaFin <= booking.FechaInicio)
            {
                ModelState.AddModelError("", "❌ La fecha de fin no puede ser menor o igual a la de inicio.");
                return View(booking);
            }

            try
            {
                var reservaExistente = await _context.Bookings.FindAsync(id);
                if (reservaExistente == null)
                    return NotFound();

                reservaExistente.FechaInicio = booking.FechaInicio;
                reservaExistente.FechaFin = booking.FechaFin;
                reservaExistente.Estado = booking.Estado;

                _context.Update(reservaExistente);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "✅ Cambios guardados correctamente.";
                return RedirectToAction("MisReservas"); // 👈 ESTA ES LA REDIRECCIÓN
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Bookings.Any(e => e.Id == booking.Id))
                    return NotFound();
                else
                    throw;
            }
        }

        // GET: Reservas/Delete/5
        [HttpGet] // ← ¡IMPORTANTE! Este atributo permite acceder desde el navegador
        public async Task<IActionResult> Delete(int id)
        {
            var reserva = await _context.Bookings
                .Include(b => b.Resource)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (reserva == null)
                return NotFound();

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "🗑️ Reserva eliminada correctamente.";
            return RedirectToAction(nameof(MisReservas));
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



