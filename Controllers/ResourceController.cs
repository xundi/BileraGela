﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;
using Reservas.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Reservas.Controllers
{
    public class ResourceController : Controller
    {
        private readonly BDContext _context;
        //Prueba GIT
        public ResourceController(BDContext context)
        {
            _context = context;
        }

        // GET: Resource
        public async Task<IActionResult> Index(string centro, string tipo)
        {
            var query = _context.Resources
                .Include(r => r.Center)
                .Include(r => r.ResourceType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(centro))
                query = query.Where(r => r.Center.NameSpanish == centro);

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(r => r.ResourceType.NameSpanish == tipo);

            var centros = await _context.Centers
                .Select(c => c.NameSpanish)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            var tipos = await _context.ResourceTypes
                .Select(t => t.NameSpanish)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            ViewBag.Centros = centros;
            ViewBag.Tipos = tipos;

            var recursos = await query
                .OrderBy(r => r.Center.NameSpanish)
                .ThenBy(r => r.ResourceType.NameSpanish)
                .ThenBy(r => r.NameEuskera)
                .ThenBy(r => r.NameSpanish)
                .ToListAsync();

            return View(recursos);
        }



        // GET: Resource/Create
        public async Task<IActionResult> Create()
        {
            await CargarCombos();
            return View();
        }

        // POST: Resource/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id, NameEuskera,NameSpanish,CenterId,ResourceTypeId")] Resource resource)
        {


            _context.Add(resource);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Recurso creado correctamente.";
            return RedirectToAction(nameof(Index));


        }

        // GET: Resource/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return NotFound();

            await CargarCombos(resource.CenterId, resource.ResourceTypeId);
            return View(resource);
        }

        // POST: Resource/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NameEuskera,NameSpanish,CenterId,ResourceTypeId")] Resource resource)
        {
            if (id != resource.Id) return NotFound();


            Center miCentro = _context.Centers.Find(resource.CenterId);
            ResourceType miResourceType = _context.ResourceTypes.Find(resource.ResourceTypeId);

            resource.Center = miCentro;
            resource.ResourceType = miResourceType;


            try
            {
                _context.Update(resource);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Recurso actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResourceExists(resource.Id))
                    return NotFound();
                else
                    throw;
            }



        }

        // GET: Resource/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var resource = await _context.Resources
                .Include(r => r.Center)
                .Include(r => r.ResourceType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (resource == null) return NotFound();

            return View(resource);
        }

        // POST: Resource/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource != null)
            {
                _context.Resources.Remove(resource);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Recurso eliminado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ResourceExists(int id)
        {
            return _context.Resources.Any(e => e.Id == id);
        }

        // Cargar combos para Center y ResourceType
        private async Task CargarCombos(int? selectedCenterId = null, int? selectedResourceTypeId = null)
        {
            var centers = await _context.Centers.ToListAsync();
            var types = await _context.ResourceTypes.ToListAsync();

            ViewBag.Centers = new SelectList(centers, "Id", "NameSpanish", selectedCenterId);
            ViewBag.ResourceTypes = new SelectList(types, "Id", "NameSpanish", selectedResourceTypeId);
        }

    }
}
