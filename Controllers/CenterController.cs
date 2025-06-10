using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;
using Reservas.Models;

namespace Reservas.Controllers
{
    public class CenterController : Controller
    {
        private readonly BDContext _context;

        public CenterController(BDContext context)
        {
            _context = context;
        }

        // GET: Center
        public async Task<IActionResult> Index()
        {
            var centers = await _context.Centers.ToListAsync();
            return View(centers);
        }

        // GET: Center/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var center = await _context.Centers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (center == null) return NotFound();

            return View(center);
        }

        // GET: Center/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // GET: Center/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NameEuskera,NameSpanish")] Center center)
        {
            if (!ModelState.IsValid)
            {
                return View(center);
            }

            _context.Add(center);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Centro creado correctamente.";
            return RedirectToAction(nameof(Index));
        }




        // GET: Center/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var center = await _context.Centers.FindAsync(id);
            if (center == null) return NotFound();

            return View(center);
        }

        // POST: Center/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Center center)
        {
            if (id != center.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(center);

            try
            {
                _context.Update(center);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Centro actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CenterExists(center.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Center/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var center = await _context.Centers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (center == null) return NotFound();

            return View(center);
        }

        // POST: Center/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var center = await _context.Centers.FindAsync(id);
            if (center != null)
            {
                _context.Centers.Remove(center);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Centro eliminado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CenterExists(int id)
        {
            return _context.Centers.Any(e => e.Id == id);
        }
    }
}
