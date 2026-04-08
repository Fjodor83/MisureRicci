using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Controllers;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MisureRicci.Tests;

public class CommissioniControllerTests
{
    private readonly Mock<ICommessaService> _mockCommessaService;
    private readonly Mock<IClienteService> _mockClienteService;
    private readonly Mock<ITenantService> _mockTenantService;
    private readonly CommissioniController _controller;

    public CommissioniControllerTests()
    {
        _mockCommessaService = new Mock<ICommessaService>();
        _mockClienteService = new Mock<IClienteService>();
        _mockTenantService = new Mock<ITenantService>();

        _controller = new CommissioniController(
            _mockCommessaService.Object,
            _mockClienteService.Object,
            _mockTenantService.Object,
            new Mock<ICustomMeasurementService>().Object,
            new Mock<IFabricService>().Object);
        
        // Setup default user via tenant service
        _mockTenantService.Setup(s => s.GetUserId()).Returns("user1");
        _mockTenantService.Setup(s => s.GetCurrentNegozioId()).Returns(1);
        _mockTenantService.Setup(s => s.IsAdmin()).Returns(false);
    }

    [Fact]
    public async Task Index_ReturnsViewWithModel()
    {
        // Arrange
        _mockCommessaService.Setup(s => s.GetCommissioniPagedAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new PagedResult<CommessaSartoriale>(new List<CommessaSartoriale>(), 0, 1, 20));
        _mockCommessaService.Setup(s => s.GetKpiAsync(It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync(new CommessaKpiViewModel());

        // Act
        var result = await _controller.Index(null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<CommessaIndexViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task Create_GET_ReturnsNotFound_WhenClienteDoesNotExist()
    {
        // Arrange
        _mockClienteService.Setup(s => s.GetClienteScopedAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _controller.Create(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_POST_RedirectsToDetails_WhenSuccessful()
    {
        // Arrange
        var cliente = new Cliente { Id = 1, Nome = "Test" };
        var model = new CommessaCreateViewModel { ClienteId = 1, TipoCapo = "Giacca" };
        _mockClienteService.Setup(s => s.GetClienteScopedAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync(cliente);
        _mockCommessaService.Setup(s => s.CreateCommessaAsync(It.IsAny<CommessaCreateViewModel>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync(Result<CommessaSartoriale>.Ok(new CommessaSartoriale { Id = 100 }));

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal(100, redirectResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenDetailsAreNull()
    {
        // Arrange
        _mockCommessaService.Setup(s => s.GetCommessaDetailsAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync((CommessaDetailsViewModel?)null);

        // Act
        var result = await _controller.Details(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AdvanceStato_RedirectsToDetails()
    {
        // Arrange
        _mockCommessaService.Setup(s => s.AdvanceStatoAsync(It.IsAny<int>(), It.IsAny<StatoCommessa>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _controller.AdvanceStato(1, StatoCommessa.InLavorazione, "nota");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
    }
}
