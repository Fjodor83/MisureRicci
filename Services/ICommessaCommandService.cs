using MisureRicci.Models;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public interface ICommessaCommandService
    {
        Task<Result<CommessaSartoriale>> CreateCommessaAsync(CommessaCreateViewModel model, string? userId, int? negozioId, bool isAdmin);
        Task<Result> DeleteCommessaAsync(int id, int? negozioId, bool isAdmin);
        Task<Result> AdvanceStatoAsync(int id, StatoCommessa nuovoStato, string? note, string? userId, int? negozioId, bool isAdmin);
        Task<Result> AddNotaAsync(int id, string nota, string? userId, int? negozioId, bool isAdmin);
        Task<Result<int>> CreateAndLinkDynamicMeasurementAsync(DynamicMeasurementCreateViewModel model, string? userId, int? negozioId, bool isAdmin);
    }
}