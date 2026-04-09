using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin,Manager,Sartoria,Boutique")]
    public class MeasurementsController : TenantAwareController
    {
        private const string MeasurementError = "MeasurementError";
        private readonly IMeasurementRegistryService _measurementRegistryService;
        private readonly ILegacyMeasurementService _legacyMeasurementService;
        private readonly IClienteService _clienteService;
        private readonly ICustomMeasurementService _customMeasurementService;
        private readonly ILegacyMeasurementUiService _legacyMeasurementUiService;
        private const string Dettagli = "Details";
        private const string Clienti = "Clienti";
        private const string DynamicMeasurements = "DynamicMeasurements";


        public MeasurementsController(
            IMeasurementRegistryService measurementRegistryService,
            ILegacyMeasurementService legacyMeasurementService,
            IClienteService clienteService,
            ICustomMeasurementService customMeasurementService,
            ILegacyMeasurementUiService legacyMeasurementUiService,
            ITenantService tenantService)
            : base(tenantService)
        {
            _measurementRegistryService = measurementRegistryService;
            _legacyMeasurementService = legacyMeasurementService;
            _clienteService = clienteService;
            _customMeasurementService = customMeasurementService;
            _legacyMeasurementUiService = legacyMeasurementUiService;
        }

        public async Task<IActionResult> GlobalRegistry(string? filter, int page = 1)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var tenantCheck = RequireTenant();
            if (tenantCheck != null) return tenantCheck;

            const int pageSize = 20;

            var result = await _measurementRegistryService.GetGlobalRegistryPagedAsync(filter, NegozioId, IsAdmin, page, pageSize);
            var model = new MeasurementsGlobalRegistryViewModel
            {
                Items = result.Items,
                Categoria = string.IsNullOrEmpty(filter) ? "TUTTE LE CATEGORIE" : filter.ToUpper(),
                Filter = filter,
                CurrentPage = page,
                TotalPages = (int)System.Math.Ceiling(result.TotalCount / (double)pageSize)
            };

            return View(model);
        }

        public async Task<IActionResult> Index(int? clienteId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (clienteId == null) return RedirectToAction("Index", Clienti);

            var tenantCheck = RequireTenant();
            if (tenantCheck != null) return tenantCheck;

            var cliente = await _clienteService.GetClienteScopedAsync(clienteId.Value, NegozioId, IsAdmin);
            if (cliente == null) return NotFound();

            var model = new MeasurementsDashboardViewModel
            {
                ClienteId = clienteId.Value,
                ClienteNome = $"{cliente.Nome} {cliente.Cognome}",
                DynamicMeasurementTypes = await _customMeasurementService.GetMeasurementTypesAsync(onlyActive: true)
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int? id, string? tipoMisura, int? registryId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();

            var resolved = await ResolveMeasurementDisplayAsync(id.Value, tipoMisura, registryId);
            if (resolved.DynamicRecordId.HasValue)
            {
                return RedirectToAction(Dettagli, DynamicMeasurements, new { id = resolved.DynamicRecordId.Value });
            }

            if (resolved.Model == null)
            {
                return NotFound();
            }

            return View(BuildDetailsViewModel(resolved.Model, resolved.TipoMisura));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? tipoMisura)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();

            var model = await _legacyMeasurementService.GetMeasurementScopedAsync(id.Value, tipoMisura, NegozioId, IsAdmin);
            if (model == null) return NotFound();

            return View(BuildEditViewModel(model, tipoMisura));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LegacyMeasurementEditViewModel input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrEmpty(input.TipoMisura)) return NotFound();

            var model = await _legacyMeasurementService.GetMeasurementScopedAsync(input.Id, input.TipoMisura, NegozioId, IsAdmin);
            if (model == null) return NotFound();

            if (TryApplyEditableMeasurementFields(model, input.Fields) && TryValidateModel(model))
            {
                if (await _legacyMeasurementService.UpdateMeasurementAsync(model, input.TipoMisura))
                {
                    int clienteId = _legacyMeasurementUiService.GetClienteId(model);
                    return RedirectToAction(Dettagli, Clienti, new { id = clienteId });
                }

                return NotFound();
            }

            return View(BuildEditViewModel(model, input.TipoMisura, input.Fields));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id, string? tipoMisura, int? registryId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();

            var resolved = await ResolveMeasurementDisplayAsync(id.Value, tipoMisura, registryId);
            if (resolved.DynamicRecordId.HasValue)
            {
                return RedirectToAction(Dettagli, DynamicMeasurements, new { id = resolved.DynamicRecordId.Value });
            }

            if (resolved.Model == null)
            {
                return NotFound();
            }

            return View(BuildDeleteViewModel(resolved.Model, resolved.TipoMisura, resolved.RegistryId));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string? tipoMisura, int? registryId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrEmpty(tipoMisura)) return NotFound();

            if (registryId.HasValue)
            {
                var registryEntry = await _measurementRegistryService.GetRegistryEntryAsync(registryId.Value, NegozioId, IsAdmin);
                if (registryEntry == null)
                {
                    return RedirectToAction(nameof(Index), Clienti);
                }

                try
                {
                    var clienteIdFromRegistry = await _measurementRegistryService.DeleteByRegistryEntryAsync(registryId.Value, NegozioId, IsAdmin);
                    if (clienteIdFromRegistry.HasValue)
                    {
                        return RedirectToAction(Dettagli, Clienti, new { id = clienteIdFromRegistry.Value });
                    }
                }
                catch (InvalidOperationException ex)
                {
                    TempData[MeasurementError] = ex.Message;
                    return RedirectToAction(Dettagli, Clienti, new { id = registryEntry.ClienteId });
                }

                return RedirectToAction(nameof(Index), Clienti);
            }

            var model = await _legacyMeasurementService.GetMeasurementScopedAsync(id, tipoMisura, NegozioId, IsAdmin);
            if (model != null)
            {
                int clienteId = _legacyMeasurementUiService.GetClienteId(model);
                try
                {
                    await _legacyMeasurementService.DeleteMeasurementAsync(id, tipoMisura, NegozioId, IsAdmin);
                }
                catch (InvalidOperationException ex)
                {
                    TempData[MeasurementError] = ex.Message;
                }

                return RedirectToAction(Dettagli, Clienti, new { id = clienteId });
            }
            return RedirectToAction(nameof(Index), Clienti);
        }

        private bool TryApplyEditableMeasurementFields(object model, IEnumerable<LegacyMeasurementFieldViewModel> fields)
        {
            return _legacyMeasurementUiService.TryApplyEditableMeasurementFields(
                model,
                fields,
                (field, message) => ModelState.AddModelError(field, message));
        }

        private LegacyMeasurementEditViewModel BuildEditViewModel(object model, string tipoMisura, IEnumerable<LegacyMeasurementFieldViewModel>? postedFields = null)
        {
            return _legacyMeasurementUiService.BuildEditViewModel(model, tipoMisura, postedFields);
        }

        private LegacyMeasurementDetailsViewModel BuildDetailsViewModel(object model, string tipoMisura)
        {
            return _legacyMeasurementUiService.BuildDetailsViewModel(model, tipoMisura);
        }

        private LegacyMeasurementDeleteViewModel BuildDeleteViewModel(object model, string tipoMisura, int? registryId)
        {
            return _legacyMeasurementUiService.BuildDeleteViewModel(model, tipoMisura, registryId);
        }

        private async Task<(object? Model, string TipoMisura, int? RegistryId, int? DynamicRecordId)> ResolveMeasurementDisplayAsync(int id, string tipoMisura, int? registryId)
        {
            if (registryId.HasValue)
            {
                var registryEntry = await _measurementRegistryService.GetRegistryEntryAsync(registryId.Value, NegozioId, IsAdmin);
                if (registryEntry == null)
                {
                    return (null, tipoMisura, null, null);
                }

                if (registryEntry.IsDynamic)
                {
                    return (null, registryEntry.TipoMisura, registryEntry.Id, registryEntry.RecordId);
                }

                var legacyModel = await _legacyMeasurementService.GetMeasurementByRegistryEntryAsync(registryId.Value, NegozioId, IsAdmin);
                return (legacyModel, registryEntry.TipoMisura, registryEntry.Id, null);
            }

            var model = await _legacyMeasurementService.GetMeasurementScopedAsync(id, tipoMisura, NegozioId, IsAdmin);
            if (model != null)
            {
                return (model, tipoMisura, registryId, null);
            }

            var dynamicType = (await _customMeasurementService.GetMeasurementTypesAsync(onlyActive: false))
                .FirstOrDefault(x => string.Equals(x.Nome, tipoMisura, StringComparison.OrdinalIgnoreCase));

            if (dynamicType != null)
            {
                return (null, tipoMisura, registryId, id);
            }

            return (null, tipoMisura, registryId, null);
        }
    }
}
