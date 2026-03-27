using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin,Manager,Sartoria,Boutique")]
    public class CommissioniController : Controller
    {
        private readonly ICommessaService _commessaService;
        private readonly IClienteService _clienteService;
        private readonly ITenantService _tenantService;

        private const string ImpossibileCreareLaCommessa = "Impossibile creare la commessa.";
        private const string OperazioneNonConsentita = "Operazione non consentita.";
        private const string ImpossibileAggiungereLaNota = "Impossibile aggiungere la nota.";
        private const string ImpossibileCollegareLaMisura = "Impossibile collegare la misura.";
        private const string ImpossibileScollegareLaMisura = "Impossibile scollegare la misura.";
        
        private const string CommissioneError = "CommissioneError";

        public CommissioniController(ICommessaService commessaService, IClienteService clienteService, ITenantService tenantService)
        {
            _commessaService = commessaService;
            _clienteService = clienteService;
            _tenantService = tenantService;
        }

        public async Task<IActionResult> Index(int? clienteId, int page = 1)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();

            if (!isAdmin && !currentNegozioId.HasValue) return View("TenantAssignmentRequired");

            const int pageSize = 20;
            var result = await _commessaService.GetCommissioniPagedAsync(clienteId, currentNegozioId, isAdmin, page, pageSize);
            var kpi = await _commessaService.GetKpiAsync(currentNegozioId, isAdmin);

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
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();

            var cliente = await _clienteService.GetClienteScopedAsync(clienteId, currentNegozioId, isAdmin);
            if (cliente == null)
            {
                return NotFound();
            }

            var misureDisponibili = await _commessaService.GetMisureDisponibiliPerClienteAsync(clienteId, currentNegozioId, isAdmin);

            var model = new CommessaCreateViewModel
            {
                ClienteId = clienteId,
                ClienteNome = $"{cliente.Nome} {cliente.Cognome}".Trim(),
                MisureDisponibili = misureDisponibili
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommessaCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var isAdmin0 = _tenantService.IsAdmin();
                var currentNegozioId0 = _tenantService.GetCurrentNegozioId();
                var cliente0 = await _clienteService.GetClienteScopedAsync(model.ClienteId, currentNegozioId0, isAdmin0);
                if (cliente0 != null) model.ClienteNome = $"{cliente0.Nome} {cliente0.Cognome}".Trim();
                model.MisureDisponibili = await _commessaService.GetMisureDisponibiliPerClienteAsync(model.ClienteId, currentNegozioId0, isAdmin0);
                return View(model);
            }

            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();
            var userId = _tenantService.GetUserId();

            var cliente = await _clienteService.GetClienteScopedAsync(model.ClienteId, currentNegozioId, isAdmin);
            if (cliente == null)
            {
                return NotFound();
            }

            model.ClienteNome = $"{cliente.Nome} {cliente.Cognome}".Trim();

            var result = await _commessaService.CreateCommessaAsync(model, userId, currentNegozioId, isAdmin);
            if (!result.IsSuccess || result.Value == null)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? ImpossibileCreareLaCommessa);
                model.MisureDisponibili = await _commessaService.GetMisureDisponibiliPerClienteAsync(model.ClienteId, currentNegozioId, isAdmin);
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();

            var vm = await _commessaService.GetCommessaDetailsAsync(id, currentNegozioId, isAdmin);
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
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();
            var userId = _tenantService.GetUserId();

            var result = await _commessaService.AdvanceStatoAsync(id, nuovoStato, note, userId, currentNegozioId, isAdmin);
            if (!result.IsSuccess)
            {
                TempData[CommissioneError] = result.Error ?? OperazioneNonConsentita;
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNota(int id, string nota)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();
            var userId = _tenantService.GetUserId();

            var result = await _commessaService.AddNotaAsync(id, nota, userId, currentNegozioId, isAdmin);
            if (!result.IsSuccess)
            {
                TempData[CommissioneError] = result.Error ?? ImpossibileAggiungereLaNota;
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkMisura(int id, int misuraClienteId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();
            var userId = _tenantService.GetUserId();

            var result = await _commessaService.LinkMisuraAsync(id, misuraClienteId, userId, currentNegozioId, isAdmin);
            if (!result.IsSuccess)
            {
                TempData[CommissioneError] = result.Error ?? ImpossibileCollegareLaMisura;
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlinkMisura(int id, int misuraClienteId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();

            var result = await _commessaService.UnlinkMisuraAsync(id, misuraClienteId, currentNegozioId, isAdmin);
            if (!result.IsSuccess)
            {
                TempData[CommissioneError] = result.Error ?? ImpossibileScollegareLaMisura;
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
