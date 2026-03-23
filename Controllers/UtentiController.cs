using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UtentiController : Controller
    {
        private readonly IUtenteService _utenteService;
        private readonly INegozioService _negozioService;

        public UtentiController(IUtenteService utenteService, INegozioService negozioService)
        {
            _utenteService = utenteService;
            _negozioService = negozioService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _utenteService.GetAllAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Negozi = await _negozioService.GetAllAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,NomeCompleto,Email,Ruolo,NegozioId,Attivo")] Utente utente)
        {
            if (ModelState.IsValid)
            {
                await _utenteService.CreateAsync(utente);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Negozi = await _negozioService.GetAllAsync();
            return View(utente);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var utente = await _utenteService.GetByIdAsync(id.Value);
            if (utente == null) return NotFound();
            return View(utente);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var utente = await _utenteService.GetByIdAsync(id.Value);
            if (utente == null) return NotFound();
            ViewBag.Negozi = await _negozioService.GetAllAsync();
            return View(utente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,NomeCompleto,Email,Ruolo,NegozioId,Attivo")] Utente utente)
        {
            if (id != utente.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _utenteService.UpdateAsync(utente);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UtenteExists(utente.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Negozi = await _negozioService.GetAllAsync();
            return View(utente);
        }

        private bool UtenteExists(int id)
        {
            return _utenteService.Exists(id);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var utente = await _utenteService.GetByIdAsync(id.Value);
            if (utente == null) return NotFound();
            return View(utente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _utenteService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
