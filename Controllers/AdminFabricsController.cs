using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminFabricsController : Controller
    {
        private const string EsisteGiaUnTessutoConQuestoNome = "Esiste gia un tessuto con questo nome.";
        private const string SiEVerificatoUnErroreInternoRiprovare = "Si e verificato un errore interno. Riprovare.";

        private readonly IFabricService _fabricService;

        public AdminFabricsController(IFabricService fabricService)
        {
            _fabricService = fabricService;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _fabricService.GetFabricsAsync(onlyActive: false);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Fabric { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Fabric model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _fabricService.CreateFabricAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                ModelState.AddModelError(nameof(model.Nome), EsisteGiaUnTessutoConQuestoNome);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, SiEVerificatoUnErroreInternoRiprovare);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var item = await _fabricService.GetFabricByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Fabric model)
        {
            var existing = await _fabricService.GetFabricByIdAsync(id);
            if (existing == null || id != model.Id) return NotFound();

            if (!ModelState.IsValid) return View(model);

            try
            {
                await _fabricService.UpdateFabricAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                ModelState.AddModelError(nameof(model.Nome), EsisteGiaUnTessutoConQuestoNome);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, SiEVerificatoUnErroreInternoRiprovare);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _fabricService.GetFabricByIdAsync(id);
            if (existing == null) return NotFound();

            try
            {
                await _fabricService.DeleteFabricAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, SiEVerificatoUnErroreInternoRiprovare);
                return View("Index", await _fabricService.GetFabricsAsync(onlyActive: false));
            }
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("UniqueIndex") ?? false;
        }
    }
}
