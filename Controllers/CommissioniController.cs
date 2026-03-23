using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize]
    public class CommissioniController : Controller
    {
        private readonly ICommessaService _commessaService;
        private readonly IClienteService _clienteService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommissioniController(ICommessaService commessaService, IClienteService clienteService, UserManager<ApplicationUser> userManager)
        {
            _commessaService = commessaService;
            _clienteService = clienteService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? clienteId, int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            const int pageSize = 20;
            var result = await _commessaService.GetCommissioniPagedAsync(clienteId, currentUser?.NegozioId, isAdmin, page, pageSize);
            var kpi = await _commessaService.GetKpiAsync(currentUser?.NegozioId, isAdmin);

            var model = new CommessaIndexViewModel
            {
                Items = result.Items,
                ClienteId = clienteId,
                CurrentPage = result.Page,
                TotalPages = result.TotalPages,
                Kpi = kpi
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int clienteId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            var cliente = await _clienteService.GetClienteScopedAsync(clienteId, currentUser?.NegozioId, isAdmin);
            if (cliente == null)
            {
                return NotFound();
            }

            var vm = new CommessaCreateViewModel
            {
                ClienteId = cliente.Id,
                ClienteNome = $"{cliente.Nome} {cliente.Cognome}".Trim(),
                MisureDisponibili = await _commessaService.GetMisureDisponibiliPerClienteAsync(clienteId, currentUser?.NegozioId, isAdmin)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommessaCreateViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            var cliente = await _clienteService.GetClienteScopedAsync(model.ClienteId, currentUser?.NegozioId, isAdmin);
            if (cliente == null)
            {
                return NotFound();
            }

            model.ClienteNome = $"{cliente.Nome} {cliente.Cognome}".Trim();
            if (!ModelState.IsValid)
            {
                model.MisureDisponibili = await _commessaService.GetMisureDisponibiliPerClienteAsync(model.ClienteId, currentUser?.NegozioId, isAdmin);
                return View(model);
            }

            var result = await _commessaService.CreateCommessaAsync(model, currentUser?.Id, currentUser?.NegozioId, isAdmin);
            if (!result.IsSuccess || result.Value == null)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Impossibile creare la commessa.");
                model.MisureDisponibili = await _commessaService.GetMisureDisponibiliPerClienteAsync(model.ClienteId, currentUser?.NegozioId, isAdmin);
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var vm = await _commessaService.GetCommessaDetailsAsync(id, currentUser?.NegozioId, isAdmin);
            if (vm == null)
            {
                return NotFound();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvanceStato(int id, StatoCommessa nuovoStato, string? note)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var result = await _commessaService.AdvanceStatoAsync(id, nuovoStato, note, currentUser?.Id, currentUser?.NegozioId, isAdmin);
            if (!result.IsSuccess)
            {
                TempData["CommessaError"] = result.Error ?? "Operazione non consentita.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNota(int id, string nota)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var result = await _commessaService.AddNotaAsync(id, nota, currentUser?.Id, currentUser?.NegozioId, isAdmin);
            if (!result.IsSuccess)
            {
                TempData["CommessaError"] = result.Error ?? "Impossibile aggiungere la nota.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkMisura(int id, int misuraClienteId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var result = await _commessaService.LinkMisuraAsync(id, misuraClienteId, currentUser?.Id, currentUser?.NegozioId, isAdmin);
            if (!result.IsSuccess)
            {
                TempData["CommessaError"] = result.Error ?? "Impossibile collegare la misura alla commessa.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlinkMisura(int id, int misuraClienteId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var result = await _commessaService.UnlinkMisuraAsync(id, misuraClienteId, currentUser?.NegozioId, isAdmin);
            if (!result.IsSuccess)
            {
                TempData["CommessaError"] = result.Error ?? "Impossibile scollegare la misura dalla commessa.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
