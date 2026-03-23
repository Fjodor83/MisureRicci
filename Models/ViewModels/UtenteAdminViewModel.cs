using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models.ViewModels
{
    public class UtenteAdminViewModel
    {
        public string? Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Ruolo")]
        public string Ruolo { get; set; } = "Sartoria";

        [Display(Name = "Negozio Assegnato")]
        public int? NegozioId { get; set; }

        [Display(Name = "Attivo")]
        public bool Attivo { get; set; } = true;

        /// <summary>Only required on Create; ignored on Edit.</summary>
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Conferma Password")]
        [Compare(nameof(Password), ErrorMessage = "Le password non coincidono.")]
        public string? ConfirmPassword { get; set; }
    }
}
