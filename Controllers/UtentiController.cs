using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UtentiController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INegozioService _negozioService;

        public UtentiController(UserManager<ApplicationUser> userManager, INegozioService negozioService)
        {
            _userManager = userManager;
            _negozioService = negozioService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .AsNoTracking()
                .Include(u => u.Negozio)
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Negozi = await _negozioService.GetAllAsync();
            return View(new UtenteAdminViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UtenteAdminViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError(nameof(vm.Password), "La password è obbligatoria per la creazione.");

            if (!ModelState.IsValid)
            {
                ViewBag.Negozi = await _negozioService.GetAllAsync();
                return View(vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.UserName,
                NormalizedUserName = vm.UserName.ToUpperInvariant(),
                Email = vm.Email,
                NormalizedEmail = vm.Email.ToUpperInvariant(),
                NomeCompleto = vm.NomeCompleto,
                Ruolo = vm.Ruolo,
                NegozioId = vm.NegozioId,
                Attivo = vm.Attivo,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password!);
            if (result.Succeeded)
                return RedirectToAction(nameof(Index));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewBag.Negozi = await _negozioService.GetAllAsync();
            return View(vm);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.Users
                .AsNoTracking()
                .Include(u => u.Negozio)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Negozi = await _negozioService.GetAllAsync();
            return View(new UtenteAdminViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                NomeCompleto = user.NomeCompleto,
                Ruolo = user.Ruolo,
                NegozioId = user.NegozioId,
                Attivo = user.Attivo
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UtenteAdminViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            ModelState.Remove(nameof(vm.Password));
            ModelState.Remove(nameof(vm.ConfirmPassword));

            if (!ModelState.IsValid)
            {
                ViewBag.Negozi = await _negozioService.GetAllAsync();
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.UserName = vm.UserName;
            user.NormalizedUserName = vm.UserName.ToUpperInvariant();
            user.Email = vm.Email;
            user.NormalizedEmail = vm.Email.ToUpperInvariant();
            user.NomeCompleto = vm.NomeCompleto;
            user.Ruolo = vm.Ruolo;
            user.NegozioId = vm.NegozioId;
            user.Attivo = vm.Attivo;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return RedirectToAction(nameof(Index));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewBag.Negozi = await _negozioService.GetAllAsync();
            return View(vm);
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.Users
                .AsNoTracking()
                .Include(u => u.Negozio)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
                await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}
