using MisureRicci.Models;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public interface ICommessaService
    {
        Task<(IEnumerable<CommessaSartoriale> Items, int TotalCount)> GetCommissioniPagedAsync(int? clienteId, int? negozioId, bool isAdmin, int page, int pageSize);
        Task<CommessaKpiViewModel> GetKpiAsync(int? negozioId, bool isAdmin);
        Task<CommessaSartoriale?> GetCommessaByIdAsync(int id, int? negozioId, bool isAdmin);
        Task<CommessaDetailsViewModel?> GetCommessaDetailsAsync(int id, int? negozioId, bool isAdmin);
        Task<CommessaSartoriale> CreateCommessaAsync(CommessaCreateViewModel model, string? userId);
        Task<bool> AdvanceStatoAsync(int id, StatoCommessa nuovoStato, string? note, string? userId, int? negozioId, bool isAdmin);
        Task<bool> AddNotaAsync(int id, string nota, string? userId, int? negozioId, bool isAdmin);
        Task<bool> LinkMisuraAsync(int id, int misuraClienteId, string? userId, int? negozioId, bool isAdmin);
        Task<bool> UnlinkMisuraAsync(int id, int misuraClienteId, int? negozioId, bool isAdmin);

        /// <summary>
        /// Restituisce lo snapshot dello stato misure per la commessa indicata:
        /// se il cliente ha misure disponibili, già collegate, o non ne ha ancora nessuna.
        /// </summary>
        Task<CommessaMisuraStatus> GetStatoMisureClienteAsync(int commessaId, int? negozioId, bool isAdmin);

        /// <summary>
        /// Restituisce tutte le misure (dinamiche e legacy) disponibili per un cliente.
        /// </summary>
        Task<List<CommessaMisuraItem>> GetMisureDisponibiliPerClienteAsync(int clienteId);
    }
}
