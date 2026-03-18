using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MisureRicci.Data;
using Microsoft.EntityFrameworkCore;

namespace MisureRicci.Controllers
{
    public class MeasurementsController : Controller
    {
        private readonly IMeasurementService _measurementService;
        private readonly IClienteService _clienteService;
        private readonly ICustomMeasurementService _customMeasurementService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MeasurementsController(IMeasurementService measurementService, IClienteService clienteService, ICustomMeasurementService customMeasurementService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _measurementService = measurementService;
            _clienteService = clienteService;
            _customMeasurementService = customMeasurementService;
            _userManager = userManager;
            _context = context;
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

            if (registryId.HasValue)
            {
                var registryEntry = await _context.RegistroMisure.FirstOrDefaultAsync(x => x.Id == registryId.Value);
                if (registryEntry?.IsDynamic == true)
                {
                    return RedirectToAction("Details", "DynamicMeasurements", new { id = registryEntry.RecordId });
                }
            }

            var model = await _measurementService.GetMeasurementAsync(id.Value, tipoMisura);
            if (model == null)
            {
                var dynamicType = (await _customMeasurementService.GetMeasurementTypesAsync(onlyActive: false))
                    .FirstOrDefault(x => string.Equals(x.Nome, tipoMisura, StringComparison.OrdinalIgnoreCase));

                if (dynamicType != null)
                {
                    return RedirectToAction("Details", "DynamicMeasurements", new { id = id.Value });
                }

                return NotFound();
            }
            ViewBag.TipoMisura = tipoMisura;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string tipoMisura)
        {
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();
            var model = await _measurementService.GetMeasurementAsync(id.Value, tipoMisura);
            if (model == null) return NotFound();
            ViewBag.TipoMisura = tipoMisura;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string tipoMisura, Microsoft.AspNetCore.Http.IFormCollection form)
        {
            if (string.IsNullOrEmpty(tipoMisura)) return NotFound();
            var model = await _measurementService.GetMeasurementAsync(id, tipoMisura);
            if (model == null) return NotFound();

            if (await TryUpdateModelAsync(model, model.GetType(), ""))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    int clienteId = (int)model.GetType().GetProperty("ClienteId")!.GetValue(model)!;
                    return RedirectToAction("Details", "Clienti", new { id = clienteId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }

            ViewBag.TipoMisura = tipoMisura;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id, string tipoMisura, int? registryId)
        {
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();

            if (registryId.HasValue)
            {
                var registryEntry = await _context.RegistroMisure.FirstOrDefaultAsync(x => x.Id == registryId.Value);
                if (registryEntry?.IsDynamic == true)
                {
                    return RedirectToAction("Details", "DynamicMeasurements", new { id = registryEntry.RecordId });
                }
            }

            var model = await _measurementService.GetMeasurementAsync(id.Value, tipoMisura);
            if (model == null)
            {
                var dynamicType = (await _customMeasurementService.GetMeasurementTypesAsync(onlyActive: false))
                    .FirstOrDefault(x => string.Equals(x.Nome, tipoMisura, StringComparison.OrdinalIgnoreCase));

                if (dynamicType != null)
                {
                    return RedirectToAction("Details", "DynamicMeasurements", new { id = id.Value });
                }

                return NotFound();
            }
            ViewBag.TipoMisura = tipoMisura;
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string tipoMisura)
        {
            if (string.IsNullOrEmpty(tipoMisura)) return NotFound();
            var model = await _measurementService.GetMeasurementAsync(id, tipoMisura);
            if (model != null)
            {
                int clienteId = (int)model.GetType().GetProperty("ClienteId")!.GetValue(model)!;
                await _measurementService.DeleteMeasurementAsync(id, tipoMisura);
                return RedirectToAction("Details", "Clienti", new { id = clienteId });
            }
            return RedirectToAction(nameof(Index), "Clienti");
        }

        private async Task<int> GetTypeIdByNameAsync(string typeName)
        {
            var type = (await _customMeasurementService.GetMeasurementTypesAsync(onlyActive: false))
                .First(x => string.Equals(x.Nome, typeName, StringComparison.OrdinalIgnoreCase));
            return type.Id;
        }
    }
}
