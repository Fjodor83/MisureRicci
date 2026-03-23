using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models.Options
{
    public class BootstrapAdminOptions
    {
        public const string SectionName = "BootstrapAdmin";

        public bool Enabled { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [MinLength(12)]
        public string? Password { get; set; }

        public string NomeCompleto { get; set; } = "Amministratore Sistema";
    }
}
