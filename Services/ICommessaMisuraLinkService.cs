using MisureRicci.Models;

namespace MisureRicci.Services
{
    public interface ICommessaMisuraLinkService
    {
        Task<Result> LinkMisuraAsync(int id, int misuraClienteId, string? userId, int? negozioId, bool isAdmin);
        Task<Result> UnlinkMisuraAsync(int id, int misuraClienteId, int? negozioId, bool isAdmin);
        Task<bool> LinkDynamicMeasurementRecordAsync(int id, int dynamicRecordId, string? userId, int? negozioId, bool isAdmin);
        Task<Result> LinkDynamicMeasurementRecordInternalAsync(int id, int dynamicRecordId, string? userId, int? negozioId, bool isAdmin);
    }
}