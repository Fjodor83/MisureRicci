using Microsoft.AspNetCore.Identity;

namespace MisureRicci.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public string Ruolo { get; set; } = "Sartoria";
        public int? NegozioId { get; set; }
        public virtual Negozio? Negozio { get; set; }
        public bool Attivo { get; set; } = true;
    }
}
