using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public interface ILegacyMeasurementUiService
    {
        int GetClienteId(object model);
        bool TryApplyEditableMeasurementFields(object model, IEnumerable<LegacyMeasurementFieldViewModel> fields, Action<string, string> addError);
        LegacyMeasurementEditViewModel BuildEditViewModel(object model, string tipoMisura, IEnumerable<LegacyMeasurementFieldViewModel>? postedFields = null);
        LegacyMeasurementDetailsViewModel BuildDetailsViewModel(object model, string tipoMisura);
        LegacyMeasurementDeleteViewModel BuildDeleteViewModel(object model, string tipoMisura, int? registryId);
    }
}
