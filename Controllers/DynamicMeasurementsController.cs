using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin,Manager,Sartoria,Boutique")]
    public class DynamicMeasurementsController : TenantAwareController
    {
        private const string MeasurementError = "MeasurementError";
        private readonly ICustomMeasurementService _customMeasurementService;
        private readonly IClienteService _clienteService;
        private readonly ICommessaService _commessaService;
        private readonly ILogger<DynamicMeasurementsController> _logger;
        private const string CreateView = "Create";

        public DynamicMeasurementsController(
            ICustomMeasurementService customMeasurementService,
            IClienteService clienteService,
            ICommessaService commessaService,
            ITenantService tenantService,
            ILogger<DynamicMeasurementsController> logger)
            : base(tenantService)
        {
            _customMeasurementService = customMeasurementService;
            _clienteService = clienteService;
            _commessaService = commessaService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int clienteId, int typeId, int? returnToCommessaId, MeasurementUnit unit = MeasurementUnit.Centimeters)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var cliente = await _clienteService.GetClienteScopedAsync(clienteId, NegozioId, IsAdmin);
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
                SelectedUnit = unit,
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
            if (!ModelState.IsValid)
            {
                var cliente0 = await _clienteService.GetClienteScopedAsync(model.ClienteId, NegozioId, IsAdmin);
                var type0 = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                if (cliente0 != null) model.ClienteNome = $"{cliente0.Nome} {cliente0.Cognome}";
                if (type0 != null)
                {
                    model.TipoNome = type0.Nome;
                    model.TypeImageUrl = type0.ImageUrl;
                }
                return View(model);
            }

            var cliente = await _clienteService.GetClienteScopedAsync(model.ClienteId, NegozioId, IsAdmin);
            var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);

            if (cliente == null || type == null)
            {
                return NotFound();
            }

            model.ClienteNome = $"{cliente.Nome} {cliente.Cognome}";
            model.TipoNome = type.Nome;

            try
            {
                if (model.ReturnToCommessaId.HasValue)
                {
                    var result = await _commessaService.CreateAndLinkDynamicMeasurementAsync(
                        model,
                        UserId,
                        NegozioId,
                        IsAdmin);

                    if (!result.IsSuccess)
                    {
                        ModelState.AddModelError(string.Empty, result.Error ?? "Impossibile creare e collegare la misura alla commessa.");
                        return View(model);
                    }

                    return RedirectToAction("Details", "Commissioni", new { id = model.ReturnToCommessaId.Value });
                }

                await _customMeasurementService.CreateDynamicMeasurementAsync(model, UserId);
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
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var record = await _customMeasurementService.GetDynamicMeasurementRecordByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            if (!CanAccessCliente(record.Cliente?.NegozioId))
            {
                return Forbid();
            }

            return View(record);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var record = await _customMeasurementService.GetDynamicMeasurementRecordByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            if (!CanAccessCliente(record.Cliente?.NegozioId))
            {
                return Forbid();
            }

            var vm = await _customMeasurementService.BuildDynamicMeasurementEditViewModelAsync(id);
            if (vm == null)
            {
                return NotFound();
            }

            return View(CreateView, vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DynamicMeasurementCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cliente0 = await _clienteService.GetClienteScopedAsync(model.ClienteId, NegozioId, IsAdmin);
                var type0 = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                if (cliente0 != null) model.ClienteNome = $"{cliente0.Nome} {cliente0.Cognome}";
                if (type0 != null)
                {
                    model.TipoNome = type0.Nome;
                    model.TypeImageUrl = type0.ImageUrl;
                }
                return View(CreateView, model);
            }

            var existingRecord = await _customMeasurementService.GetDynamicMeasurementRecordByIdAsync(model.RecordId);
            if (existingRecord == null)
            {
                return NotFound();
            }

            if (!CanAccessCliente(existingRecord.Cliente?.NegozioId))
            {
                return Forbid();
            }

            if (existingRecord.ClienteId != model.ClienteId || existingRecord.MeasurementTypeId != model.MeasurementTypeId)
            {
                return NotFound();
            }

            var cliente = await _clienteService.GetClienteScopedAsync(model.ClienteId, NegozioId, IsAdmin);
            var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);

            if (cliente == null || type == null)
            {
                return NotFound();
            }

            model.ClienteNome = $"{cliente.Nome} {cliente.Cognome}";
            model.TipoNome = type.Nome;

            try
            {
                await _customMeasurementService.UpdateDynamicMeasurementAsync(model);
                return RedirectToAction(nameof(Details), new { id = model.RecordId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operazione non valida in modifica misura dinamica record {RecordId}", model.RecordId);
                ModelState.AddModelError(string.Empty, "I dati inseriti non sono validi.");
                return View(CreateView, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore imprevisto durante la modifica misura dinamica record {RecordId}", model.RecordId);
                ModelState.AddModelError(string.Empty, "Si è verificato un errore interno. Riprovare.");
                return View(CreateView, model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int clienteId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var record = await _customMeasurementService.GetDynamicMeasurementRecordByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            if (!CanAccessCliente(record.Cliente?.NegozioId))
            {
                return Forbid();
            }

            if (record.ClienteId != clienteId)
            {
                return NotFound();
            }

            try
            {
                await _customMeasurementService.DeleteDynamicMeasurementAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                TempData[MeasurementError] = ex.Message;
            }

            return RedirectToAction("Details", "Clienti", new { id = record.ClienteId });
        }

        private bool CanAccessCliente(int? clienteNegozioId)
        {
            if (IsAdmin) return true;

            return NegozioId.HasValue
                && clienteNegozioId.HasValue
                && NegozioId.Value == clienteNegozioId.Value;
        }
    }
}
