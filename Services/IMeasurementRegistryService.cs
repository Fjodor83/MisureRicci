using MisureRicci.Models;

namespace MisureRicci.Services
{
    public interface IMeasurementRegistryService
    {
        Task<IEnumerable<MisureCliente>> GetGlobalRegistryAsync(string filter, int? negozioId, bool isAdmin);
        Task<(IEnumerable<MisureCliente> Items, int TotalCount)> GetGlobalRegistryPagedAsync(string filter, int? negozioId, bool isAdmin, int page, int pageSize);
        Task<MisureCliente?> GetRegistryEntryAsync(int registryId, int? negozioId, bool isAdmin);
        Task<int?> DeleteByRegistryEntryAsync(int registryId, int? negozioId, bool isAdmin);
    }
}
