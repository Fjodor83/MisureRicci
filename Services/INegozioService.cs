using MisureRicci.Models;

namespace MisureRicci.Services
{
    public interface INegozioService
    {
        Task<List<Negozio>> GetAllAsync();
        Task<Negozio?> GetByIdAsync(int id);
        Task<Negozio> CreateAsync(Negozio negozio);
        Task UpdateAsync(Negozio negozio);
        Task DeleteAsync(int id);
        bool Exists(int id);
        void InvalidateCache();
    }
}