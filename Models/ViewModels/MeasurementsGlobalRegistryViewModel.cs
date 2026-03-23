using MisureRicci.Models;

namespace MisureRicci.Models.ViewModels
{
    public class MeasurementsGlobalRegistryViewModel
    {
        public IEnumerable<MisureCliente> Items { get; set; } = Enumerable.Empty<MisureCliente>();
        public string Categoria { get; set; } = "TUTTE LE CATEGORIE";
        public string? Filter { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
