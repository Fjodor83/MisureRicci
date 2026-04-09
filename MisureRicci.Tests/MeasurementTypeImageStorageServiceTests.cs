using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using MisureRicci.Models;
using MisureRicci.Services;
using Xunit;

namespace MisureRicci.Tests;

public class MeasurementTypeImageStorageServiceTests : IDisposable
{
    private readonly string _rootPath;
    private readonly TestWebHostEnvironment _environment;
    private readonly MeasurementTypeImageStorageService _service;

    public MeasurementTypeImageStorageServiceTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), "misurericci-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_rootPath);
        Directory.CreateDirectory(Path.Combine(_rootPath, "wwwroot"));

        _environment = new TestWebHostEnvironment
        {
            ApplicationName = "MisureRicci.Tests",
            EnvironmentName = "Development",
            ContentRootPath = _rootPath,
            ContentRootFileProvider = new PhysicalFileProvider(_rootPath),
            WebRootPath = Path.Combine(_rootPath, "wwwroot"),
            WebRootFileProvider = new PhysicalFileProvider(Path.Combine(_rootPath, "wwwroot"))
        };

        var storageProvider = new LocalFileStorageProvider(Path.Combine(_rootPath, "SecureUploads"));
        _service = new MeasurementTypeImageStorageService(_environment, storageProvider, NullLogger<MeasurementTypeImageStorageService>.Instance);
    }

    [Fact]
    public async Task SaveImageAsync_WithValidPng_SavesOutsideWebRootAndReturnsProtectedUrl()
    {
        var formFile = CreateFormFile("schema.png", "image/png", CreatePngBytes());

        var imageUrl = await _service.SaveImageAsync(formFile);

        Assert.StartsWith("/images/measurement-types/", imageUrl, StringComparison.Ordinal);

        var fileName = Path.GetFileName(imageUrl);
        var storedImage = _service.GetImage(fileName);

        Assert.NotNull(storedImage);
        Assert.StartsWith(Path.Combine(_rootPath, "SecureUploads"), storedImage!.PhysicalPath, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain($"{Path.DirectorySeparatorChar}wwwroot{Path.DirectorySeparatorChar}", storedImage.PhysicalPath, StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(storedImage.PhysicalPath));
    }

    [Fact]
    public async Task SaveImageAsync_WithInvalidExtension_ThrowsValidationException()
    {
        var formFile = CreateFormFile("payload.aspx", "application/octet-stream", CreatePngBytes());

        var exception = await Assert.ThrowsAsync<MeasurementTypeImageValidationException>(() => _service.SaveImageAsync(formFile));

        Assert.Contains("Formato non consentito", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MigrateLegacyImagesAsync_MovesLegacyFileToProtectedStorageAndUpdatesUrl()
    {
        using var factory = new TestDbContextFactory();
        var legacyDirectory = Path.Combine(_environment.WebRootPath, "uploads", "measurement-types");
        Directory.CreateDirectory(legacyDirectory);

        var legacyFileName = "legacy-image.png";
        var legacyPath = Path.Combine(legacyDirectory, legacyFileName);
        await File.WriteAllBytesAsync(legacyPath, CreatePngBytes());

        using (var seedContext = factory.CreateContext())
        {
            seedContext.DynamicMeasurementTypes.Add(new MeasurementType
            {
                Nome = "Giacca",
                ImageUrl = $"/uploads/measurement-types/{legacyFileName}",
                IsActive = true
            });

            await seedContext.SaveChangesAsync();
        }

        using var actContext = factory.CreateContext();
        var migratedCount = await _service.MigrateLegacyImagesAsync(actContext);

        Assert.Equal(1, migratedCount);
        Assert.False(File.Exists(legacyPath));

        var measurementType = await actContext.DynamicMeasurementTypes.SingleAsync();
        Assert.StartsWith("/images/measurement-types/", measurementType.ImageUrl, StringComparison.Ordinal);

        var migratedFileName = Path.GetFileName(measurementType.ImageUrl);
        var storedImage = _service.GetImage(migratedFileName);
        Assert.NotNull(storedImage);
        Assert.True(File.Exists(storedImage!.PhysicalPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }

    private static FormFile CreateFormFile(string fileName, string contentType, byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "ImageUpload", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static byte[] CreatePngBytes()
    {
        return
        [
            0x89, 0x50, 0x4E, 0x47,
            0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52
        ];
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
