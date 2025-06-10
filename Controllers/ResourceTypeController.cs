using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;
using Reservas.Models;

namespace Reservas.Controllers
{
    public class ResourceTypeController : Controller
    {
        private readonly BDContext _context;

        public ResourceTypeController(BDContext context)
        {
            _context = context;
        }

        // GET: ResourceType
        public async Task<IActionResult> Index()
        {
            var tipos = await _context.ResourceTypes.ToListAsync();
            return View(tipos);
        }

        // GET: ResourceType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ResourceType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NameEuskera,NameSpanish")] ResourceType resourceType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(resourceType);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Tipo de recurso creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(resourceType);
        }

        // GET: ResourceType/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var resourceType = await _context.ResourceTypes.FindAsync(id);
            if (resourceType == null) return NotFound();

            return View(resourceType);
        }

        // POST: ResourceType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NameEuskera,NameSpanish")] ResourceType resourceType)
        {
            if (id != resourceType.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(resourceType);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Tipo de recurso actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResourceTypeExists(resourceType.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(resourceType);
        }

        // GET: ResourceType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var resourceType = await _context.ResourceTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (resourceType == null) return NotFound();

            return View(resourceType);
        }

        // POST: ResourceType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resourceType = await _context.ResourceTypes.FindAsync(id);
            if (resourceType != null)
            {
                _context.ResourceTypes.Remove(resourceType);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Tipo de recurso eliminado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ResourceTypeExists(int id)
        {
            return _context.ResourceTypes.Any(e => e.Id == id);
        }
    }
}
