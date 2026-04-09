namespace MisureRicci.Services
{
    public interface IFileStorageProvider
    {
        Task<string> SaveAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
        Task<Stream?> GetAsync(string fileName, CancellationToken cancellationToken = default);
        Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);
    }
}
