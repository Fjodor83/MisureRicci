using MisureRicci.Models;

namespace MisureRicci.Models.ViewModels
{
    public class ClienteDetailsViewModel
    {
        public Cliente Cliente { get; set; } = null!;
        public List<MisureCliente> History { get; set; } = new();
    }
}
