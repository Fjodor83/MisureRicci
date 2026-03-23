using MisureRicci.Models;

namespace MisureRicci.Services
{
    /// <summary>
    /// Obsolete — backed by the legacy Utenti table. Use UserManager&lt;ApplicationUser&gt; directly.
    /// </summary>
    [Obsolete("Use UserManager<ApplicationUser> instead. IUtenteService will be removed with the Utenti table.")]
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