using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;
using Reservas.Models;
using Reservas.Models.ViewModels;
using Reservas.Services;
using System.Security.Claims;

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
        // GET: /Reservas/Create?centroId=1&tipoId=1&recursoId=10
        [HttpGet]
        public IActionResult Create(int? centroId, int? tipoId, int? recursoId)
        {
            var vm = new ReservaViewModel
            {
                CentroId = centroId,
                TipoId = tipoId,
                RecursoId = recursoId,
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddHours(1)
            };
            return View(vm);
        }

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

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError("", "No se pudo identificar al usuario.");
                return View(vm);
            }

            var reserva = new Booking
            {
                ResourceId = vm.RecursoId.Value,
                UserId = userId,                     // UserId es int
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
        [HttpGet]
        public async Task<IActionResult> MisReservas(string? sortOrder)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            IQueryable<Booking> query = _context.Bookings
                .Include(b => b.Resource).ThenInclude(r => r.Center)
                .Include(b => b.Resource).ThenInclude(r => r.ResourceType)
                .Where(b => b.UserId == userId);

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
