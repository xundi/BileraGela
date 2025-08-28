using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;
using Reservas.Services;
// using MailKit;               // opcional
using MailKit.Security;          // para SslHandshakeException

namespace Reservas.Controllers
{
    public class HomeController : Controller
    {
        private readonly BDContext _context;
        private readonly IEmailSender _email;   // servicio de correo

        public HomeController(BDContext context, IEmailSender email)
        {
            _context = context;
            _email = email;
        }

        public IActionResult Index()
        {
            int? tipoUsuario = HttpContext.Session.GetInt32("UserTypeId");

            return tipoUsuario switch
            {
                3 => RedirectToAction("PanelGestion", "Home"),
                2 => RedirectToAction("PanelValidador", "Home"),
                1 => RedirectToAction("Panel", "Home"),
                _ => View()
            };
        }

        public IActionResult PanelGestion()
        {
            int? tipoUsuario = HttpContext.Session.GetInt32("UserTypeId");
            if (tipoUsuario != 3) return Unauthorized();
            return View();
        }

        public IActionResult PanelValidador()
        {
            int? tipoUsuario = HttpContext.Session.GetInt32("UserTypeId");
            if (tipoUsuario != 2) return Unauthorized();
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

        public async Task<IActionResult> Panel()
        {
            int tipoUsuario = HttpContext.Session.GetInt32("UserTypeId") ?? 0;
            if (tipoUsuario != 1) return Unauthorized();

            var centros = await _context.Centers
                .Select(c => new { c.Id, c.NameSpanish })
                .ToListAsync();
            ViewBag.Centros = new SelectList(centros, "Id", "NameSpanish");

            return View();
        }

        // ================== TEST EMAIL ==================
        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                // ⚠️ cámbialo por tu correo real
                string destinatario = "idoia.lertxundiiribar@osakidetza.eus";

                string subject = "📧 Prueba SMTP Office365 - Sistema de Reservas";
                string body = @"
                    <h2>Prueba de correo</h2>
                    <p>Este mensaje ha sido enviado usando <b>smtp.office365.com:587</b> con STARTTLS.</p>
                    <p>Si lo recibes, la configuración SMTP funciona correctamente ✅</p>";

                await _email.SendAsync(destinatario, subject, body);

                return Content("✅ Correo de prueba enviado a " + destinatario);
            }
            catch (MailKit.ServiceNotAuthenticatedException ex)
            {
                return Content("❌ Error de autenticación SMTP: " + ex.Message);
            }
            catch (MailKit.ServiceNotConnectedException ex)
            {
                return Content("❌ Error de conexión SMTP: " + ex.Message);
            }
            catch (SslHandshakeException ex)
            {
                return Content("❌ Error de TLS/SSL: " + ex.Message);
            }
            catch (Exception ex)
            {
                return Content("❌ Error general enviando correo: " + ex.Message);
            }
        }
    }
}
