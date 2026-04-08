using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models
{
    public class Fabric
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descrizione { get; set; }

        [StringLength(100)]
        public string? Composizione { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
