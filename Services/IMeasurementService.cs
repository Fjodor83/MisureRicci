using MisureRicci.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MisureRicci.Services
{
    public interface IMeasurementService
    {
        Task<IEnumerable<MisureCliente>> GetGlobalRegistryAsync(string filter, int? negozioId, bool isAdmin);
        Task<(IEnumerable<MisureCliente> Items, int TotalCount)> GetGlobalRegistryPagedAsync(string filter, int? negozioId, bool isAdmin, int page, int pageSize);
        Task<MisureCliente?> GetRegistryEntryAsync(int registryId, int? negozioId, bool isAdmin);
        Task<object?> GetMeasurementByRegistryEntryAsync(int registryId, int? negozioId, bool isAdmin);
        Task<int?> DeleteByRegistryEntryAsync(int registryId, int? negozioId, bool isAdmin);
        Task<object?> GetMeasurementScopedAsync(int id, string tipoMisura, int? negozioId, bool isAdmin);
        Task<bool> UpdateMeasurementAsync(object model, string tipoMisura);
        
        Task<object?> GetMeasurementAsync(int id, string tipoMisura);
        Task DeleteMeasurementAsync(int id, string tipoMisura);

        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task CreateGiaccaAsync(GiaccaMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task CreatePantaloneAsync(PantaloneMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task CreateGiletAsync(GiletMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task CreateMaglieAsync(MaglieMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task CreateOutdoorAsync(OutdoorMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task CreateAbitoAsync(AbitoCompletoMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task CreateCamiciaAsync(CamiciaMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task CreateScarpeAsync(ScarpeMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task CreateCravattaAsync(CravattaMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task CreateCinturaAsync(CinturaMeasurement model);

        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task UpdateGiaccaAsync(GiaccaMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task UpdatePantaloneAsync(PantaloneMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task UpdateGiletAsync(GiletMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task UpdateMaglieAsync(MaglieMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task UpdateOutdoorAsync(OutdoorMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task UpdateAbitoAsync(AbitoCompletoMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task UpdateCamiciaAsync(CamiciaMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task UpdateScarpeAsync(ScarpeMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task UpdateCravattaAsync(CravattaMeasurement model);
        [Obsolete("Legacy static measurement API. Prefer dynamic measurement workflow.")]
        Task UpdateCinturaAsync(CinturaMeasurement model);
    }
}
