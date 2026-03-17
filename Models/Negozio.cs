using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models
{
    public class Negozio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome Negozio")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Città")]
        public string Citta { get; set; } = string.Empty;

        [Display(Name = "Indirizzo")]
        public string? Indirizzo { get; set; }

        [Display(Name = "Codice Negozio")]
        public string? CodiceNegozio { get; set; }

        [Required]
        [Display(Name = "Paese")]
        public string Paese { get; set; } = "Italy";

        public bool Attivo { get; set; } = true;
    }
}
