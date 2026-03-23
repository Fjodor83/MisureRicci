using MisureRicci.Models;

namespace MisureRicci.Models.ViewModels
{
    public class UtenteAdminPageViewModel
    {
        public UtenteAdminViewModel Form { get; set; } = new();
        public IEnumerable<Negozio> Negozi { get; set; } = Enumerable.Empty<Negozio>();
    }
}
