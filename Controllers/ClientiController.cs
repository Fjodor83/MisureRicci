using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;
using System.Threading.Tasks;

namespace MisureRicci.Controllers
{
    [Authorize]
    public class ClientiController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly INegozioService _negozioService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientiController(
            IClienteService clienteService,
            INegozioService negozioService,
            UserManager<ApplicationUser> userManager)
        {
            _clienteService = clienteService;
            _negozioService = negozioService;
            _userManager = userManager;
        }

        // GET: Clienti
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");
            const int pageSize = 20;
            
            var result = await _clienteService.GetClientiPagedAsync(searchString, currentUser?.NegozioId, isAdmin, page, pageSize);
            var model = new ClientiIndexViewModel
            {
                Clienti = result.Items,
                SearchString = searchString,
                CurrentPage = page,
                TotalPages = (int)System.Math.Ceiling(result.TotalCount / (double)pageSize)
            };

            return View(model);
        }

        // GET: Clienti/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");
            var cliente = await _clienteService.GetClienteScopedAsync(id.Value, currentUser?.NegozioId, isAdmin);
            
            if (cliente == null) return NotFound();

            var history = await _clienteService.GetStoricoMisureScopedAsync(id.Value, currentUser?.NegozioId, isAdmin);

            var vm = new MisureRicci.Models.ViewModels.ClienteDetailsViewModel
            {
                Cliente = cliente,
                History = history
            };

            return View(vm);
        }

        // GET: Clienti/Create
        public async Task<IActionResult> Create()
        {
            return View(await BuildPageViewModelAsync(new Cliente()));
        }

        // POST: Clienti/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientePageViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole(ApplicationRoles.Admin);
            ValidateClienteTenantAssignment(model.Cliente, isAdmin);

            if (ModelState.IsValid)
            {
                var created = await _clienteService.CreateClienteScopedAsync(model.Cliente, currentUser?.NegozioId, isAdmin);
                if (created == null)
                {
                    return Forbid();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(await BuildPageViewModelAsync(model.Cliente, isAdmin));
        }

        // GET: Clienti/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole(ApplicationRoles.Admin);
            var cliente = await _clienteService.GetClienteScopedAsync(id.Value, currentUser?.NegozioId, isAdmin);
            if (cliente == null) return NotFound();
            
            return View(await BuildPageViewModelAsync(cliente, isAdmin));
        }

        // POST: Clienti/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientePageViewModel model)
        {
            if (id != model.Cliente.Id) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole(ApplicationRoles.Admin);
            ValidateClienteTenantAssignment(model.Cliente, isAdmin);

            if (ModelState.IsValid)
            {
                if (await _clienteService.UpdateClienteScopedAsync(model.Cliente, currentUser?.NegozioId, isAdmin))
                {
                    return RedirectToAction(nameof(Index));
                }

                return NotFound();
            }

            return View(await BuildPageViewModelAsync(model.Cliente, isAdmin));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");
            var cliente = await _clienteService.GetClienteScopedAsync(id.Value, currentUser?.NegozioId, isAdmin);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            if (!await _clienteService.DeleteClienteScopedAsync(id, currentUser?.NegozioId, isAdmin))
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<ClientePageViewModel> BuildPageViewModelAsync(Cliente cliente, bool? isAdminOverride = null)
        {
            var isAdmin = isAdminOverride ?? User.IsInRole(ApplicationRoles.Admin);

            return new ClientePageViewModel
            {
                Cliente = cliente,
                IsAdmin = isAdmin,
                Negozi = isAdmin ? await _negozioService.GetAllAsync() : Enumerable.Empty<Negozio>()
            };
        }

        private void ValidateClienteTenantAssignment(Cliente cliente, bool isAdmin)
        {
            if (isAdmin && !cliente.NegozioId.HasValue)
            {
                ModelState.AddModelError("Cliente.NegozioId", "Il negozio è obbligatorio per rendere il cliente visibile alla boutique corretta.");
            }
        }
    }
}
