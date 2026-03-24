using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MisureRicci.Controllers;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MisureRicci.Tests;

public class DynamicMeasurementsControllerTests
{
    private readonly Mock<ICustomMeasurementService> _mockCustomMeasurementService;
    private readonly Mock<IClienteService> _mockClienteService;
    private readonly Mock<ICommessaService> _mockCommessaService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<DynamicMeasurementsController>> _mockLogger;
    private readonly DynamicMeasurementsController _controller;

    public DynamicMeasurementsControllerTests()
    {
        _mockCustomMeasurementService = new Mock<ICustomMeasurementService>();
        _mockClienteService = new Mock<IClienteService>();
        _mockCommessaService = new Mock<ICommessaService>();
        
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockLogger = new Mock<ILogger<DynamicMeasurementsController>>();

        _controller = new DynamicMeasurementsController(
            _mockCustomMeasurementService.Object,
            _mockClienteService.Object,
            _mockCommessaService.Object,
            _mockUserManager.Object,
            _mockLogger.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "Admin"),
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task Create_GET_ReturnsViewWithViewModel_WhenSuccessful()
    {
        // Arrange
        var cliente = new Cliente { Id = 1, Nome = "Mario", Cognome = "Rossi" };
        var type = new MeasurementType { Id = 1, Nome = "Tipo1", IsActive = true };
        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new ApplicationUser { Id = "1" });
        _mockClienteService.Setup(s => s.GetClienteScopedAsync(1, It.IsAny<int?>(), true))
            .ReturnsAsync(cliente);
        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypeByIdAsync(1))
            .ReturnsAsync(type);
        _mockCustomMeasurementService.Setup(s => s.GetFieldsByTypeAsync(1, true))
            .ReturnsAsync(new List<MeasurementFieldDefinition>());

        // Act
        var result = await _controller.Create(1, 1, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DynamicMeasurementCreateViewModel>(viewResult.Model);
        Assert.Equal(1, model.ClienteId);
        Assert.Equal(1, model.MeasurementTypeId);
        Assert.Equal("Mario Rossi", model.ClienteNome);
    }

    [Fact]
    public async Task Create_POST_RedirectsToClientDetails_WhenSuccessful()
    {
        // Arrange
        var model = new DynamicMeasurementCreateViewModel { ClienteId = 1, MeasurementTypeId = 1 };
        var cliente = new Cliente { Id = 1, Nome = "Mario", Cognome = "Rossi" };
        var type = new MeasurementType { Id = 1, Nome = "Tipo1" };
        var record = new DynamicMeasurementRecord { Id = 10 };

        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new ApplicationUser { Id = "1" });
        _mockClienteService.Setup(s => s.GetClienteScopedAsync(1, It.IsAny<int?>(), true))
            .ReturnsAsync(cliente);
        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypeByIdAsync(1))
            .ReturnsAsync(type);
        _mockCustomMeasurementService.Setup(s => s.CreateDynamicMeasurementAsync(model, "1"))
            .ReturnsAsync(record);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal("Clienti", redirectResult.ControllerName);
        Assert.Equal(1, redirectResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenRecordDoesNotExist()
    {
        // Arrange
        _mockCustomMeasurementService.Setup(s => s.GetDynamicMeasurementRecordByIdAsync(1))
            .ReturnsAsync((DynamicMeasurementRecord?)null);

        // Act
        var result = await _controller.Details(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_POST_RedirectsToClientDetails()
    {
        // Arrange
        var cliente = new Cliente { Id = 1 };
        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new ApplicationUser { Id = "1" });
        _mockClienteService.Setup(s => s.GetClienteScopedAsync(1, It.IsAny<int?>(), true))
            .ReturnsAsync(cliente);

        // Act
        var result = await _controller.Delete(10, 1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal("Clienti", redirectResult.ControllerName);
        Assert.Equal(1, redirectResult.RouteValues?["id"]);
        _mockCustomMeasurementService.Verify(s => s.DeleteDynamicMeasurementAsync(10), Times.Once);
    }
}
