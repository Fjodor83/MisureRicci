using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models
{
    public class MisureCliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }
        public virtual Cliente? Cliente { get; set; }

        [Required]
        [Display(Name = "Tipo di Misura")]
        public string TipoMisura { get; set; } = string.Empty; // Giacca, Pantalone, Camicia, etc.

        public DateTime DataCreazione { get; set; } = DateTime.UtcNow;

        [Display(Name = "Note")]
        public string? Note { get; set; }

        // Reference to the specific record in the category table
        public int RecordId { get; set; }
    }
}
