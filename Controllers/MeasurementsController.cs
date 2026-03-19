using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MisureRicci.Controllers
{
    public class MeasurementsController : Controller
    {
        private readonly IMeasurementService _measurementService;
        private readonly IClienteService _clienteService;
        private readonly ICustomMeasurementService _customMeasurementService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MeasurementsController(IMeasurementService measurementService, IClienteService clienteService, ICustomMeasurementService customMeasurementService, UserManager<ApplicationUser> userManager)
        {
            _measurementService = measurementService;
            _clienteService = clienteService;
            _customMeasurementService = customMeasurementService;
            _userManager = userManager;
        }

        public async Task<IActionResult> GlobalRegistry(string filter, int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            const int pageSize = 20;

            var result = await _measurementService.GetGlobalRegistryPagedAsync(filter, currentUser?.NegozioId, isAdmin, page, pageSize);

            ViewBag.Categoria = string.IsNullOrEmpty(filter) ? "TUTTE LE CATEGORIE" : filter.ToUpper();
            ViewBag.Filter = filter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling(result.TotalCount / (double)pageSize);
            
            return View(result.Items);
        }

        public async Task<IActionResult> Index(int? clienteId)
        {
            if (clienteId == null) return RedirectToAction("Index", "Clienti");

            var cliente = await _clienteService.GetClienteByIdAsync(clienteId.Value);
            if (cliente == null) return NotFound();

            ViewBag.ClienteId = clienteId;
            ViewBag.ClienteNome = $"{cliente.Nome} {cliente.Cognome}";
            ViewBag.DynamicMeasurementTypes = await _customMeasurementService.GetMeasurementTypesAsync(onlyActive: true);
            return View();
        }

        public async Task<IActionResult> Details(int? id, string tipoMisura, int? registryId)
        {
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();

            var resolved = await ResolveMeasurementDisplayAsync(id.Value, tipoMisura, registryId);
            if (resolved.DynamicRecordId.HasValue)
            {
                return RedirectToAction("Details", "DynamicMeasurements", new { id = resolved.DynamicRecordId.Value });
            }

            if (resolved.Model == null)
            {
                return NotFound();
            }

            ViewBag.TipoMisura = resolved.TipoMisura;
            return View(resolved.Model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string tipoMisura)
        {
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            var model = await _measurementService.GetMeasurementScopedAsync(id.Value, tipoMisura, currentUser?.NegozioId, isAdmin);
            if (model == null) return NotFound();

            ViewBag.TipoMisura = tipoMisura;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string tipoMisura, Microsoft.AspNetCore.Http.IFormCollection form)
        {
            if (string.IsNullOrEmpty(tipoMisura)) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            var model = await _measurementService.GetMeasurementScopedAsync(id, tipoMisura, currentUser?.NegozioId, isAdmin);
            if (model == null) return NotFound();

            if (await TryUpdateModelAsync(model, model.GetType(), ""))
            {
                if (await _measurementService.UpdateMeasurementAsync(model, tipoMisura))
                {
                    int clienteId = (int)model.GetType().GetProperty("ClienteId")!.GetValue(model)!;
                    return RedirectToAction("Details", "Clienti", new { id = clienteId });
                }

                return NotFound();
            }

            ViewBag.TipoMisura = tipoMisura;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id, string tipoMisura, int? registryId)
        {
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();

            var resolved = await ResolveMeasurementDisplayAsync(id.Value, tipoMisura, registryId);
            if (resolved.DynamicRecordId.HasValue)
            {
                return RedirectToAction("Details", "DynamicMeasurements", new { id = resolved.DynamicRecordId.Value });
            }

            if (resolved.Model == null)
            {
                return NotFound();
            }

            ViewBag.TipoMisura = resolved.TipoMisura;
            ViewBag.RegistryId = resolved.RegistryId;
            return View(resolved.Model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string tipoMisura, int? registryId)
        {
            if (string.IsNullOrEmpty(tipoMisura)) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            if (registryId.HasValue)
            {
                var clienteIdFromRegistry = await _measurementService.DeleteByRegistryEntryAsync(registryId.Value, currentUser?.NegozioId, isAdmin);
                if (clienteIdFromRegistry.HasValue)
                {
                    return RedirectToAction("Details", "Clienti", new { id = clienteIdFromRegistry.Value });
                }

                return RedirectToAction(nameof(Index), "Clienti");
            }

            var model = await _measurementService.GetMeasurementScopedAsync(id, tipoMisura, currentUser?.NegozioId, isAdmin);
            if (model != null)
            {
                int clienteId = (int)model.GetType().GetProperty("ClienteId")!.GetValue(model)!;
                await _measurementService.DeleteMeasurementAsync(id, tipoMisura);
                return RedirectToAction("Details", "Clienti", new { id = clienteId });
            }
            return RedirectToAction(nameof(Index), "Clienti");
        }

        private async Task<(object? Model, string TipoMisura, int? RegistryId, int? DynamicRecordId)> ResolveMeasurementDisplayAsync(int id, string tipoMisura, int? registryId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            if (registryId.HasValue)
            {
                var registryEntry = await _measurementService.GetRegistryEntryAsync(registryId.Value, currentUser?.NegozioId, isAdmin);
                if (registryEntry == null)
                {
                    return (null, tipoMisura, null, null);
                }

                if (registryEntry.IsDynamic)
                {
                    return (null, registryEntry.TipoMisura, registryEntry.Id, registryEntry.RecordId);
                }

                var legacyModel = await _measurementService.GetMeasurementByRegistryEntryAsync(registryId.Value, currentUser?.NegozioId, isAdmin);
                return (legacyModel, registryEntry.TipoMisura, registryEntry.Id, null);
            }

            var model = await _measurementService.GetMeasurementScopedAsync(id, tipoMisura, currentUser?.NegozioId, isAdmin);
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
