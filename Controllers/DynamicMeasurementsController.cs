using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public DynamicMeasurementsController(
            ICustomMeasurementService customMeasurementService,
            IClienteService clienteService,
            ICommessaService commessaService,
            UserManager<ApplicationUser> userManager)
        {
            _customMeasurementService = customMeasurementService;
            _clienteService = clienteService;
            _commessaService = commessaService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int clienteId, int typeId, int? returnToCommessaId)
        {
            var cliente = await _clienteService.GetClienteByIdAsync(clienteId);
            if (cliente == null)
            {
                return NotFound();
            }

            if (!await CanAccessClienteAsync(cliente.Id, cliente.NegozioId))
            {
                return Forbid();
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
            var cliente = await _clienteService.GetClienteByIdAsync(model.ClienteId);
            var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);

            if (cliente == null || type == null)
            {
                return NotFound();
            }

            if (!await CanAccessClienteAsync(cliente.Id, cliente.NegozioId))
            {
                return Forbid();
            }

            model.ClienteNome = $"{cliente.Nome} {cliente.Cognome}";
            model.TipoNome = type.Nome;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var record = await _customMeasurementService.CreateDynamicMeasurementAsync(model, currentUser?.Id);

                // Return-to-commessa flow: auto-link the newly created measure and redirect back.
                if (model.ReturnToCommessaId.HasValue)
                {
                    var isAdmin = User.IsInRole("Admin");
                    // Find the RegistroMisure entry created for this dynamic record.
                    var misuraClienteId = await _customMeasurementService.GetRegistroMisuraIdByDynamicRecordAsync(record.Id);
                    if (misuraClienteId.HasValue)
                    {
                        await _commessaService.LinkMisuraAsync(
                            model.ReturnToCommessaId.Value,
                            misuraClienteId.Value,
                            currentUser?.Id,
                            currentUser?.NegozioId,
                            isAdmin);
                    }

                    return RedirectToAction("Details", "Commissioni", new { id = model.ReturnToCommessaId.Value });
                }

                return RedirectToAction("Details", "Clienti", new { id = model.ClienteId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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

            if (!await CanAccessClienteAsync(record.ClienteId, record.Cliente?.NegozioId))
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

            if (!await CanAccessClienteAsync(record.ClienteId, record.Cliente?.NegozioId))
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
            var cliente = await _clienteService.GetClienteByIdAsync(model.ClienteId);
            var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);

            if (cliente == null || type == null)
            {
                return NotFound();
            }

            if (!await CanAccessClienteAsync(cliente.Id, cliente.NegozioId))
            {
                return Forbid();
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
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Create", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int clienteId)
        {
            var cliente = await _clienteService.GetClienteByIdAsync(clienteId);
            if (cliente == null)
            {
                return NotFound();
            }

            if (!await CanAccessClienteAsync(cliente.Id, cliente.NegozioId))
            {
                return Forbid();
            }

            await _customMeasurementService.DeleteDynamicMeasurementAsync(id);
            return RedirectToAction("Details", "Clienti", new { id = clienteId });
        }

        private async Task<bool> CanAccessClienteAsync(int clienteId, int? clienteNegozioId)
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
