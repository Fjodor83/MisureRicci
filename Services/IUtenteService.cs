using MisureRicci.Models;

namespace MisureRicci.Services
{
    public interface IUtenteService
    {
        Task<List<Utente>> GetAllAsync();
        Task<Utente?> GetByIdAsync(int id);
        Task<Utente> CreateAsync(Utente utente);
        Task UpdateAsync(Utente utente);
        Task DeleteAsync(int id);
        bool Exists(int id);
    }
}