using MisureRicci.Models;

namespace MisureRicci.Services
{
    public interface IFabricService
    {
        Task<List<Fabric>> GetFabricsAsync(bool onlyActive = true);
        Task<Fabric?> GetFabricByIdAsync(int id);
        Task<Fabric> CreateFabricAsync(Fabric model);
        Task UpdateFabricAsync(Fabric model);
        Task DeleteFabricAsync(int id);
    }
}
