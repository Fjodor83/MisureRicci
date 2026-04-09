using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin,Manager,Sartoria,Boutique")]
    public class CommissioniController : TenantAwareController
    {
        private readonly ICommessaService _commessaService;
        private readonly IClienteService _clienteService;
        private readonly ICustomMeasurementService _customMeasurementService;
        private readonly IFabricService _fabricService;

        private const string ImpossibileCreareLaCommessa = "Impossibile creare la commessa.";
        private const string OperazioneNonConsentita = "Operazione non consentita.";
        private const string ImpossibileAggiungereLaNota = "Impossibile aggiungere la nota.";
        private const string ImpossibileCollegareLaMisura = "Impossibile collegare la misura.";
        private const string ImpossibileScollegareLaMisura = "Impossibile scollegare la misura.";
        private const string ImpossibileEliminareLaCommessa = "Impossibile eliminare la commessa.";
        
        private const string CommissioneError = "CommissioneError";

        public CommissioniController(ICommessaService commessaService, IClienteService clienteService, ITenantService tenantService, ICustomMeasurementService customMeasurementService, IFabricService fabricService)
            : base(tenantService)
        {
            _commessaService = commessaService;
            _clienteService = clienteService;
            _customMeasurementService = customMeasurementService;
            _fabricService = fabricService;
        }

        public async Task<IActionResult> Index(int? clienteId, int page = 1)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var tenantCheck = RequireTenant();
            if (tenantCheck != null) return tenantCheck;

            const int pageSize = 20;
            var result = await _commessaService.GetCommissioniPagedAsync(clienteId, NegozioId, IsAdmin, page, pageSize);
            var kpi = await _commessaService.GetKpiAsync(NegozioId, IsAdmin);

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

            var cliente = await _clienteService.GetClienteScopedAsync(clienteId, NegozioId, IsAdmin);
            if (cliente == null)
            {
                return NotFound();
            }

            var misureDisponibili = await _commessaService.GetMisureDisponibiliPerClienteAsync(clienteId, NegozioId, IsAdmin);
            var tipiCapoDisponibili = await _customMeasurementService.GetMeasurementTypesAsync(onlyActive: true);
            var tessutiDisponibili = await _fabricService.GetFabricsAsync(onlyActive: true);

            var model = new CommessaCreateViewModel
            {
                ClienteId = clienteId,
                ClienteNome = $"{cliente.Nome} {cliente.Cognome}".Trim(),
                MisureDisponibili = misureDisponibili,
                TipoCapiDisponibili = tipiCapoDisponibili,
                TessutiDisponibili = tessutiDisponibili
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommessaCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cliente0 = await _clienteService.GetClienteScopedAsync(model.ClienteId, NegozioId, IsAdmin);
                if (cliente0 != null) model.ClienteNome = $"{cliente0.Nome} {cliente0.Cognome}".Trim();
                model.MisureDisponibili = await _commessaService.GetMisureDisponibiliPerClienteAsync(model.ClienteId, NegozioId, IsAdmin);
                model.TipoCapiDisponibili = await _customMeasurementService.GetMeasurementTypesAsync(onlyActive: true);
                model.TessutiDisponibili = await _fabricService.GetFabricsAsync(onlyActive: true);
                return View(model);
            }

            var cliente = await _clienteService.GetClienteScopedAsync(model.ClienteId, NegozioId, IsAdmin);
            if (cliente == null)
            {
                return NotFound();
            }

            model.ClienteNome = $"{cliente.Nome} {cliente.Cognome}".Trim();

            var result = await _commessaService.CreateCommessaAsync(model, UserId, NegozioId, IsAdmin);
            if (!result.IsSuccess || result.Value == null)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? ImpossibileCreareLaCommessa);
                model.MisureDisponibili = await _commessaService.GetMisureDisponibiliPerClienteAsync(model.ClienteId, NegozioId, IsAdmin);
                model.TipoCapiDisponibili = await _customMeasurementService.GetMeasurementTypesAsync(onlyActive: true);
                model.TessutiDisponibili = await _fabricService.GetFabricsAsync(onlyActive: true);
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var vm = await _commessaService.GetCommessaDetailsAsync(id, NegozioId, IsAdmin);
            if (vm == null)
            {
                return NotFound();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _commessaService.GetCommessaByIdAsync(id, NegozioId, IsAdmin);
            if (existing == null)
            {
                return NotFound();
            }

            var result = await _commessaService.DeleteCommessaAsync(id, NegozioId, IsAdmin);
            if (!result.IsSuccess)
            {
                TempData[CommissioneError] = result.Error ?? ImpossibileEliminareLaCommessa;
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction("Details", "Clienti", new { id = existing.ClienteId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvanceStato(int id, StatoCommessa nuovoStato, string? note)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _commessaService.AdvanceStatoAsync(id, nuovoStato, note, UserId, NegozioId, IsAdmin);
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

            var result = await _commessaService.AddNotaAsync(id, nota, UserId, NegozioId, IsAdmin);
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

            var result = await _commessaService.LinkMisuraAsync(id, misuraClienteId, UserId, NegozioId, IsAdmin);
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

            var result = await _commessaService.UnlinkMisuraAsync(id, misuraClienteId, NegozioId, IsAdmin);
            if (!result.IsSuccess)
            {
                TempData[CommissioneError] = result.Error ?? ImpossibileScollegareLaMisura;
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
