using MisureRicci.Models;

namespace MisureRicci.Models.ViewModels
{
    public class ClientiIndexViewModel
    {
        public IEnumerable<Cliente> Clienti { get; set; } = Enumerable.Empty<Cliente>();
        public string? SearchString { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
