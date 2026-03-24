using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;

namespace MisureRicci.Services
{
    public interface IMeasurementTypeImageStorageService
    {
        long MaxFileSizeBytes { get; }
        Task<string> SaveImageAsync(IFormFile file, CancellationToken cancellationToken = default);
        MeasurementTypeImageFile? GetImage(string fileName);
        Task DeleteImageAsync(string? imageUrl, CancellationToken cancellationToken = default);
        Task<int> MigrateLegacyImagesAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default);
    }

    public sealed class MeasurementTypeImageValidationException : InvalidOperationException
    {
        public MeasurementTypeImageValidationException(string message) : base(message)
        {
        }
    }

    public sealed class MeasurementTypeImageFile
    {
        public MeasurementTypeImageFile(string fileName, string physicalPath, string contentType)
        {
            FileName = fileName;
            PhysicalPath = physicalPath;
            ContentType = contentType;
        }

        public string FileName { get; }
        public string PhysicalPath { get; }
        public string ContentType { get; }
    }

    public sealed class MeasurementTypeImageStorageService : IMeasurementTypeImageStorageService
    {
        private const string ProtectedRoutePrefix = "/images/measurement-types/";
        private const string LegacyRoutePrefix = "/uploads/measurement-types/";
        private const long DefaultMaxFileSizeBytes = 5 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private static readonly Regex SafeFileNamePattern = new(
            "^[a-f0-9]{32}\\.(jpg|jpeg|png|webp)$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<MeasurementTypeImageStorageService> _logger;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

        public MeasurementTypeImageStorageService(
            IWebHostEnvironment environment,
            ILogger<MeasurementTypeImageStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public long MaxFileSizeBytes => DefaultMaxFileSizeBytes;

        public async Task<string> SaveImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            ValidateUpload(file);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            await using (var validationStream = file.OpenReadStream())
            {
                var signatureBuffer = new byte[12];
                var bytesRead = await validationStream.ReadAsync(signatureBuffer.AsMemory(0, signatureBuffer.Length), cancellationToken);
                if (!HasValidSignature(extension, signatureBuffer, bytesRead))
                {
                    throw new MeasurementTypeImageValidationException("Il contenuto del file non corrisponde a un'immagine valida.");
                }
            }

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(GetSecureStoragePath(), fileName);

            await using var sourceStream = file.OpenReadStream();
            await using var targetStream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await sourceStream.CopyToAsync(targetStream, cancellationToken);

            return BuildProtectedUrl(fileName);
        }

        public MeasurementTypeImageFile? GetImage(string fileName)
        {
            if (!IsSafeManagedFileName(fileName))
            {
                return null;
            }

            var physicalPath = Path.Combine(GetSecureStoragePath(), fileName);
            if (!File.Exists(physicalPath))
            {
                return null;
            }

            if (!_contentTypeProvider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return new MeasurementTypeImageFile(fileName, physicalPath, contentType);
        }

        public Task DeleteImageAsync(string? imageUrl, CancellationToken cancellationToken = default)
        {
            if (!TryResolveManagedPath(imageUrl, out var physicalPath))
            {
                return Task.CompletedTask;
            }

            if (!File.Exists(physicalPath))
            {
                return Task.CompletedTask;
            }

            File.Delete(physicalPath);
            return Task.CompletedTask;
        }

        public async Task<int> MigrateLegacyImagesAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            var legacyMeasurementTypes = await dbContext.DynamicMeasurementTypes
                .Where(x => x.ImageUrl != null && x.ImageUrl.StartsWith(LegacyRoutePrefix))
                .ToListAsync(cancellationToken);

            var migratedCount = 0;
            var secureStoragePath = GetSecureStoragePath();
            var quarantinePath = GetQuarantinePath();

            foreach (var measurementType in legacyMeasurementTypes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!TryResolveManagedPath(measurementType.ImageUrl, out var legacyPath, legacyOnly: true))
                {
                    _logger.LogWarning(
                        "Impossibile risolvere il percorso legacy per la tipologia misura {MeasurementTypeId} con URL {ImageUrl}.",
                        measurementType.Id,
                        measurementType.ImageUrl);
                    measurementType.ImageUrl = null;
                    migratedCount++;
                    continue;
                }

                if (!File.Exists(legacyPath))
                {
                    _logger.LogWarning(
                        "File legacy non trovato per la tipologia misura {MeasurementTypeId}: {LegacyPath}.",
                        measurementType.Id,
                        legacyPath);
                    measurementType.ImageUrl = null;
                    migratedCount++;
                    continue;
                }

                var extension = Path.GetExtension(legacyPath).ToLowerInvariant();
                var destinationDirectory = secureStoragePath;

                if (!AllowedExtensions.Contains(extension) || !await FileHasValidSignatureAsync(legacyPath, extension, cancellationToken))
                {
                    _logger.LogWarning(
                        "Il file legacy {LegacyPath} per la tipologia misura {MeasurementTypeId} non supera i controlli di sicurezza ed e stato spostato in quarantena.",
                        legacyPath,
                        measurementType.Id);
                    destinationDirectory = quarantinePath;
                    measurementType.ImageUrl = null;
                }
                else
                {
                    measurementType.ImageUrl = BuildProtectedUrl($"{Guid.NewGuid():N}{extension}");
                }

                var destinationFileName = measurementType.ImageUrl == null
                    ? $"{Guid.NewGuid():N}{extension}"
                    : ExtractFileNameFromUrl(measurementType.ImageUrl)!;

                var destinationPath = Path.Combine(destinationDirectory, destinationFileName);
                File.Move(legacyPath, destinationPath, overwrite: false);
                migratedCount++;
            }

            if (migratedCount > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            QuarantineOrphanedLegacyFiles(quarantinePath);

            return migratedCount;
        }

        private void ValidateUpload(IFormFile file)
        {
            if (file == null)
            {
                throw new MeasurementTypeImageValidationException("Nessun file caricato.");
            }

            if (file.Length <= 0)
            {
                throw new MeasurementTypeImageValidationException("Il file caricato e vuoto.");
            }

            if (file.Length > DefaultMaxFileSizeBytes)
            {
                throw new MeasurementTypeImageValidationException("File troppo grande. Il limite massimo e 5 MB.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            {
                throw new MeasurementTypeImageValidationException("Formato non consentito. Sono ammessi solo JPG, PNG e WEBP.");
            }
        }

        private string GetSecureStoragePath()
        {
            var path = Path.Combine(_environment.ContentRootPath, "SecureUploads", "measurement-types");
            Directory.CreateDirectory(path);
            return path;
        }

        private string GetQuarantinePath()
        {
            var path = Path.Combine(_environment.ContentRootPath, "SecureUploads", "quarantine", "measurement-types");
            Directory.CreateDirectory(path);
            return path;
        }

        private void QuarantineOrphanedLegacyFiles(string quarantinePath)
        {
            var legacyDirectory = Path.Combine(_environment.WebRootPath, "uploads", "measurement-types");
            if (!Directory.Exists(legacyDirectory))
            {
                return;
            }

            foreach (var legacyPath in Directory.EnumerateFiles(legacyDirectory))
            {
                var extension = Path.GetExtension(legacyPath).ToLowerInvariant();
                var destinationPath = Path.Combine(quarantinePath, $"{Guid.NewGuid():N}{extension}");
                File.Move(legacyPath, destinationPath, overwrite: false);

                _logger.LogWarning(
                    "File legacy pubblico non referenziato trovato in {LegacyPath} e spostato in quarantena {DestinationPath}.",
                    legacyPath,
                    destinationPath);
            }
        }

        private string BuildProtectedUrl(string fileName)
        {
            return $"{ProtectedRoutePrefix}{fileName}";
        }

        private string? ExtractFileNameFromUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return null;
            }

            if (!imageUrl.StartsWith(ProtectedRoutePrefix, StringComparison.OrdinalIgnoreCase)
                && !imageUrl.StartsWith(LegacyRoutePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var prefix = imageUrl.StartsWith(ProtectedRoutePrefix, StringComparison.OrdinalIgnoreCase)
                ? ProtectedRoutePrefix
                : LegacyRoutePrefix;

            var fileName = Uri.UnescapeDataString(imageUrl[prefix.Length..]);
            return Path.GetFileName(fileName) == fileName ? fileName : null;
        }

        private bool TryResolveManagedPath(string? imageUrl, out string physicalPath, bool legacyOnly = false)
        {
            physicalPath = string.Empty;

            var fileName = ExtractFileNameFromUrl(imageUrl);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            var isLegacy = imageUrl!.StartsWith(LegacyRoutePrefix, StringComparison.OrdinalIgnoreCase);
            if (legacyOnly && !isLegacy)
            {
                return false;
            }

            if (isLegacy)
            {
                physicalPath = Path.Combine(_environment.WebRootPath, "uploads", "measurement-types", fileName);
                return true;
            }

            if (!IsSafeManagedFileName(fileName))
            {
                return false;
            }

            physicalPath = Path.Combine(GetSecureStoragePath(), fileName);
            return true;
        }

        private static bool IsSafeManagedFileName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName)
                && Path.GetFileName(fileName) == fileName
                && SafeFileNamePattern.IsMatch(fileName);
        }

        private static async Task<bool> FileHasValidSignatureAsync(string physicalPath, string extension, CancellationToken cancellationToken)
        {
            await using var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var buffer = new byte[12];
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            return HasValidSignature(extension, buffer, bytesRead);
        }

        private static bool HasValidSignature(string extension, byte[] buffer, int bytesRead)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => bytesRead >= 3
                    && buffer[0] == 0xFF
                    && buffer[1] == 0xD8
                    && buffer[2] == 0xFF,
                ".png" => bytesRead >= 8
                    && buffer[0] == 0x89
                    && buffer[1] == 0x50
                    && buffer[2] == 0x4E
                    && buffer[3] == 0x47
                    && buffer[4] == 0x0D
                    && buffer[5] == 0x0A
                    && buffer[6] == 0x1A
                    && buffer[7] == 0x0A,
                ".webp" => bytesRead >= 12
                    && buffer[0] == 0x52
                    && buffer[1] == 0x49
                    && buffer[2] == 0x46
                    && buffer[3] == 0x46
                    && buffer[8] == 0x57
                    && buffer[9] == 0x45
                    && buffer[10] == 0x42
                    && buffer[11] == 0x50,
                _ => false
            };
        }
    }
}
