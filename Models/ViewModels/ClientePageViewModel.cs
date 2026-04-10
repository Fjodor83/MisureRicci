namespace MisureRicci.Models.ViewModels
{
    public class ClientePageViewModel
    {
        public Cliente Cliente { get; set; } = new();
        public IEnumerable<Negozio> Negozi { get; set; } = [];
        
        public required bool IsAdmin { get; set; }
    }
}
