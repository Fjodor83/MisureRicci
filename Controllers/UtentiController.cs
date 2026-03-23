using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = ApplicationRoles.Admin)]
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

            foreach (var user in users)
            {
                user.Ruolo = await ResolveAssignedRoleAsync(user);
            }

            return View(users);
        }

        public async Task<IActionResult> Create()
        {
            var pageModel = new UtenteAdminPageViewModel
            {
                Form = new UtenteAdminViewModel(),
                Negozi = await _negozioService.GetAllAsync()
            };
            return View(pageModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UtenteAdminPageViewModel model)
        {
            var vm = model.Form;

            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                ModelState.AddModelError(nameof(vm.Password), "La password è obbligatoria per la creazione.");
            }

            ValidateRoleAssignment(vm);

            if (!ModelState.IsValid)
            {
                model.Negozi = await _negozioService.GetAllAsync();
                return View(model);
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
            {
                var roleSyncResult = await SynchronizeUserRoleAsync(user, vm.Ruolo);
                if (roleSyncResult.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                await _userManager.DeleteAsync(user);
                result = roleSyncResult;
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.Negozi = await _negozioService.GetAllAsync();
            return View(model);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .AsNoTracking()
                .Include(u => u.Negozio)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            user.Ruolo = await ResolveAssignedRoleAsync(user);
            return View(user);
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(new UtenteAdminPageViewModel
            {
                Form = new UtenteAdminViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    NomeCompleto = user.NomeCompleto,
                    Ruolo = await ResolveAssignedRoleAsync(user),
                    NegozioId = user.NegozioId,
                    Attivo = user.Attivo
                },
                Negozi = await _negozioService.GetAllAsync()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UtenteAdminPageViewModel model)
        {
            var vm = model.Form;
            if (id != vm.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(vm.Password));
            ModelState.Remove(nameof(vm.ConfirmPassword));
            ValidateRoleAssignment(vm);

            if (!ModelState.IsValid)
            {
                model.Negozi = await _negozioService.GetAllAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

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
            {
                var roleSyncResult = await SynchronizeUserRoleAsync(user, vm.Ruolo);
                if (roleSyncResult.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                result = roleSyncResult;
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.Negozi = await _negozioService.GetAllAsync();
            return View(model);
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .AsNoTracking()
                .Include(u => u.Negozio)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            user.Ruolo = await ResolveAssignedRoleAsync(user);
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }

        private void ValidateRoleAssignment(UtenteAdminViewModel vm)
        {
            if (!ApplicationRoles.IsSupported(vm.Ruolo))
            {
                ModelState.AddModelError(nameof(vm.Ruolo), "Il ruolo selezionato non è supportato.");
            }

            if (!string.Equals(vm.Ruolo, ApplicationRoles.Admin, StringComparison.Ordinal) && !vm.NegozioId.HasValue)
            {
                ModelState.AddModelError(nameof(vm.NegozioId), "Il negozio è obbligatorio per questo ruolo.");
            }
        }

        private async Task<IdentityResult> SynchronizeUserRoleAsync(ApplicationUser user, string targetRole)
        {
            if (!ApplicationRoles.IsSupported(targetRole))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Il ruolo selezionato non è supportato."
                });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles
                .Where(role => !string.Equals(role, targetRole, StringComparison.Ordinal))
                .ToArray();

            if (rolesToRemove.Length > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    return removeResult;
                }
            }

            if (!currentRoles.Contains(targetRole, StringComparer.Ordinal))
            {
                var addResult = await _userManager.AddToRoleAsync(user, targetRole);
                if (!addResult.Succeeded)
                {
                    return addResult;
                }
            }

            return IdentityResult.Success;
        }

        private async Task<string> ResolveAssignedRoleAsync(ApplicationUser user)
        {
            var actualRole = (await _userManager.GetRolesAsync(user))
                .FirstOrDefault(ApplicationRoles.IsSupported);

            return actualRole ?? user.Ruolo;
        }
    }
}
