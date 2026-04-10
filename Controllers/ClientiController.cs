using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;


namespace MisureRicci.Controllers
{
    [Authorize]
    public class ClientiController(
        IClienteService clienteService,
        INegozioService negozioService,
        ITenantService tenantService) : TenantAwareController(tenantService)
    {
        // GET: Clienti
        public async Task<IActionResult> Index(string? searchString, int page = 1)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var tenantCheck = RequireTenant();
            if (tenantCheck != null) return tenantCheck;

            const int pageSize = 20;
            
            var result = await clienteService.GetClientiPagedAsync(searchString, NegozioId, IsAdmin, page, pageSize);
            var model = new ClientiIndexViewModel
            {
                Clienti = result.Items,
                SearchString = searchString,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize)
            };

            return View(model);
        }

        // GET: Clienti/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id == null) return NotFound();

            var cliente = await clienteService.GetClienteScopedAsync(id.Value, NegozioId, IsAdmin);
            
            if (cliente == null) return NotFound();

            var history = await clienteService.GetStoricoMisureScopedAsync(id.Value, NegozioId, IsAdmin);

            var vm = new ClienteDetailsViewModel
            {
                Cliente = cliente,
                History = history
            };

            return View(vm);
        }

        // GET: Clienti/Create
        public async Task<IActionResult> Create()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return View(await BuildPageViewModelAsync(new Cliente()));
        }

        // POST: Clienti/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientePageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(await BuildPageViewModelAsync(model.Cliente, IsAdmin));
            }

            ValidateClienteTenantAssignment(model.Cliente, IsAdmin, NegozioId);

            if (ModelState.IsValid)
            {
                var result = await clienteService.CreateClienteScopedAsync(model.Cliente, NegozioId, IsAdmin);
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError(string.Empty, result.Error ?? "Operazione non riuscita.");
                    return View(await BuildPageViewModelAsync(model.Cliente, IsAdmin));
                }

                return RedirectToAction(nameof(Details), new { id = result.Value!.Id });
            }

            return View(await BuildPageViewModelAsync(model.Cliente, IsAdmin));
        }

        // GET: Clienti/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id == null) return NotFound();

            var cliente = await clienteService.GetClienteScopedAsync(id.Value, NegozioId, IsAdmin);
            if (cliente == null) return NotFound();
            
            return View(await BuildPageViewModelAsync(cliente, IsAdmin));
        }

        // POST: Clienti/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientePageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(await BuildPageViewModelAsync(model.Cliente, IsAdmin));
            }

            if (id != model.Cliente.Id) return NotFound();

            ValidateClienteTenantAssignment(model.Cliente, IsAdmin, NegozioId);

            if (ModelState.IsValid)
            {
                var result = await clienteService.UpdateClienteScopedAsync(model.Cliente, NegozioId, IsAdmin);
                if (result.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, result.Error ?? "Operazione non riuscita.");
            }

            return View(await BuildPageViewModelAsync(model.Cliente, IsAdmin));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id == null) return NotFound();

            var cliente = await clienteService.GetClienteScopedAsync(id.Value, NegozioId, IsAdmin);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await clienteService.DeleteClienteScopedAsync(id, NegozioId, IsAdmin);
            if (!result.IsSuccess)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<ClientePageViewModel> BuildPageViewModelAsync(Cliente cliente, bool? isAdminOverride = null)
        {
            var isAdmin = isAdminOverride ?? IsAdmin;

            return new ClientePageViewModel
            {
                Cliente = cliente,
                IsAdmin = isAdmin,
                Negozi = isAdmin ? await negozioService.GetAllAsync() : Enumerable.Empty<Negozio>()
            };
        }

        private void ValidateClienteTenantAssignment(Cliente cliente, bool isAdmin, int? currentNegozioId)
        {
            if (isAdmin)
            {
                if (!cliente.NegozioId.HasValue)
                {
                    ModelState.AddModelError("Cliente.NegozioId", "Il negozio è obbligatorio per rendere il cliente visibile alla boutique corretta.");
                }
            }
            else
            {
                // Safety: force the ID of the current tenant to prevent spoofing
                cliente.NegozioId = currentNegozioId;
            }
        }
    }
}
