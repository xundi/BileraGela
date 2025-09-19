using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;
using Reservas.Models;

namespace Reservas.Controllers
{
    public class UserController : Controller
    {
        private readonly BDContext _context;

        public UserController(BDContext context)
        {
            _context = context;
        }

        // GET: User
        // using Microsoft.EntityFrameworkCore;  <-- asegúrate de tenerlo arriba

        public async Task<IActionResult> Index(string? sortOrder)
        {
            // Para saber qué orden está activo y alternar
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DniSortParm = sortOrder == "dni" ? "dni_desc" : "dni";
            ViewBag.TipoSortParm = sortOrder == "tipo" ? "tipo_desc" : "tipo";

            // Trae también el tipo de usuario (Include)
            var query = _context.Users
                .Include(u => u.UserType)   // <-- IMPORTANTE: sin esto se verá vacío
                .AsQueryable();

            // Orden según cabecera clicada
            switch (sortOrder)
            {
                case "dni": query = query.OrderBy(u => u.Dni); break;
                case "dni_desc": query = query.OrderByDescending(u => u.Dni); break;
                case "tipo": query = query.OrderBy(u => u.UserType.Name); break;
                case "tipo_desc": query = query.OrderByDescending(u => u.UserType.Name); break;
                default: query = query.OrderBy(u => u.Dni); break; // por defecto
            }

            var lista = await query.ToListAsync();
            return View(lista);
        }





        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _context.Users.Include(u => u.UserType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            ViewData["UserTypeId"] = new SelectList(_context.UserTypes, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Dni,UserTypeId, Email")] User user)
        {
            // 1) Normalizar DNI *antes* de cualquier validación
            user.Dni = (user.Dni ?? string.Empty)
                .Replace("-", "")
                .Replace(" ", "")
                .ToUpperInvariant();

            // 2) Validación de formato (si ya tienes [RegularExpression] en el modelo,
            //    este bloque es opcional; lo dejo para que veas el error SI falla)
            var dniRegex = new System.Text.RegularExpressions.Regex(
                @"^([XYZ]\d{7}[A-Z]|[0-9]{8}[A-Z])$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!dniRegex.IsMatch(user.Dni))
            {
                ModelState.AddModelError("Dni", "DNI/NIE no válido (8 dígitos + letra, o NIE X/Y/Z + 7 dígitos + letra).");
            }

            // 3) Duplicado (anota el error en el CAMPO Dni)
            if (await _context.Users.AnyAsync(u => u.Dni == user.Dni))
            {
                ModelState.AddModelError("Dni", "Ya existe un usuario con ese DNI.");
            }

            // 4) Asegurar que el tipo existe (y poner por defecto si viene 0)
            if (user.UserTypeId == 0)
                user.UserTypeId = 1; // <-- CAMBIA si el Id de "Usuario normal" no es 1

            var tipoExiste = await _context.UserTypes.AnyAsync(t => t.Id == user.UserTypeId);
            if (!tipoExiste)
            {
                ModelState.AddModelError("UserTypeId", "El tipo de usuario seleccionado no existe.");
            }

            // 5) Si hay cualquier error de validación, volvemos a la vista y mostramos los mensajes
            if (!ModelState.IsValid)
            {
                ViewData["UserTypeId"] = new SelectList(_context.UserTypes.OrderBy(t => t.Name), "Id", "Name", user.UserTypeId);
                return View(user);
            }

            // 6) Guardar con captura de excepción de BD para mostrar el motivo si falla
            try
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Mostramos el mensaje de la BD para saber EXACTAMENTE qué está fallando (índice único, FK, etc.)
                ModelState.AddModelError(string.Empty, $"No se pudo guardar en la BD: {ex.GetBaseException().Message}");
                ViewData["UserTypeId"] = new SelectList(_context.UserTypes.OrderBy(t => t.Name), "Id", "Name", user.UserTypeId);
                return View(user);
            }
        }




        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            ViewData["UserTypeId"] = new SelectList(_context.UserTypes, "Id", "Name", user.UserTypeId);
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Dni,UserTypeId")] User user)
        {
            if (id != user.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Usuario actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["UserTypeId"] = new SelectList(_context.UserTypes, "Id", "Name", user.UserTypeId);
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _context.Users.Include(u => u.UserType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Usuario eliminado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
