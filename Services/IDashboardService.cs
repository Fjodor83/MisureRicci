using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public interface IDashboardService
    {
        Task<DashboardKpiViewModel> GetKpiAsync(int? negozioId, bool isAdmin, CancellationToken ct = default);
    }
}
