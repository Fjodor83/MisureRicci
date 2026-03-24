using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminMeasurementTypesController : Controller
    {
        private const long MaxImageUploadRequestSize = 6 * 1024 * 1024;

        private readonly ICustomMeasurementService _customMeasurementService;
        private readonly IMeasurementTypeImageStorageService _measurementTypeImageStorageService;
        private readonly ILogger<AdminMeasurementTypesController> _logger;

        public AdminMeasurementTypesController(
            ICustomMeasurementService customMeasurementService,
            IMeasurementTypeImageStorageService measurementTypeImageStorageService,
            ILogger<AdminMeasurementTypesController> logger)
        {
            _customMeasurementService = customMeasurementService;
            _measurementTypeImageStorageService = measurementTypeImageStorageService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _customMeasurementService.GetMeasurementTypesAsync(onlyActive: false);
            return View(items);
        }

        [HttpGet]
        public IActionResult CreateType()
        {
            return View(new MeasurementType { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxImageUploadRequestSize)]
        public async Task<IActionResult> CreateType(MeasurementType model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? storedImageUrl = null;

            try
            {
                if (model.ImageUpload != null && model.ImageUpload.Length > 0)
                {
                    storedImageUrl = await _measurementTypeImageStorageService.SaveImageAsync(model.ImageUpload, HttpContext.RequestAborted);
                    model.ImageUrl = storedImageUrl;
                }

                await _customMeasurementService.CreateMeasurementTypeAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (MeasurementTypeImageValidationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageUpload), ex.Message);
                return View(model);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                if (storedImageUrl != null)
                {
                    await _measurementTypeImageStorageService.DeleteImageAsync(storedImageUrl, HttpContext.RequestAborted);
                }

                ModelState.AddModelError(nameof(model.Nome), "Esiste gia una tipologia con questo nome.");
                return View(model);
            }
            catch (Exception ex)
            {
                if (storedImageUrl != null)
                {
                    await _measurementTypeImageStorageService.DeleteImageAsync(storedImageUrl, HttpContext.RequestAborted);
                }

                _logger.LogError(ex, "Errore durante la creazione della tipologia misura {Nome}", model.Nome);
                ModelState.AddModelError(string.Empty, "Si e verificato un errore interno. Riprovare.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditType(int id)
        {
            var item = await _customMeasurementService.GetMeasurementTypeByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxImageUploadRequestSize)]
        public async Task<IActionResult> EditType(int id, MeasurementType model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var existing = await _customMeasurementService.GetMeasurementTypeByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.ImageUrl = existing.ImageUrl;
                model.CreatedAt = existing.CreatedAt;
                model.IsSystem = existing.IsSystem;
                return View(model);
            }

            var previousImageUrl = existing.ImageUrl;
            string? newImageUrl = null;

            try
            {
                if (model.ImageUpload != null && model.ImageUpload.Length > 0)
                {
                    newImageUrl = await _measurementTypeImageStorageService.SaveImageAsync(model.ImageUpload, HttpContext.RequestAborted);
                }

                existing.Nome = model.Nome;
                existing.Descrizione = model.Descrizione;
                existing.IsActive = model.IsActive;
                existing.ImageUrl = newImageUrl ?? previousImageUrl;

                await _customMeasurementService.UpdateMeasurementTypeAsync(existing);

                if (newImageUrl != null && !string.IsNullOrWhiteSpace(previousImageUrl))
                {
                    await _measurementTypeImageStorageService.DeleteImageAsync(previousImageUrl, HttpContext.RequestAborted);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (MeasurementTypeImageValidationException ex)
            {
                model.ImageUrl = previousImageUrl;
                model.CreatedAt = existing.CreatedAt;
                model.IsSystem = existing.IsSystem;
                ModelState.AddModelError(nameof(model.ImageUpload), ex.Message);
                return View(model);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                if (newImageUrl != null)
                {
                    await _measurementTypeImageStorageService.DeleteImageAsync(newImageUrl, HttpContext.RequestAborted);
                }

                model.ImageUrl = previousImageUrl;
                model.CreatedAt = existing.CreatedAt;
                model.IsSystem = existing.IsSystem;
                ModelState.AddModelError(nameof(model.Nome), "Esiste gia una tipologia con questo nome.");
                return View(model);
            }
            catch (Exception ex)
            {
                if (newImageUrl != null)
                {
                    await _measurementTypeImageStorageService.DeleteImageAsync(newImageUrl, HttpContext.RequestAborted);
                }

                model.ImageUrl = previousImageUrl;
                model.CreatedAt = existing.CreatedAt;
                model.IsSystem = existing.IsSystem;
                _logger.LogError(ex, "Errore durante la modifica della tipologia misura {Id}", model.Id);
                ModelState.AddModelError(string.Empty, "Si e verificato un errore interno. Riprovare.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteType(int id)
        {
            var existing = await _customMeasurementService.GetMeasurementTypeByIdAsync(id);
            await _customMeasurementService.DeleteMeasurementTypeAsync(id);

            if (existing != null && !existing.IsSystem && !string.IsNullOrWhiteSpace(existing.ImageUrl))
            {
                await _measurementTypeImageStorageService.DeleteImageAsync(existing.ImageUrl, HttpContext.RequestAborted);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Fields(int typeId)
        {
            var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(typeId);
            if (type == null)
            {
                return NotFound();
            }

            var fields = await _customMeasurementService.GetFieldsByTypeAsync(typeId, onlyActive: false);
            var vm = new MeasurementTypeManageViewModel
            {
                Type = type,
                Fields = fields
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> CreateField(int typeId)
        {
            var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(typeId);
            if (type == null)
            {
                return NotFound();
            }

            return View(new MeasurementFieldPageViewModel
            {
                TypeName = type.Nome,
                Field = new MeasurementFieldDefinition
                {
                    MeasurementTypeId = typeId,
                    IsActive = true
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateField(MeasurementFieldPageViewModel pageModel)
        {
            var model = pageModel.Field;

            if (!ModelState.IsValid)
            {
                var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                pageModel.TypeName = type?.Nome ?? string.Empty;
                return View(pageModel);
            }

            try
            {
                await _customMeasurementService.CreateFieldAsync(model);
                return RedirectToAction(nameof(Fields), new { typeId = model.MeasurementTypeId });
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                ModelState.AddModelError(nameof(model.NomeCampo), "Esiste gia un campo con questo nome per la tipologia selezionata.");
                var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                pageModel.TypeName = type?.Nome ?? string.Empty;
                return View(pageModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del campo {NomeCampo} per tipo {TypeId}", model.NomeCampo, model.MeasurementTypeId);
                ModelState.AddModelError(string.Empty, "Si e verificato un errore interno. Riprovare.");
                var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                pageModel.TypeName = type?.Nome ?? string.Empty;
                return View(pageModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditField(int id)
        {
            var field = await _customMeasurementService.GetFieldByIdAsync(id);
            if (field == null)
            {
                return NotFound();
            }

            var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(field.MeasurementTypeId);
            return View(new MeasurementFieldPageViewModel
            {
                TypeName = type?.Nome ?? string.Empty,
                Field = field
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditField(int id, MeasurementFieldPageViewModel pageModel)
        {
            var model = pageModel.Field;

            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                pageModel.TypeName = type?.Nome ?? string.Empty;
                return View(pageModel);
            }

            try
            {
                await _customMeasurementService.UpdateFieldAsync(model);
                return RedirectToAction(nameof(Fields), new { typeId = model.MeasurementTypeId });
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                ModelState.AddModelError(nameof(model.NomeCampo), "Esiste gia un campo con questo nome per la tipologia selezionata.");
                var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                pageModel.TypeName = type?.Nome ?? string.Empty;
                return View(pageModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la modifica del campo {Id}", model.Id);
                ModelState.AddModelError(string.Empty, "Si e verificato un errore interno. Riprovare.");
                var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                pageModel.TypeName = type?.Nome ?? string.Empty;
                return View(pageModel);
            }
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx)
            {
                return sqlEx.Number is 2601 or 2627;
            }

            var message = ex.InnerException?.Message ?? ex.Message;
            return message.Contains("unique", StringComparison.OrdinalIgnoreCase)
                || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteField(int id, int typeId)
        {
            await _customMeasurementService.DeleteFieldAsync(id);
            return RedirectToAction(nameof(Fields), new { typeId });
        }
    }
}
