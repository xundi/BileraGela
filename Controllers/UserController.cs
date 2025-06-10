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
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Users.Include(u => u.UserType).ToListAsync();
            return View(usuarios);
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
        public async Task<IActionResult> Create([Bind("Id,Dni,UserTypeId")] User user)
        {
            if (!ModelState.IsValid)
            {
                ViewData["UserTypeId"] = new SelectList(_context.UserTypes, "Id", "Name", user.UserTypeId);
                return View(user);
            }

            // ⚠️ Validar que no se repita el DNI
            if (await _context.Users.AnyAsync(u => u.Dni == user.Dni))
            {
                ModelState.AddModelError("", "Ya existe un usuario con ese DNI.");
                ViewData["UserTypeId"] = new SelectList(_context.UserTypes, "Id", "Name", user.UserTypeId);
                return View(user);
            }

            // ✅ Asignar rol por defecto si no se elige ninguno
            if (user.UserTypeId == 0)
            {
                user.UserTypeId = 1; // Id del rol "Usuario normal"
            }

            _context.Add(user);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
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
