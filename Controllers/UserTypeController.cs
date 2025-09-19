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
            var lista = await _context.UserTypes
                .OrderBy(t => t.Name)
                .ToListAsync();
            return View(lista);

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
            userType.Name = (userType.Name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(userType.Name))
                ModelState.AddModelError(nameof(UserType.Name), "El nombre es obligatorio.");

            // Duplicados case/space-insensitive
            if (await _context.UserTypes
                .AnyAsync(u => u.Name.ToLower() == userType.Name.ToLower()))
            {
                ModelState.AddModelError(nameof(UserType.Name), "Ya existe un tipo de usuario con ese nombre.");
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

            userType.Name = (userType.Name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(userType.Name))
                ModelState.AddModelError(nameof(UserType.Name), "El nombre es obligatorio.");

            // Duplicado en otro registro
            if (await _context.UserTypes
                .AnyAsync(u => u.Id != id && u.Name.ToLower() == userType.Name.ToLower()))
            {
                ModelState.AddModelError(nameof(UserType.Name), "Ya existe otro tipo de usuario con ese nombre.");
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
                var exists = await _context.UserTypes.AnyAsync(e => e.Id == userType.Id);
                if (!exists) return NotFound();
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
            if (userType == null)
                return RedirectToAction(nameof(Index));

            try
            {
                _context.UserTypes.Remove(userType);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Tipo de usuario eliminado correctamente.";
            }
            catch (DbUpdateException)
            {
                // Suele pasar por FK: hay usuarios con este UserTypeId
                TempData["Mensaje"] = "No se puede eliminar: hay usuarios asociados a este tipo.";
            }

            return RedirectToAction(nameof(Index));
        }


    }
   

    }
