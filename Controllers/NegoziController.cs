using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NegoziController : Controller
    {
        private readonly INegozioService _negozioService;

        public NegoziController(INegozioService negozioService)
        {
            _negozioService = negozioService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _negozioService.GetAllAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,Citta,Indirizzo,CodiceNegozio,Paese,Attivo")] Negozio negozio)
        {
            if (ModelState.IsValid)
            {
                await _negozioService.CreateAsync(negozio);
                return RedirectToAction(nameof(Index));
            }
            return View(negozio);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id == null) return NotFound();
            var negozio = await _negozioService.GetByIdAsync(id.Value);
            if (negozio == null) return NotFound();
            return View(negozio);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id == null) return NotFound();
            var negozio = await _negozioService.GetByIdAsync(id.Value);
            if (negozio == null) return NotFound();
            return View(negozio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Citta,Indirizzo,CodiceNegozio,Paese,Attivo")] Negozio negozio)
        {
            if (!ModelState.IsValid)
            {
                return View(negozio);
            }

            if (id != negozio.Id) return NotFound();

            try
            {
                await _negozioService.UpdateAsync(negozio);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NegozioExists(negozio.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        private bool NegozioExists(int id)
        {
            return _negozioService.Exists(id);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id == null) return NotFound();
            var negozio = await _negozioService.GetByIdAsync(id.Value);
            if (negozio == null) return NotFound();
            return View(negozio);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _negozioService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
