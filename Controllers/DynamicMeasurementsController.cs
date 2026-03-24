using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize]
    public class DynamicMeasurementsController : Controller
    {
        private readonly ICustomMeasurementService _customMeasurementService;
        private readonly IClienteService _clienteService;
        private readonly ICommessaService _commessaService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DynamicMeasurementsController> _logger;

        public DynamicMeasurementsController(
            ICustomMeasurementService customMeasurementService,
            IClienteService clienteService,
            ICommessaService commessaService,
            UserManager<ApplicationUser> userManager,
            ILogger<DynamicMeasurementsController> logger)
        {
            _customMeasurementService = customMeasurementService;
            _clienteService = clienteService;
            _commessaService = commessaService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int clienteId, int typeId, int? returnToCommessaId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            var cliente = await _clienteService.GetClienteScopedAsync(clienteId, currentUser?.NegozioId, isAdmin);
            if (cliente == null)
            {
                return NotFound();
            }

            var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(typeId);
            if (type == null || !type.IsActive)
            {
                return NotFound();
            }

            var fields = await _customMeasurementService.GetFieldsByTypeAsync(typeId, onlyActive: true);

            var vm = new DynamicMeasurementCreateViewModel
            {
                ClienteId = clienteId,
                MeasurementTypeId = typeId,
                ClienteNome = $"{cliente.Nome} {cliente.Cognome}",
                TipoNome = type.Nome,
                ReturnToCommessaId = returnToCommessaId,
                TypeImageUrl = type.ImageUrl,
                Fields = fields.Select(f => new DynamicFieldInputViewModel
                {
                    FieldDefinitionId = f.Id,
                    NomeCampo = f.NomeCampo,
                    Etichetta = f.Etichetta,
                    Gruppo = f.Gruppo,
                    OrdineGruppo = f.OrdineGruppo,
                    TipoDato = f.TipoDato,
                    Template = f.Template,
                    UnitaMisura = f.UnitaMisura,
                    Placeholder = f.Placeholder,
                    HelpText = f.HelpText,
                    Obbligatorio = f.Obbligatorio,
                    Ordine = f.Ordine
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DynamicMeasurementCreateViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            var cliente = await _clienteService.GetClienteScopedAsync(model.ClienteId, currentUser?.NegozioId, isAdmin);
            var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);

            if (cliente == null || type == null)
            {
                return NotFound();
            }

            model.ClienteNome = $"{cliente.Nome} {cliente.Cognome}";
            model.TipoNome = type.Nome;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var record = await _customMeasurementService.CreateDynamicMeasurementAsync(model, currentUser?.Id);

                // Return-to-commessa flow: auto-link the newly created measure and redirect back.
                if (model.ReturnToCommessaId.HasValue)
                {
                    var linked = await _commessaService.LinkDynamicMeasurementRecordAsync(
                        model.ReturnToCommessaId.Value,
                        record.Id,
                        currentUser?.Id,
                        currentUser?.NegozioId,
                        isAdmin);

                    if (!linked)
                    {
                        return Forbid();
                    }

                    return RedirectToAction("Details", "Commissioni", new { id = model.ReturnToCommessaId.Value });
                }

                return RedirectToAction("Details", "Clienti", new { id = model.ClienteId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operazione non valida in creazione misura dinamica per cliente {ClienteId}, tipo {TipoId}", model.ClienteId, model.MeasurementTypeId);
                ModelState.AddModelError(string.Empty, "I dati inseriti non sono validi.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore imprevisto durante la creazione misura dinamica per cliente {ClienteId}, tipo {TipoId}", model.ClienteId, model.MeasurementTypeId);
                ModelState.AddModelError(string.Empty, "Si è verificato un errore interno. Riprovare.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var record = await _customMeasurementService.GetDynamicMeasurementRecordByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            if (!await CanAccessClienteAsync(record.Cliente?.NegozioId))
            {
                return Forbid();
            }

            return View(record);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var record = await _customMeasurementService.GetDynamicMeasurementRecordByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            if (!await CanAccessClienteAsync(record.Cliente?.NegozioId))
            {
                return Forbid();
            }

            var vm = await _customMeasurementService.BuildDynamicMeasurementEditViewModelAsync(id);
            if (vm == null)
            {
                return NotFound();
            }

            return View("Create", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DynamicMeasurementCreateViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            var cliente = await _clienteService.GetClienteScopedAsync(model.ClienteId, currentUser?.NegozioId, isAdmin);
            var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);

            if (cliente == null || type == null)
            {
                return NotFound();
            }

            model.ClienteNome = $"{cliente.Nome} {cliente.Cognome}";
            model.TipoNome = type.Nome;

            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            try
            {
                await _customMeasurementService.UpdateDynamicMeasurementAsync(model);
                return RedirectToAction(nameof(Details), new { id = model.RecordId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operazione non valida in modifica misura dinamica record {RecordId}", model.RecordId);
                ModelState.AddModelError(string.Empty, "I dati inseriti non sono validi.");
                return View("Create", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore imprevisto durante la modifica misura dinamica record {RecordId}", model.RecordId);
                ModelState.AddModelError(string.Empty, "Si è verificato un errore interno. Riprovare.");
                return View("Create", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int clienteId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            var cliente = await _clienteService.GetClienteScopedAsync(clienteId, currentUser?.NegozioId, isAdmin);
            if (cliente == null)
            {
                return NotFound();
            }

            await _customMeasurementService.DeleteDynamicMeasurementAsync(id);
            return RedirectToAction("Details", "Clienti", new { id = clienteId });
        }

        private async Task<bool> CanAccessClienteAsync(int? clienteNegozioId)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return false;
            }

            return currentUser.NegozioId.HasValue
                && clienteNegozioId.HasValue
                && currentUser.NegozioId.Value == clienteNegozioId.Value;
        }
    }
}
