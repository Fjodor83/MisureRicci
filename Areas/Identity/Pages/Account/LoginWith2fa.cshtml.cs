using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MisureRicci.Models;

namespace MisureRicci.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWith2FaModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginWith2FaModel> _logger;

        public LoginWith2FaModel(
            SignInManager<ApplicationUser> signInManager,
            ILogger<LoginWith2FaModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Il codice di autenticazione è obbligatorio.")]
            [StringLength(7, ErrorMessage = "Il codice deve avere al massimo {1} caratteri.")]
            [Display(Name = "Codice di Autenticazione")]
            public string TwoFactorCode { get; set; } = string.Empty;

            [Display(Name = "Ricorda questo dispositivo")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl ??= Url.Content("~/");
            returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            var authenticatorCode = Input.TwoFactorCode
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
                authenticatorCode, rememberMe, Input.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("Utente {UserId} autenticato con 2FA.", user.Id);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Account utente {UserId} bloccato.", user.Id);
                return RedirectToPage("./Lockout");
            }

            _logger.LogWarning("Codice di autenticazione non valido per utente {UserId}.", user.Id);
            ModelState.AddModelError(string.Empty, "Codice di autenticazione non valido.");
            return Page();
        }
    }
}
