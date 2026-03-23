using System.Threading.Tasks;

namespace MisureRicci.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateDossierPdfAsync(int clienteId, int? negozioId, bool isAdmin);
    }
}
