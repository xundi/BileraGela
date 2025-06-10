using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;
using Reservas.Models;

namespace Reservas.Controllers
{
    public class UserTypeController : Controller
    {
        private readonly BDContext _context;

        public UserTypeController(BDContext context)
        {
            _context = context;
        }

        // GET: UserType
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserTypes.ToListAsync());
        }



        // GET: UserType/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userType = await _context.UserTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (userType == null)
                return NotFound();

            return View(userType);
        }

        // GET: UserType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] UserType userType)
        {
            if (await _context.UserTypes.AnyAsync(u => u.Name == userType.Name))
            {
                ModelState.AddModelError("", "Ya existe un tipo de usuario con ese nombre.");
                return View(userType);
            }

            if (!ModelState.IsValid)
                return View(userType);

            _context.Add(userType);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Tipo de usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: UserType/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userType = await _context.UserTypes.FindAsync(id);
            if (userType == null)
                return NotFound();

            return View(userType);
        }

        // POST: UserType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] UserType userType)
        {
            if (id != userType.Id)
                return NotFound();

            if (await _context.UserTypes.AnyAsync(u => u.Name == userType.Name && u.Id != id))
            {
                ModelState.AddModelError("", "Ya existe otro tipo de usuario con ese nombre.");
                return View(userType);
            }

            if (!ModelState.IsValid)
                return View(userType);

            try
            {
                _context.Update(userType);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Tipo de usuario actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserTypeExists(userType.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: UserType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userType = await _context.UserTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (userType == null)
                return NotFound();

            return View(userType);
        }

        // POST: UserType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userType = await _context.UserTypes.FindAsync(id);
            if (userType != null)
            {
                _context.UserTypes.Remove(userType);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Tipo de usuario eliminado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserTypeExists(int id)
        {
            return _context.UserTypes.Any(e => e.Id == id);
        }
    }
}
