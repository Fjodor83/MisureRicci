using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Controllers;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MisureRicci.Tests;

public class MeasurementsControllerTests
{
    private readonly Mock<IMeasurementRegistryService> _mockRegistryService;
    private readonly Mock<ILegacyMeasurementService> _mockLegacyService;
    private readonly Mock<IClienteService> _mockClienteService;
    private readonly Mock<ICustomMeasurementService> _mockCustomService;
    private readonly Mock<ILegacyMeasurementUiService> _mockUiService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly MeasurementsController _controller;

    public MeasurementsControllerTests()
    {
        _mockRegistryService = new Mock<IMeasurementRegistryService>();
        _mockLegacyService = new Mock<ILegacyMeasurementService>();
        _mockClienteService = new Mock<IClienteService>();
        _mockCustomService = new Mock<ICustomMeasurementService>();
        _mockUiService = new Mock<ILegacyMeasurementUiService>();
        
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _controller = new MeasurementsController(
            _mockRegistryService.Object,
            _mockLegacyService.Object,
            _mockClienteService.Object,
            _mockCustomService.Object,
            _mockUiService.Object,
            _mockUserManager.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Admin"),
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GlobalRegistry_ReturnsViewWithModel()
    {
        // Arrange
        var items = new List<MisureCliente>();
        var result = (Items: (IEnumerable<MisureCliente>)items, TotalCount: 0);
        _mockRegistryService.Setup(s => s.GetGlobalRegistryPagedAsync(It.IsAny<string>(), It.IsAny<int?>(), true, 1, 20))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.GlobalRegistry(null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(actionResult);
        var model = Assert.IsType<MeasurementsGlobalRegistryViewModel>(viewResult.Model);
        Assert.Equal(1, model.CurrentPage);
    }

    [Fact]
    public async Task Index_RedirectsToClienti_WhenClienteIdIsNull()
    {
        // Act
        var result = await _controller.Index(null);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Clienti", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Index_ReturnsViewWithDashboard_WhenSuccessful()
    {
        // Arrange
        var cliente = new Cliente { Id = 1, Nome = "Mario", Cognome = "Rossi" };
        _mockClienteService.Setup(s => s.GetClienteScopedAsync(1, It.IsAny<int?>(), true))
            .ReturnsAsync(cliente);
        _mockCustomService.Setup(s => s.GetMeasurementTypesAsync(true))
            .ReturnsAsync(new List<MeasurementType>());

        // Act
        var result = await _controller.Index(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MeasurementsDashboardViewModel>(viewResult.Model);
        Assert.Equal(1, model.ClienteId);
        Assert.Equal("Mario Rossi", model.ClienteNome);
    }

    [Fact]
    public async Task Details_RedirectsToDynamic_WhenRegistryEntryIsDynamic()
    {
        // Arrange
        var entry = new MisureCliente { Id = 100, IsDynamic = true, RecordId = 50, TipoMisura = "DynamicType" };
        _mockRegistryService.Setup(s => s.GetRegistryEntryAsync(100, It.IsAny<int?>(), true))
            .ReturnsAsync(entry);

        // Act
        var result = await _controller.Details(1, "DynamicType", 100);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal("DynamicMeasurements", redirectResult.ControllerName);
        Assert.Equal(50, redirectResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task Edit_GET_ReturnsViewWithLegacyModel()
    {
        // Arrange
        var legacyModel = new object();
        var editVm = new LegacyMeasurementEditViewModel { Id = 1, TipoMisura = "LegacyType" };
        _mockLegacyService.Setup(s => s.GetMeasurementScopedAsync(1, "LegacyType", It.IsAny<int?>(), true))
            .ReturnsAsync(legacyModel);
        _mockUiService.Setup(s => s.BuildEditViewModel(legacyModel, "LegacyType", null))
            .Returns(editVm);

        // Act
        var result = await _controller.Edit(1, "LegacyType");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(editVm, viewResult.Model);
    }

    [Fact]
    public async Task DeleteConfirmed_RedirectsToClientDetails()
    {
        // Arrange
        var legacyModel = new object();
        _mockLegacyService.Setup(s => s.GetMeasurementScopedAsync(1, "LegacyType", It.IsAny<int?>(), true))
            .ReturnsAsync(legacyModel);
        _mockUiService.Setup(s => s.GetClienteId(legacyModel)).Returns(5);

        // Act
        var result = await _controller.DeleteConfirmed(1, "LegacyType", null);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal("Clienti", redirectResult.ControllerName);
        Assert.Equal(5, redirectResult.RouteValues?["id"]);
        _mockLegacyService.Verify(s => s.DeleteMeasurementAsync(1, "LegacyType", It.IsAny<int?>(), true), Times.Once);
    }
}
