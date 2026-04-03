using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole(ApplicationRoles.Admin);
            
            var query = _userManager.Users
                .AsNoTracking()
                .Include(u => u.Negozio)
                .AsQueryable();

            if (!isAdmin)
            {
                if (userId == null) return Forbid();
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (currentUser?.NegozioId == null) return Forbid();
                query = query.Where(u => u.NegozioId == currentUser.NegozioId);
            }

            var users = await query
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole(ApplicationRoles.Admin);
            
            var pageModel = new UtenteAdminPageViewModel
            {
                Form = new UtenteAdminViewModel { Attivo = true },
                Negozi = isAdmin ? await _negozioService.GetAllAsync() : new List<Negozio>()
            };

            if (!isAdmin && userId != null)
            {
                var currentUser = await _userManager.FindByIdAsync(userId);
                pageModel.Form.NegozioId = currentUser?.NegozioId;
            }

            return View(pageModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UtenteAdminPageViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole(ApplicationRoles.Admin);
            var vm = model.Form;

            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                ModelState.AddModelError(nameof(vm.Password), "La password è obbligatoria per la creazione.");
            }

            if (!isAdmin)
            {
                if (userId != null)
                {
                    var currentUser = await _userManager.FindByIdAsync(userId);
                    vm.NegozioId = currentUser?.NegozioId;
                }
                
                if (vm.Ruolo == ApplicationRoles.Admin)
                {
                    ModelState.AddModelError(nameof(vm.Ruolo), "Non hai i permessi per creare un amministratore.");
                }
            }

            ValidateRoleAssignment(vm);

            if (!ModelState.IsValid)
            {
                model.Negozi = isAdmin ? await _negozioService.GetAllAsync() : new List<Negozio>();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = vm.UserName,
                Email = vm.Email,
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

            model.Negozi = isAdmin ? await _negozioService.GetAllAsync() : new List<Negozio>();
            return View(model);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole(ApplicationRoles.Admin);

            var user = await _userManager.Users
                .AsNoTracking()
                .Include(u => u.Negozio)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            if (!isAdmin)
            {
                if (userId == null) return Forbid();
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (user.NegozioId != currentUser?.NegozioId)
                {
                    return Forbid();
                }
            }

            user.Ruolo = await ResolveAssignedRoleAsync(user);
            return View(user);
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole(ApplicationRoles.Admin);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!isAdmin)
            {
                if (userId == null) return Forbid();
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (user.NegozioId != currentUser?.NegozioId)
                {
                    return Forbid();
                }
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
                Negozi = isAdmin ? await _negozioService.GetAllAsync() : new List<Negozio>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UtenteAdminPageViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole(ApplicationRoles.Admin);
            var vm = model.Form;

            if (id != vm.Id) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!isAdmin)
            {
                if (userId == null) return Forbid();
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (user.NegozioId != currentUser?.NegozioId)
                    return Forbid();

                vm.NegozioId = user.NegozioId;
                if (vm.Ruolo == ApplicationRoles.Admin)
                    ModelState.AddModelError(nameof(vm.Ruolo), "Non puoi assegnare il ruolo di amministratore.");
            }

            ModelState.Remove($"{nameof(UtenteAdminPageViewModel.Form)}.{nameof(vm.Password)}");
            ModelState.Remove($"{nameof(UtenteAdminPageViewModel.Form)}.{nameof(vm.ConfirmPassword)}");
            ValidateRoleAssignment(vm);

            if (!ModelState.IsValid)
            {
                model.Negozi = isAdmin ? await _negozioService.GetAllAsync() : new List<Negozio>();
                return View(model);
            }

            user.UserName = vm.UserName;
            user.Email = vm.Email;
            user.NomeCompleto = vm.NomeCompleto;
            user.Ruolo = vm.Ruolo;
            user.NegozioId = vm.NegozioId;
            user.Attivo = vm.Attivo;

            // ✅ nested if eliminated — complexity drops by 2
            var result = await UpdateUserAndSyncRoleAsync(user, vm.Ruolo);

            if (result.Succeeded)
                return RedirectToAction(nameof(Index));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            model.Negozi = isAdmin ? await _negozioService.GetAllAsync() : new List<Negozio>();
            return View(model);
        }

        /// <summary>
        /// Persists user changes and keeps the ASP.NET Identity role table in sync.
        /// Returns the first failed <see cref="IdentityResult"/>, or a succeeded one.
        /// </summary>
        private async Task<IdentityResult> UpdateUserAndSyncRoleAsync(ApplicationUser user, string ruolo)
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return result;

            return await SynchronizeUserRoleAsync(user, ruolo);
        }
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole(ApplicationRoles.Admin);

            var user = await _userManager.Users
                .AsNoTracking()
                .Include(u => u.Negozio)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            if (!isAdmin)
            {
                if (userId == null) return Forbid();
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (user.NegozioId != currentUser?.NegozioId)
                {
                    return Forbid();
                }
            }

            user.Ruolo = await ResolveAssignedRoleAsync(user);
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole(ApplicationRoles.Admin);

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (!isAdmin)
                {
                    if (userId == null) return Forbid();
                    var currentUser = await _userManager.FindByIdAsync(userId);
                    if (user.NegozioId != currentUser?.NegozioId)
                    {
                        return Forbid();
                    }
                }

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
