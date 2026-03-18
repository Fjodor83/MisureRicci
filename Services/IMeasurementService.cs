using MisureRicci.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MisureRicci.Services
{
    public interface IMeasurementService
    {
        Task<IEnumerable<MisureCliente>> GetGlobalRegistryAsync(string filter, int? negozioId, bool isAdmin);
        Task<(IEnumerable<MisureCliente> Items, int TotalCount)> GetGlobalRegistryPagedAsync(string filter, int? negozioId, bool isAdmin, int page, int pageSize);
        
        Task<object?> GetMeasurementAsync(int id, string tipoMisura);
        Task DeleteMeasurementAsync(int id, string tipoMisura);

        Task CreateGiaccaAsync(GiaccaMeasurement model);
        Task CreatePantaloneAsync(PantaloneMeasurement model);
        Task CreateGiletAsync(GiletMeasurement model);
        Task CreateMaglieAsync(MaglieMeasurement model);
        Task CreateOutdoorAsync(OutdoorMeasurement model);
        Task CreateAbitoAsync(AbitoCompletoMeasurement model);
        Task CreateCamiciaAsync(CamiciaMeasurement model);
        Task CreateScarpeAsync(ScarpeMeasurement model);
        Task CreateCravattaAsync(CravattaMeasurement model);
        Task CreateCinturaAsync(CinturaMeasurement model);

        Task UpdateGiaccaAsync(GiaccaMeasurement model);
        Task UpdatePantaloneAsync(PantaloneMeasurement model);
        Task UpdateGiletAsync(GiletMeasurement model);
        Task UpdateMaglieAsync(MaglieMeasurement model);
        Task UpdateOutdoorAsync(OutdoorMeasurement model);
        Task UpdateAbitoAsync(AbitoCompletoMeasurement model);
        Task UpdateCamiciaAsync(CamiciaMeasurement model);
        Task UpdateScarpeAsync(ScarpeMeasurement model);
        Task UpdateCravattaAsync(CravattaMeasurement model);
        Task UpdateCinturaAsync(CinturaMeasurement model);
    }
}
