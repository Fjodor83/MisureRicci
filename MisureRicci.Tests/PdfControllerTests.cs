using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Controllers;
using MisureRicci.Models;
using MisureRicci.Services;
using Moq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MisureRicci.Tests;

public class PdfControllerTests
{
    private readonly Mock<IPdfService> _mockPdfService;
    private readonly Mock<IClienteService> _mockClienteService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly PdfController _controller;

    public PdfControllerTests()
    {
        _mockPdfService = new Mock<IPdfService>();
        _mockClienteService = new Mock<IClienteService>();
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _controller = new PdfController(_mockPdfService.Object, _mockClienteService.Object, _mockUserManager.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test@example.com"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task DossierCliente_ReturnsNotFound_WhenClienteDoesNotExist()
    {
        // Arrange
        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new ApplicationUser { Id = "1" });
        _mockClienteService.Setup(s => s.GetClienteScopedAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _controller.DossierCliente(1, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DossierCliente_ReturnsFile_WhenSuccessful()
    {
        // Arrange
        var cliente = new Cliente { Id = 1, ClientCode = "C001" };
        var pdfBytes = new byte[] { 1, 2, 3 };

        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new ApplicationUser { Id = "1", NegozioId = 1 });
        _mockClienteService.Setup(s => s.GetClienteScopedAsync(1, It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync(cliente);
        _mockPdfService.Setup(s => s.GenerateDossierPdfAsync(1, It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pdfBytes);

        // Act
        var result = await _controller.DossierCliente(1, CancellationToken.None);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("dossier-C001.pdf", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task DossierCliente_ReturnsForbid_WhenUnauthorized()
    {
        // Arrange
        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new ApplicationUser { Id = "1" });
        _mockClienteService.Setup(s => s.GetClienteScopedAsync(1, It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync(new Cliente { Id = 1 });
        _mockPdfService.Setup(s => s.GenerateDossierPdfAsync(1, It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.UnauthorizedAccessException());

        // Act
        var result = await _controller.DossierCliente(1, CancellationToken.None);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }
}
