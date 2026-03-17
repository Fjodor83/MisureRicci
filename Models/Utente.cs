using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models
{
    public class Utente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome Utente/Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Ruolo")]
        public string Ruolo { get; set; } = "Sartoria"; // Admin, Sartoria, Boutique, Manager

        [Display(Name = "Negozio di Riferimento")]
        public int? NegozioId { get; set; }
        public virtual Negozio? Negozio { get; set; }

        public bool Attivo { get; set; } = true;

        public DateTime DataCreazione { get; set; } = DateTime.UtcNow;
    }
}
