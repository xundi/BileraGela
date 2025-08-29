using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;
using Reservas.Models;
using Reservas.Models.ViewModels;
using Reservas.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Reservas.Controllers
{
    public class ReservasController : Controller
    {
        private readonly BDContext _context;
        private readonly IEmailSender _email;

        public ReservasController(BDContext context, IEmailSender email)
        {
            _context = context;
            _email = email;
        }

        // ===================== CREATE =====================
        [HttpGet]
        public async Task<IActionResult> Create(int? centroId, int? tipoId, int? recursoId)
        {
            var inicio = DateTime.Now.AddMinutes(5);

            string? centroNombre = null, tipoNombre = null, recursoNombre = null;

            if (recursoId.HasValue)
            {
                var recurso = await _context.Resources
                    .Include(r => r.Center)
                    .Include(r => r.ResourceType)
                    .FirstOrDefaultAsync(r => r.Id == recursoId.Value);

                if (recurso != null)
                {
                    recursoNombre = recurso.NameSpanish;
                    centroNombre = recurso.Center?.NameSpanish;
                    tipoNombre = recurso.ResourceType?.NameSpanish;

                    centroId ??= recurso.CenterId;
                    tipoId ??= recurso.ResourceTypeId;
                }
            }

            var vm = new ReservaViewModel
            {
                CentroId = centroId,
                TipoId = tipoId,
                RecursoId = recursoId,
                CentroNombre = centroNombre,
                TipoNombre = tipoNombre,
                RecursoNombre = recursoNombre,
                FechaInicio = inicio,
                FechaFin = inicio.AddHours(1)
            };

            return View(vm);
        }



        // POST: /Reservas/Create
        // POST: /Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservaViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            if (vm.RecursoId is null)
            {
                ModelState.AddModelError(nameof(vm.RecursoId), "Falta seleccionar el recurso.");
                return View(vm);
            }

            // ← Obtén el usuario por DNI (Name)
            var dni = User.Identity?.Name;
            var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Dni == dni);
            if (usuario == null) return Unauthorized();

            var reserva = new Booking
            {
                ResourceId = vm.RecursoId.Value,
                UserId = usuario.Id,              // ← aquí va el int real
                FechaInicio = vm.FechaInicio,
                FechaFin = vm.FechaFin,
                Estado = "Pendiente",
                FechaCreacion = DateTime.Now
            };

            _context.Bookings.Add(reserva);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "✅ Reserva creada correctamente.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(MisReservas));
        }


        // ===================== MIS RESERVAS (con orden) =====================
        // GET: /Reservas/MisReservas
        [HttpGet]
        public async Task<IActionResult> MisReservas(string? sortOrder)
        {
            // ← Igual que en Validar()
            var dni = User.Identity?.Name;
            var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Dni == dni);
            if (usuario == null) return Unauthorized();

            IQueryable<Booking> query = _context.Bookings
                .Include(b => b.Resource).ThenInclude(r => r.Center)
                .Include(b => b.Resource).ThenInclude(r => r.ResourceType)
                .Where(b => b.UserId == usuario.Id);

            ViewBag.CurrentSort = string.IsNullOrEmpty(sortOrder) ? "fechainicio_desc" : sortOrder;
            switch (sortOrder)
            {
                case "centro": query = query.OrderBy(b => b.Resource.Center.NameSpanish); break;
                case "centro_desc": query = query.OrderByDescending(b => b.Resource.Center.NameSpanish); break;
                case "recurso": query = query.OrderBy(b => b.Resource.NameSpanish); break;
                case "recurso_desc": query = query.OrderByDescending(b => b.Resource.NameSpanish); break;
                case "tipo": query = query.OrderBy(b => b.Resource.ResourceType.NameSpanish); break;
                case "tipo_desc": query = query.OrderByDescending(b => b.Resource.ResourceType.NameSpanish); break;
                case "fechainicio": query = query.OrderBy(b => b.FechaInicio); break;
                case "fechainicio_desc": query = query.OrderByDescending(b => b.FechaInicio); break;
                case "fechafin": query = query.OrderBy(b => b.FechaFin); break;
                case "fechafin_desc": query = query.OrderByDescending(b => b.FechaFin); break;
                case "estado": query = query.OrderBy(b => b.Estado); break;
                case "estado_desc": query = query.OrderByDescending(b => b.Estado); break;
                case "fechacreacion": query = query.OrderBy(b => b.FechaCreacion); break;
                case "fechacreacion_desc": query = query.OrderByDescending(b => b.FechaCreacion); break;
                default: query = query.OrderByDescending(b => b.FechaInicio); break;
            }

            var lista = await query.ToListAsync();
            return View(lista);
        }

        // ===================== DELETE (desde tu modal) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var reserva = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (reserva == null)
            {
                TempData["Mensaje"] = "No se encontró la reserva o no te pertenece.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(MisReservas));
            }

            _context.Bookings.Remove(reserva);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Reserva eliminada correctamente.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(MisReservas));
        }
        // ===================== CALENDARIO =====================
        // GET: /Reservas/Calendario
        [HttpGet]
        public async Task<IActionResult> Calendario(int? centroId, int? tipoId, int? recursoId)
        {
            // Centros
            ViewBag.Centros = await _context.Centers
                .OrderBy(c => c.NameSpanish)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.NameSpanish })
                .ToListAsync();

            // Tipos de recurso
            ViewBag.Tipos = await _context.ResourceTypes
                .OrderBy(t => t.NameSpanish)
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.NameSpanish })
                .ToListAsync();

            // Recursos -> tu RecursoOptionVM
            ViewBag.Recursos = await _context.Resources
                .Include(r => r.Center)
                .Include(r => r.ResourceType)
                .OrderBy(r => r.NameSpanish)
                .Select(r => new RecursoOptionVM
                {
                    Value = r.Id.ToString(),     // ok como string para el <option value="">
                    Text = r.NameSpanish,
                    CenterId = r.CenterId,       // <-- int (sin ToString)
                    ResourceTypeId = r.ResourceTypeId // <-- int (sin ToString)
                })
                .ToListAsync();

            // Preselección que usa la vista (data-selected)
            ViewBag.CentroId = centroId;
            ViewBag.TipoId = tipoId;
            ViewBag.RecursoId = recursoId;

            // Si tienes CalendarioViewModel, pásalo; si no, devuelve View() a secas.
            var vm = new CalendarioViewModel { CentroId = centroId, TipoId = tipoId, RecursoId = recursoId };
            return View(vm);
        }

        // GET: /Reservas/CalendarioEventos?recursoId=10&start=...&end=...
        [HttpGet]
        public async Task<IActionResult> CalendarioEventos(int recursoId, DateTimeOffset start, DateTimeOffset end)
        {
            // Ojo con zonas horarias: FullCalendar envía UTC (toISOString).
            var desde = start.LocalDateTime;
            var hasta = end.LocalDateTime;

            var eventos = await _context.Bookings
                .Include(b => b.Resource)
                .Where(b => b.ResourceId == recursoId && b.FechaInicio < hasta && b.FechaFin > desde)
                .Select(b => new
                {
                    id = b.Id,
                    title = $"{b.Resource.NameSpanish} ({b.Estado})",
                    start = b.FechaInicio,
                    end = b.FechaFin,
                    color = b.Estado == "Confirmada" ? "#23a559"
                           : b.Estado == "Rechazada" ? "#d9534f"
                           : "#f0ad4e"
                })
                .ToListAsync();

            return Json(eventos);
        }


        // ===================== VALIDAR (validador) =====================
        [HttpGet]
        public async Task<IActionResult> Validar()
        {
            var dni = User.Identity?.Name;

            var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Dni == dni);
            if (usuario == null) return Unauthorized();

            var recursosAsignados = await _context.ResourceValidators
                .Where(rv => rv.UserId == usuario.Id)
                .Select(rv => rv.ResourceId)
                .ToListAsync();

            var reservas = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Resource).ThenInclude(r => r.Center)
                .Where(b => recursosAsignados.Contains(b.ResourceId))
                .Where(b => b.Estado == "Pendiente")
                .ToListAsync();

            return View(reservas);
        }

        // ===================== VALIDAR / RECHAZAR + EMAIL =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidarReserva(int id, string accion, string? motivo)
        {
            var reserva = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Resource)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (reserva == null) return NotFound();

            if (accion == "validar")
            {
                reserva.Estado = "Confirmada";
            }
            else if (accion == "rechazar")
            {
                if (string.IsNullOrWhiteSpace(motivo))
                {
                    TempData["Error"] = "Debes indicar un motivo de rechazo.";
                    return RedirectToAction(nameof(Validar));
                }
                reserva.Estado = "Rechazada";
            }
            else
            {
                TempData["Error"] = "Acción no válida.";
                return RedirectToAction(nameof(Validar));
            }

            await _context.SaveChangesAsync();

            var destinatario = reserva.User?.Email;
            if (!string.IsNullOrWhiteSpace(destinatario))
            {
                var sala = reserva.Resource?.NameSpanish ?? reserva.Sala ?? "Sala";
                var fi = reserva.FechaInicio.ToString("dd/MM/yyyy HH:mm");
                var ff = reserva.FechaFin.ToString("dd/MM/yyyy HH:mm");

                string subject, body;
                if (reserva.Estado == "Confirmada")
                {
                    subject = "✅ Reserva validada";
                    body = $@"<h2>Reserva validada</h2>
                              <p>Tu reserva ha sido <b>CONFIRMADA</b>.</p>
                              <ul>
                                <li><b>Sala:</b> {sala}</li>
                                <li><b>Desde:</b> {fi}</li>
                                <li><b>Hasta:</b> {ff}</li>
                              </ul>";
                }
                else
                {
                    subject = "❌ Reserva rechazada";
                    body = $@"<h2>Reserva rechazada</h2>
                              <p>Tu reserva ha sido <b>RECHAZADA</b>.</p>
                              <ul>
                                <li><b>Sala:</b> {sala}</li>
                                <li><b>Desde:</b> {fi}</li>
                                <li><b>Hasta:</b> {ff}</li>
                              </ul>
                              {(string.IsNullOrWhiteSpace(motivo) ? "" : $"<p><b>Motivo:</b> {System.Net.WebUtility.HtmlEncode(motivo)}</p>")}";
                }

                try { await _email.SendAsync(destinatario, subject, body); }
                catch { TempData["Error"] = "Se actualizó el estado, pero el email no pudo enviarse."; }
            }
            else
            {
                TempData["Error"] = "El usuario no tiene email configurado.";
            }

            TempData["Mensaje"] = reserva.Estado == "Confirmada"
                ? "✅ Reserva validada y notificada."
                : "❌ Reserva rechazada y notificada.";

            return RedirectToAction(nameof(Validar));
        }
    }
}
