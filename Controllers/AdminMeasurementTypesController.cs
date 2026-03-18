using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminMeasurementTypesController : Controller
    {
        private readonly ICustomMeasurementService _customMeasurementService;

        public AdminMeasurementTypesController(ICustomMeasurementService customMeasurementService)
        {
            _customMeasurementService = customMeasurementService;
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
        public async Task<IActionResult> CreateType(MeasurementType model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _customMeasurementService.CreateMeasurementTypeAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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
        public async Task<IActionResult> EditType(int id, MeasurementType model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _customMeasurementService.UpdateMeasurementTypeAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteType(int id)
        {
            await _customMeasurementService.DeleteMeasurementTypeAsync(id);
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

            ViewBag.TypeName = type.Nome;
            return View(new MeasurementFieldDefinition
            {
                MeasurementTypeId = typeId,
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateField(MeasurementFieldDefinition model)
        {
            if (!ModelState.IsValid)
            {
                var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                ViewBag.TypeName = type?.Nome;
                return View(model);
            }

            try
            {
                await _customMeasurementService.CreateFieldAsync(model);
                return RedirectToAction(nameof(Fields), new { typeId = model.MeasurementTypeId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                ViewBag.TypeName = type?.Nome;
                return View(model);
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
            ViewBag.TypeName = type?.Nome;
            return View(field);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditField(int id, MeasurementFieldDefinition model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                ViewBag.TypeName = type?.Nome;
                return View(model);
            }

            try
            {
                await _customMeasurementService.UpdateFieldAsync(model);
                return RedirectToAction(nameof(Fields), new { typeId = model.MeasurementTypeId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var type = await _customMeasurementService.GetMeasurementTypeByIdAsync(model.MeasurementTypeId);
                ViewBag.TypeName = type?.Nome;
                return View(model);
            }
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
