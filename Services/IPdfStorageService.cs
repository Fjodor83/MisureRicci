namespace MisureRicci.Services
{
    public interface IPdfStorageService
    {
        Task<string> SaveAsync(string relativePath, byte[] content, CancellationToken ct = default);
        Task<byte[]?> GetAsync(string relativePath, CancellationToken ct = default);
    }
}
