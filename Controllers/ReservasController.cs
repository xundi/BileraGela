using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;
using Reservas.Models;
using Reservas.Models.ViewModels;
using Reservas.Services;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public async Task<IActionResult> Create(int? centroId, int? tipoId, int? recursoId, DateTime? inicio, DateTime? fin)
        {
            // Si no vienen de calendario, por defecto ahora+5 min
            var fechaInicio = inicio ?? DateTime.Now.AddMinutes(5);
            var fechaFin = fin ?? fechaInicio.AddHours(1);

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
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
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

            // 🚫 No permitir reservas en el pasado
            if (vm.FechaInicio < DateTime.Now)
            {
                ModelState.AddModelError(nameof(vm.FechaInicio),
                    "La fecha de inicio no puede ser anterior al momento actual.");
                return View(vm);
            }

            // ⏱️ Diferencia mínima de 30 minutos
            if (vm.FechaFin < vm.FechaInicio.AddMinutes(30))
            {
                ModelState.AddModelError(nameof(vm.FechaFin),
                    "La fecha fin debe ser al menos 30 minutos posterior a la fecha de inicio.");
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

            // → IR A LA SEGUNDA PANTALLA
            return RedirectToAction(nameof(Datos), new { id = reserva.Id });
        }

        // GET: /Reservas/Datos/5   (segunda pantalla)
        [HttpGet]
        public async Task<IActionResult> Datos(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Resource).ThenInclude(r => r.Center)
                .Include(b => b.Resource).ThenInclude(r => r.ResourceType)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            var vm = new BookingFormViewModel
            {
                BookingId = booking.Id,         // ← asegúrate de tenerlo en el VM
                ResourceId = booking.ResourceId,
                Start = booking.FechaInicio,
                End = booking.FechaFin
            };

            return View("Datos", vm);            // Views/Reservas/Datos.cshtml
        }

        // POST: /Reservas/Datos   (guardar los datos obligatorios)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Datos(BookingFormViewModel vm)
        {
            if (!ModelState.IsValid) return View("Datos", vm);

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == vm.BookingId);
            if (booking == null) return NotFound();

            // mapear del VM al modelo
            booking.DNI = vm.DNI;
            booking.NombreApellidos = vm.NombreApellidos;
            booking.Telefono = vm.Telefono;
            booking.Email = vm.Email;
            booking.NumeroAsistentes = vm.NumeroAsistentes;
            booking.NombreServicio = vm.NombreServicio;
            booking.DescripcionActividad = vm.DescripcionActividad;
            booking.Ambito = vm.Ambito;
            booking.Observaciones = vm.Observaciones;
            booking.EquiposUtilizar = vm.EquiposUtilizar != null ? string.Join(",", vm.EquiposUtilizar) : null;

            // ahora sí queda como "Pendiente" para que lo vean los validadores
            booking.Estado = "Pendiente";

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Datos añadidos. La reserva queda pendiente de validación.";
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

            ViewBag.CentroSortParm = sortOrder == "centro" ? "centro_desc" : "centro";
            ViewBag.RecursoSortParm = sortOrder == "recurso" ? "recurso_desc" : "recurso";
            ViewBag.TipoSortParm = sortOrder == "tipo" ? "tipo_desc" : "tipo";
            ViewBag.FechaInicioSortParm = sortOrder == "fechainicio" ? "fechainicio_desc" : "fechainicio";
            ViewBag.FechaFinSortParm = sortOrder == "fechafin" ? "fechafin_desc" : "fechafin";
            ViewBag.EstadoSortParm = sortOrder == "estado" ? "estado_desc" : "estado";
            ViewBag.FechaCreacionSortParm = sortOrder == "fechacreacion" ? "fechacreacion_desc" : "fechacreacion"; // 🔵 NUEVO



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
                default: query = query.OrderByDescending(b => b.FechaCreacion); break;
            }

            var lista = await query.ToListAsync();
            return View(lista);

        }



        // ===================== EDIT =====================

        // GET: /Reservas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dni = User.Identity?.Name;
            var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Dni == dni);
            if (usuario == null) return Unauthorized();

            var reserva = await _context.Bookings
                .Include(b => b.Resource).ThenInclude(r => r.Center)
                .Include(b => b.Resource).ThenInclude(r => r.ResourceType)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == usuario.Id);

            if (reserva == null)
            {
                TempData["Mensaje"] = "No se encontró la reserva o no te pertenece.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(MisReservas));
            }

            return View(reserva);
        }

        // POST: /Reservas/Edit  (mismo esquema que Delete)
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost([Bind("Id,ResourceId,FechaInicio,FechaFin,Sala,Estado")] Booking input)
        {
            var dni = User.Identity?.Name;
            var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Dni == dni);
            if (usuario == null) return Unauthorized();

            // Limpiar campos no posteados
            ModelState.Remove(nameof(Booking.UserId));
            ModelState.Remove(nameof(Booking.User));
            ModelState.Remove(nameof(Booking.Resource));
            ModelState.Remove(nameof(Booking.FechaCreacion));

            // Derivar Sala del recurso si viene vacía
            if (string.IsNullOrWhiteSpace(input.Sala) && input.ResourceId > 0)
            {
                var sala = await _context.Resources
                    .Where(r => r.Id == input.ResourceId)
                    .Select(r => r.NameSpanish)
                    .FirstOrDefaultAsync();

                if (sala is null)
                {
                    ModelState.AddModelError(nameof(input.ResourceId), "No se encontró el recurso.");
                    return View("Edit", input);
                }
                input.Sala = sala;
            }
            ModelState.Remove(nameof(Booking.Sala));

            // Fin > Inicio
            if (input.FechaFin <= input.FechaInicio)
                ModelState.AddModelError(nameof(input.FechaFin),
                    "La fecha fin debe ser posterior a la fecha de inicio.");

            if (!ModelState.IsValid) return View("Edit", input);

            var reserva = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == input.Id && b.UserId == usuario.Id);
            if (reserva == null)
            {
                TempData["Mensaje"] = "No se encontró la reserva o no te pertenece.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(MisReservas));
            }

            reserva.FechaInicio = input.FechaInicio;
            reserva.FechaFin = input.FechaFin;
            reserva.ResourceId = input.ResourceId;
            reserva.Sala = input.Sala;
            reserva.Estado = input.Estado;

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "✅ Reserva actualizada correctamente.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(MisReservas));
        }





        // ===================== DELETE (desde tu modal) =====================

        // GET /Reservas/Delete/64  (si usas la vista de confirmación Delete.cshtml)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var reserva = await _context.Bookings
                .Include(b => b.Resource)
                .FirstOrDefaultAsync(b => b.Id == id);   // sin restricciones por estado
            if (reserva == null) return NotFound();
            return View(reserva);
        }

        // POST /Reservas/Delete  (desde tu modal o la vista)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            if (reserva == null)
            {
                TempData["Mensaje"] = "No se encontró la reserva.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(MisReservas));
            }

            _context.Bookings.Remove(reserva);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                $"Reserva de {(reserva.Resource?.NameSpanish ?? reserva.Sala)} " +
                $"({reserva.FechaInicio:g}–{reserva.FechaFin:g}) eliminada correctamente.";
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
                    // naranja si está pendiente
                    backgroundColor = b.Estado == "Confirmada" ? "#23a559"
                                    : b.Estado == "Rechazada" ? "#d9534f"
                                    : "#ff7518",

                    borderColor = b.Estado == "Confirmada" ? "#1c7c43"
                                 : b.Estado == "Rechazada" ? "#a94442"
                                 : "#cc5500"


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
