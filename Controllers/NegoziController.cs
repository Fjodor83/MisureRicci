using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Controllers
{
    public class NegoziController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NegoziController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Negozi.ToListAsync());
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
                _context.Add(negozio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(negozio);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var negozio = await _context.Negozi.FirstOrDefaultAsync(m => m.Id == id);
            if (negozio == null) return NotFound();
            return View(negozio);
        }
    }
}
