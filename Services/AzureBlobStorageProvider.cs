namespace MisureRicci.Services
{
    /// <summary>
    /// Stub per Azure Blob Storage. Implementare in produzione.
    /// Configurare la connection string in appsettings o User Secrets:
    ///   "Storage": { "Provider": "AzureBlob", "AzureBlobConnectionString": "...", "ContainerName": "uploads" }
    /// </summary>
    public class AzureBlobStorageProvider : IFileStorageProvider
    {
        // TODO: Aggiungere pacchetto NuGet Azure.Storage.Blobs
        // TODO: Iniettare BlobServiceClient tramite DI
        // TODO: Leggere configurazione da IOptions<StorageOptions>

        public Task<string> SaveAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
        {
            // TODO: Implementare upload su Azure Blob Storage
            // var blobClient = _containerClient.GetBlobClient(fileName);
            // await blobClient.UploadAsync(stream, overwrite: false, cancellationToken);
            // return blobClient.Uri.ToString();
            throw new NotImplementedException("AzureBlobStorageProvider non ancora implementato. Configurare Storage:Provider=Local per dev.");
        }

        public Task<Stream?> GetAsync(string fileName, CancellationToken cancellationToken = default)
        {
            // TODO: Implementare download da Azure Blob Storage
            // var blobClient = _containerClient.GetBlobClient(fileName);
            // var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            // return response.Value.Content;
            throw new NotImplementedException("AzureBlobStorageProvider non ancora implementato.");
        }

        public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
        {
            // TODO: Implementare eliminazione da Azure Blob Storage
            // var blobClient = _containerClient.GetBlobClient(fileName);
            // await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            throw new NotImplementedException("AzureBlobStorageProvider non ancora implementato.");
        }
    }
}
