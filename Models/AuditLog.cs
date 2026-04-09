using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string EntityName { get; set; } = string.Empty;

        [MaxLength(128)]
        public string? EntityId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? UserId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }
    }
}
