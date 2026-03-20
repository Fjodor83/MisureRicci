using MisureRicci.Models;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public interface ICustomMeasurementService
    {
        Task<List<MeasurementType>> GetMeasurementTypesAsync(bool onlyActive = true);
        Task<MeasurementType?> GetMeasurementTypeByIdAsync(int id);
        Task<MeasurementType> CreateMeasurementTypeAsync(MeasurementType model);
        Task UpdateMeasurementTypeAsync(MeasurementType model);
        Task DeleteMeasurementTypeAsync(int id);

        Task<List<MeasurementFieldDefinition>> GetFieldsByTypeAsync(int measurementTypeId, bool onlyActive = true);
        Task<MeasurementFieldDefinition?> GetFieldByIdAsync(int id);
        Task<MeasurementFieldDefinition> CreateFieldAsync(MeasurementFieldDefinition model);
        Task UpdateFieldAsync(MeasurementFieldDefinition model);
        Task DeleteFieldAsync(int id);

        Task<DynamicMeasurementRecord> CreateDynamicMeasurementAsync(DynamicMeasurementCreateViewModel model, string? createdByUserId);
        Task<DynamicMeasurementRecord?> GetDynamicMeasurementRecordByIdAsync(int id);
        Task<DynamicMeasurementCreateViewModel?> BuildDynamicMeasurementEditViewModelAsync(int recordId);
        Task UpdateDynamicMeasurementAsync(DynamicMeasurementCreateViewModel model);
        Task DeleteDynamicMeasurementAsync(int recordId);

        /// <summary>
        /// Restituisce l'Id della voce RegistroMisure associata al record dinamico indicato,
        /// necessario per collegare automaticamente la misura appena creata a una commessa.
        /// </summary>
        Task<int?> GetRegistroMisuraIdByDynamicRecordAsync(int dynamicRecordId);
    }
}
