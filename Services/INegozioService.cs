using MisureRicci.Models;

namespace MisureRicci.Services
{
    public interface INegozioService
    {
        Task<List<Negozio>> GetAllAsync();
        Task<Negozio?> GetByIdAsync(int id);
        Task<Result<Negozio>> CreateAsync(Negozio negozio);
        Task<Result> UpdateAsync(Negozio negozio);
        Task<Result> DeleteAsync(int id);
        bool Exists(int id);
        void InvalidateCache();
    }
}