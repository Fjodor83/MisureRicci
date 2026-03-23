namespace MisureRicci.Services
{
    public interface ILegacyMeasurementService
    {
        Task<object?> GetMeasurementByRegistryEntryAsync(int registryId, int? negozioId, bool isAdmin);
        Task<object?> GetMeasurementScopedAsync(int id, string tipoMisura, int? negozioId, bool isAdmin);
        Task<bool> UpdateMeasurementAsync(object model, string tipoMisura);
        Task<object?> GetMeasurementAsync(int id, string tipoMisura);
        Task<bool> DeleteMeasurementAsync(int id, string tipoMisura, int? negozioId, bool isAdmin);
    }
}
