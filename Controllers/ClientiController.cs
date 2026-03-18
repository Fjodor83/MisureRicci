using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Services;
using System.Threading.Tasks;

namespace MisureRicci.Controllers
{
    [Authorize]
    public class ClientiController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientiController(IClienteService clienteService, UserManager<ApplicationUser> userManager)
        {
            _clienteService = clienteService;
            _userManager = userManager;
        }

        // GET: Clienti
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            const int pageSize = 20;
            
            var result = await _clienteService.GetClientiPagedAsync(searchString, currentUser?.NegozioId, isAdmin, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling(result.TotalCount / (double)pageSize);
            ViewBag.SearchString = searchString;

            return View(result.Items);
        }

        // GET: Clienti/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _clienteService.GetClienteByIdAsync(id.Value);
            
            if (cliente == null) return NotFound();

            var history = await _clienteService.GetStoricoMisureAsync(id.Value);

            var vm = new MisureRicci.Models.ViewModels.ClienteDetailsViewModel
            {
                Cliente = cliente,
                History = history
            };

            return View(vm);
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
                await _clienteService.CreateClienteAsync(cliente);
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clienti/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _clienteService.GetClienteByIdAsync(id.Value);
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
                    await _clienteService.UpdateClienteAsync(cliente);
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
                {
                    if (!_clienteService.ClienteExists(cliente.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _clienteService.GetClienteByIdAsync(id.Value);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _clienteService.DeleteClienteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
