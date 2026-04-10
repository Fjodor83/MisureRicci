using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models
{
    public class Cliente
    {
        [Key]
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Codice cliente nel formato SR-YYYY-NNNNN.
        /// Generato da ClienteService.CreateClienteScopedAsync dopo il salvataggio.
        /// (In precedenza era una colonna calcolata SQL Server PERSISTED)
        /// </summary>
        [Display(Name = "Codice Cliente")]
        [MaxLength(20)]
        [Required]
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

        [Display(Name = "Negozio")]
        public int? NegozioId { get; set; }
        public virtual Negozio? Negozio { get; set; }
    }
}
