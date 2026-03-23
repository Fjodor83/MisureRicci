using MisureRicci.Models;

namespace MisureRicci.Models.ViewModels
{
    public class MeasurementFieldPageViewModel
    {
        public MeasurementFieldDefinition Field { get; set; } = new();
        public string TypeName { get; set; } = string.Empty;
    }
}
