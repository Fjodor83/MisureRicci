using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using System.Threading.Tasks;

namespace MisureRicci.Controllers
{
    public class ClientiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clienti
        public async Task<IActionResult> Index(string searchString)
        {
            var clienti = from c in _context.Clienti
                         select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                clienti = clienti.Where(s => s.Nome.Contains(searchString) || s.Cognome.Contains(searchString) || s.ClientCode.Contains(searchString));
            }

            return View(await clienti.OrderByDescending(c => c.DataRegistrazione).ToListAsync());
        }

        // GET: Clienti/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clienti
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (cliente == null) return NotFound();

            // Fetch measurement history
            var history = await _context.RegistroMisure
                .Where(m => m.ClienteId == id)
                .OrderByDescending(m => m.DataCreazione)
                .ToListAsync();

            ViewBag.History = history;
            ViewBag.ClienteId = cliente.Id;
            ViewBag.ClienteNome = $"{cliente.Nome} {cliente.Cognome}";

            return View(cliente);
        }

        // GET: Clienti/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clienti/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Cognome,Email,Telefono,Indirizzo,Citta,StatoProvincia,CodicePostale,Paese,Note")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();

                // Generate automatic code: SR-Year-ID (padded)
                cliente.ClientCode = $"SR-{DateTime.Now.Year}-{cliente.Id:D5}";
                _context.Update(cliente);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clienti/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clienti.FindAsync(id);
            if (cliente == null) return NotFound();
            
            return View(cliente);
        }

        // POST: Clienti/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientCode,Nome,Cognome,Email,Telefono,Indirizzo,Citta,StatoProvincia,CodicePostale,Paese,Note,DataRegistrazione")] Cliente cliente)
        {
            if (id != cliente.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        private bool ClienteExists(int id)
        {
            return _context.Clienti.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clienti.FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clienti.FindAsync(id);
            if (cliente != null)
            {
                _context.Clienti.Remove(cliente);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
