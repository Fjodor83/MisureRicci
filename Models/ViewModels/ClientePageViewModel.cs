namespace MisureRicci.Models.ViewModels
{
    public class ClientePageViewModel
    {
        public Cliente Cliente { get; set; } = new();
        public IEnumerable<Negozio> Negozi { get; set; } = Enumerable.Empty<Negozio>();
        public bool IsAdmin { get; set; }
    }
}
