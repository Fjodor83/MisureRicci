namespace MisureRicci.Services
{
    public class LocalFileStorageProvider : IFileStorageProvider
    {
        private readonly string _basePath;

        public LocalFileStorageProvider(string basePath)
        {
            _basePath = basePath;
            Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
        {
            var physicalPath = Path.Combine(_basePath, fileName);
            var directory = Path.GetDirectoryName(physicalPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await using var fileStream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await stream.CopyToAsync(fileStream, cancellationToken);
            return fileName;
        }

        public Task<Stream?> GetAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var physicalPath = Path.Combine(_basePath, fileName);
            if (!File.Exists(physicalPath))
                return Task.FromResult<Stream?>(null);

            Stream stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult<Stream?>(stream);
        }

        public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var physicalPath = Path.Combine(_basePath, fileName);
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);

            return Task.CompletedTask;
        }
    }
}
