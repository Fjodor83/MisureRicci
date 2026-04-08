using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MisureRicci.Models;

namespace MisureRicci.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "La password attuale è obbligatoria.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password Attuale")]
            public string OldPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "La nuova password è obbligatoria.")]
            [StringLength(100, ErrorMessage = "La {0} deve avere almeno {2} caratteri.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Nuova Password")]
            public string NewPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "La conferma password è obbligatoria.")]
            [DataType(DataType.Password)]
            [Display(Name = "Conferma Nuova Password")]
            [Compare("NewPassword", ErrorMessage = "Le password non corrispondono.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var changeResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!changeResult.Succeeded)
            {
                foreach (var error in changeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("Utente {UserId} ha cambiato la password.", user.Id);

            TempData["SuccessMessage"] = "La password è stata aggiornata con successo.";
            return RedirectToPage();
        }
    }
}
