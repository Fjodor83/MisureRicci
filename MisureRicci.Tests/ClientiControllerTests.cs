using Microsoft.AspNetCore.Mvc;
using MisureRicci.Controllers;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;
using Moq;
using Xunit;

namespace MisureRicci.Tests;

public class ClientiControllerTests
{
    private readonly Mock<IClienteService> _mockClienteService;
    private readonly Mock<INegozioService> _mockNegozioService;
    private readonly Mock<ITenantService> _mockTenantService;
    private readonly ClientiController _controller;

    public ClientiControllerTests()
    {
        _mockClienteService = new Mock<IClienteService>();
        _mockNegozioService = new Mock<INegozioService>();
        _mockTenantService = new Mock<ITenantService>();
        _controller = new ClientiController(_mockClienteService.Object, _mockNegozioService.Object, _mockTenantService.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewWithModel()
    {
        // Arrange
        _mockTenantService.Setup(s => s.IsAdmin()).Returns(false);
        _mockTenantService.Setup(s => s.GetCurrentNegozioId()).Returns(1);
        _mockClienteService.Setup(s => s.GetClientiPagedAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((new List<Cliente>(), 0));

        // Act
        var result = await _controller.Index("");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ClientiIndexViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenIdIsNull()
    {
        // Act
        var result = await _controller.Details(null);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenClienteDoesNotExist()
    {
        // Arrange
        _mockClienteService.Setup(s => s.GetClienteScopedAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _controller.Details(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewWithModel_WhenClienteExists()
    {
        // Arrange
        var cliente = new Cliente { Id = 1, Nome = "Test" };
        _mockClienteService.Setup(s => s.GetClienteScopedAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync(cliente);
        _mockClienteService.Setup(s => s.GetStoricoMisureScopedAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<MisureCliente>());

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ClienteDetailsViewModel>(viewResult.Model);
        Assert.Equal(cliente, model.Cliente);
    }

    [Fact]
    public async Task Create_GET_ReturnsView()
    {
        // Arrange
        _mockTenantService.Setup(s => s.IsAdmin()).Returns(false);

        // Act
        var result = await _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ClientePageViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task Create_POST_RedirectsToDetails_WhenSuccessful()
    {
        // Arrange
        var model = new ClientePageViewModel { Cliente = new Cliente { Nome = "Test" }, IsAdmin = false };
        _mockTenantService.Setup(s => s.IsAdmin()).Returns(false);
        _mockTenantService.Setup(s => s.GetCurrentNegozioId()).Returns(1);
        _mockClienteService.Setup(s => s.CreateClienteScopedAsync(It.IsAny<Cliente>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync(Result<Cliente>.Ok(new Cliente { Id = 42 }));

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal(42, redirectResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task Create_POST_ReturnsViewWithError_WhenServiceFails()
    {
        // Arrange
        var model = new ClientePageViewModel { Cliente = new Cliente { Nome = "Test" }, IsAdmin = false };
        _mockTenantService.Setup(s => s.IsAdmin()).Returns(false);
        _mockTenantService.Setup(s => s.GetCurrentNegozioId()).Returns(1);
        _mockClienteService.Setup(s => s.CreateClienteScopedAsync(It.IsAny<Cliente>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync(Result<Cliente>.Fail("Accesso negato: tenant non assegnato."));
        _mockNegozioService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Negozio>());

        var result = await _controller.Create(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);

    }

    [Fact]
    public async Task Edit_GET_ReturnsNotFound_WhenIdIsNull()
    {
        // Act
        var result = await _controller.Edit(null);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_POST_RedirectsToIndex_WhenSuccessful()
    {
        // Arrange
        var cliente = new Cliente { Id = 1, Nome = "Test" };
        var model = new ClientePageViewModel { Cliente = cliente, IsAdmin = false };
        _mockTenantService.Setup(s => s.IsAdmin()).Returns(false);
        _mockTenantService.Setup(s => s.GetCurrentNegozioId()).Returns(1);
        _mockClienteService.Setup(s => s.UpdateClienteScopedAsync(It.IsAny<Cliente>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Delete_GET_ReturnsNotFound_WhenClienteDoesNotExist()
    {
        // Arrange
        _mockClienteService.Setup(s => s.GetClienteScopedAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_RedirectsToIndex_WhenSuccessful()
    {
        // Arrange
        _mockClienteService.Setup(s => s.DeleteClienteScopedAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _controller.DeleteConfirmed(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }
}
