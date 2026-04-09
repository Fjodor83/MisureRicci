using MisureRicci.Models;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public interface ICommessaQueryService
    {
        Task<PagedResult<CommessaSartoriale>> GetCommissioniPagedAsync(int? clienteId, int? negozioId, bool isAdmin, int page, int pageSize);
        Task<CommessaKpiViewModel> GetKpiAsync(int? negozioId, bool isAdmin);
        Task<CommessaSartoriale?> GetCommessaByIdAsync(int id, int? negozioId, bool isAdmin);
        Task<CommessaDetailsViewModel?> GetCommessaDetailsAsync(int id, int? negozioId, bool isAdmin);
        Task<List<CommessaMisuraItem>> GetMisureDisponibiliPerClienteAsync(int clienteId, int? negozioId, bool isAdmin);
        Task<CommessaMisuraStatus> GetStatoMisureClienteAsync(int commessaId, int? negozioId, bool isAdmin);
    }
}