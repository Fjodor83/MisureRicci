using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Controllers
{
    public class UtentiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UtentiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Utenti.Include(u => u.Negozio).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Negozi = await _context.Negozi.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,NomeCompleto,Email,Ruolo,NegozioId,Attivo")] Utente utente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(utente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Negozi = await _context.Negozi.ToListAsync();
            return View(utente);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var utente = await _context.Utenti.Include(u => u.Negozio).FirstOrDefaultAsync(m => m.Id == id);
            if (utente == null) return NotFound();
            return View(utente);
        }
    }
}
