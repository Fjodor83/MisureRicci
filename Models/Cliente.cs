using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Codice Cliente")]
        public string? ClientCode { get; set; }

        [Required]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cognome")]
        public string Cognome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Telefono")]
        public string? Telefono { get; set; }

        [Display(Name = "Indirizzo")]
        public string? Indirizzo { get; set; }

        [Display(Name = "Città")]
        public string? Citta { get; set; }

        [Display(Name = "Stato/Provincia/Regione")]
        public string? StatoProvincia { get; set; }

        [Display(Name = "Codice Postale")]
        public string? CodicePostale { get; set; }

        [Required]
        [Display(Name = "Paese")]
        public string Paese { get; set; } = "Italy";

        [Display(Name = "Note")]
        public string? Note { get; set; }

        public DateTime DataRegistrazione { get; set; } = DateTime.UtcNow;

        [Display(Name = "Negozio (Opzionale)")]
        public int? NegozioId { get; set; }
        public virtual Negozio? Negozio { get; set; }
    }
}
