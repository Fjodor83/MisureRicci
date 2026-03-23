using MisureRicci.Models;

namespace MisureRicci.Models.ViewModels
{
    public class MeasurementsDashboardViewModel
    {
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public List<MeasurementType> DynamicMeasurementTypes { get; set; } = new();
    }
}
