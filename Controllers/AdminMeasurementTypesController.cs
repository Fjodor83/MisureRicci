using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;
using Npgsql;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminMeasurementTypesController : Controller
    {
        private const long MaxImageUploadRequestSize = 6 * 1024 * 1024;
        private const string EsisteGiaUnaTipologiaConQuestoNome = "Esiste gia una tipologia con questo nome.";
        private const string SiEVerificatoUnErroreInternoRiprovare = "Si e verificato un errore interno. Riprovare.";
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
            if (!ModelState.IsValid) return View(model);

            string? storedImageUrl = null;

            try
            {
                if (model.ImageUpload != null && model.ImageUpload.Length > 0)
                {
                    storedImageUrl = await _measurementTypeImageStorageService
                        .SaveImageAsync(model.ImageUpload, HttpContext.RequestAborted);
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
                    await _measurementTypeImageStorageService
                        .DeleteImageAsync(storedImageUrl, HttpContext.RequestAborted);

                ModelState.AddModelError(nameof(model.Nome), EsisteGiaUnaTipologiaConQuestoNome);
                return View(model);
            }
            catch (Exception ex)
            {
                if (storedImageUrl != null)
                    await _measurementTypeImageStorageService
                        .DeleteImageAsync(storedImageUrl, HttpContext.RequestAborted);

                _logger.LogError(ex, "Errore creazione tipologia misura {Nome}", model.Nome);
                ModelState.AddModelError(string.Empty, SiEVerificatoUnErroreInternoRiprovare);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditType(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var item = await _customMeasurementService.GetMeasurementTypeByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxImageUploadRequestSize)]
        public async Task<IActionResult> EditType(int id, MeasurementType model)
        {
            var existing = await _customMeasurementService.GetMeasurementTypeByIdAsync(id);
            if (existing == null || id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                RestoreMetadata(model, existing);
                return View(model);
            }

            var previousImageUrl = existing.ImageUrl;
            string? newImageUrl = null;

            try
            {
                if (model.ImageUpload != null && model.ImageUpload.Length > 0)
                    newImageUrl = await _measurementTypeImageStorageService
                        .SaveImageAsync(model.ImageUpload, HttpContext.RequestAborted);

                existing.Nome = model.Nome;
                existing.Descrizione = model.Descrizione;
                existing.IsActive = model.IsActive;
                existing.ImageUrl = newImageUrl ?? previousImageUrl;

                await _customMeasurementService.UpdateMeasurementTypeAsync(existing);

                if (newImageUrl != null && !string.IsNullOrWhiteSpace(previousImageUrl))
                    await _measurementTypeImageStorageService
                        .DeleteImageAsync(previousImageUrl, HttpContext.RequestAborted);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (newImageUrl != null)
                    await _measurementTypeImageStorageService
                        .DeleteImageAsync(newImageUrl, HttpContext.RequestAborted);

                RestoreMetadata(model, existing, previousImageUrl);
                HandleEditException(ex, model);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteType(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _customMeasurementService.GetMeasurementTypeByIdAsync(id);
            await _customMeasurementService.DeleteMeasurementTypeAsync(id);

            if (existing != null && !existing.IsSystem && !string.IsNullOrWhiteSpace(existing.ImageUrl))
                await _measurementTypeImageStorageService
                    .DeleteImageAsync(existing.ImageUrl, HttpContext.RequestAborted);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Fields(int typeId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(typeId);
            if (type == null) return NotFound();

            var fields = await _customMeasurementService.GetFieldsByTypeAsync(typeId, onlyActive: false);
            var vm = new MeasurementTypeManageViewModel { Type = type, Fields = fields };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> CreateField(int typeId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(typeId);
            if (type == null) return NotFound();

            return View(new MeasurementFieldPageViewModel
            {
                TypeName = type.Nome,
                Field = new MeasurementFieldDefinition { MeasurementTypeId = typeId, IsActive = true }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateField(MeasurementFieldPageViewModel pageModel)
        {
            if (!ModelState.IsValid)
            {
                var type = await _customMeasurementService
                    .GetMeasurementTypeByIdAsync(pageModel.Field.MeasurementTypeId);
                pageModel.TypeName = type?.Nome ?? string.Empty;
                return View(pageModel);
            }

            var model = pageModel.Field;
            try
            {
                await _customMeasurementService.CreateFieldAsync(model);
                return RedirectToAction(nameof(Fields), new { typeId = model.MeasurementTypeId });
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                ModelState.AddModelError(nameof(model.NomeCampo),
                    "Esiste gia un campo con questo nome per la tipologia selezionata.");
                var type = await _customMeasurementService
                    .GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                pageModel.TypeName = type?.Nome ?? string.Empty;
                return View(pageModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore creazione campo {NomeCampo}", model.NomeCampo);
                ModelState.AddModelError(string.Empty, SiEVerificatoUnErroreInternoRiprovare);
                var type = await _customMeasurementService
                    .GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                pageModel.TypeName = type?.Nome ?? string.Empty;
                return View(pageModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditField(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var field = await _customMeasurementService.GetFieldByIdAsync(id);
            if (field == null) return NotFound();

            var type = await _customMeasurementService
                .GetMeasurementTypeByIdAsync(field.MeasurementTypeId);
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
            if (!ModelState.IsValid)
            {
                var type = await _customMeasurementService
                    .GetMeasurementTypeByIdAsync(pageModel.Field.MeasurementTypeId);
                pageModel.TypeName = type?.Nome ?? string.Empty;
                return View(pageModel);
            }

            var model = pageModel.Field;
            if (id != model.Id) return NotFound();

            try
            {
                await _customMeasurementService.UpdateFieldAsync(model);
                return RedirectToAction(nameof(Fields), new { typeId = model.MeasurementTypeId });
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                ModelState.AddModelError(nameof(model.NomeCampo),
                    "Esiste gia un campo con questo nome per la tipologia selezionata.");
                var type = await _customMeasurementService
                    .GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                pageModel.TypeName = type?.Nome ?? string.Empty;
                return View(pageModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore modifica campo {Id}", model.Id);
                ModelState.AddModelError(string.Empty, SiEVerificatoUnErroreInternoRiprovare);
                var type = await _customMeasurementService
                    .GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                pageModel.TypeName = type?.Nome ?? string.Empty;
                return View(pageModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteField(int id, int typeId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _customMeasurementService.DeleteFieldAsync(id);
            return RedirectToAction(nameof(Fields), new { typeId });
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Rileva violazione di unicità su PostgreSQL (errore 23505)
        /// o tramite messaggio come fallback per SQLite/altri.
        /// </summary>
        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx)
            {
                // 23505 = unique_violation in PostgreSQL
                return pgEx.SqlState == "23505";
            }

            var message = ex.InnerException?.Message ?? ex.Message;
            return message.Contains("unique", StringComparison.OrdinalIgnoreCase)
                || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
        }

        private static void RestoreMetadata(
            MeasurementType model, MeasurementType existing, string? imageUrlOverride = null)
        {
            model.ImageUrl = imageUrlOverride ?? existing.ImageUrl;
            model.CreatedAt = existing.CreatedAt;
            model.IsSystem = existing.IsSystem;
        }

        private void HandleEditException(Exception ex, MeasurementType model)
        {
            if (ex is MeasurementTypeImageValidationException)
            {
                ModelState.AddModelError(nameof(model.ImageUpload), ex.Message);
            }
            else if (ex is DbUpdateException dbEx && IsUniqueConstraintViolation(dbEx))
            {
                ModelState.AddModelError(nameof(model.Nome), EsisteGiaUnaTipologiaConQuestoNome);
            }
            else
            {
                _logger.LogError(ex, "Errore modifica tipologia misura {Id}", model.Id);
                ModelState.AddModelError(string.Empty, SiEVerificatoUnErroreInternoRiprovare);
            }
        }
    }
}