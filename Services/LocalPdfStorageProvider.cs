namespace MisureRicci.Services
{
    public class LocalPdfStorageProvider : IPdfStorageService
    {
        private readonly string _basePath;

        public LocalPdfStorageProvider(string basePath)
        {
            _basePath = basePath;
            Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveAsync(string relativePath, byte[] content, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("Path is required.", nameof(relativePath));
            }

            var sanitized = relativePath.Replace('\\', '/').TrimStart('/');
            var fullPath = Path.Combine(_basePath, sanitized.Replace('/', Path.DirectorySeparatorChar));
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(fullPath, content, ct);
            return sanitized;
        }

        public async Task<byte[]?> GetAsync(string relativePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            var sanitized = relativePath.Replace('\\', '/').TrimStart('/');
            var fullPath = Path.Combine(_basePath, sanitized.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                return null;
            }

            return await File.ReadAllBytesAsync(fullPath, ct);
        }
    }
}
