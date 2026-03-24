using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MisureRicci.Controllers;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MisureRicci.Tests;

public class AdminMeasurementTypesControllerTests
{
    private readonly Mock<ICustomMeasurementService> _mockCustomMeasurementService;
    private readonly Mock<IMeasurementTypeImageStorageService> _mockImageStorageService;
    private readonly Mock<ILogger<AdminMeasurementTypesController>> _mockLogger;
    private readonly AdminMeasurementTypesController _controller;

    public AdminMeasurementTypesControllerTests()
    {
        _mockCustomMeasurementService = new Mock<ICustomMeasurementService>();
        _mockImageStorageService = new Mock<IMeasurementTypeImageStorageService>();
        _mockLogger = new Mock<ILogger<AdminMeasurementTypesController>>();
        _controller = new AdminMeasurementTypesController(
            _mockCustomMeasurementService.Object,
            _mockImageStorageService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewWithMeasurementTypes()
    {
        // Arrange
        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypesAsync(false))
            .ReturnsAsync(new List<MeasurementType>());

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<IEnumerable<MeasurementType>>(viewResult.Model);
    }

    [Fact]
    public async Task CreateType_POST_RedirectsToIndex_WhenSuccessful()
    {
        // Arrange
        var model = new MeasurementType { Nome = "TestType" };
        _mockCustomMeasurementService.Setup(s => s.CreateMeasurementTypeAsync(model))
            .ReturnsAsync(model);

        // Act
        var result = await _controller.CreateType(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task EditType_GET_ReturnsNotFound_WhenTypeDoesNotExist()
    {
        // Arrange
        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypeByIdAsync(1))
            .ReturnsAsync((MeasurementType?)null);

        // Act
        var result = await _controller.EditType(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Fields_ReturnsViewWithModel()
    {
        // Arrange
        var type = new MeasurementType { Id = 1, Nome = "Test" };
        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypeByIdAsync(1))
            .ReturnsAsync(type);
        _mockCustomMeasurementService.Setup(s => s.GetFieldsByTypeAsync(1, false))
            .ReturnsAsync(new List<MeasurementFieldDefinition>());

        // Act
        var result = await _controller.Fields(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MeasurementTypeManageViewModel>(viewResult.Model);
        Assert.Equal(type, model.Type);
    }

    [Fact]
    public async Task CreateField_POST_RedirectsToFields_WhenSuccessful()
    {
        // Arrange
        var pageModel = new MeasurementFieldPageViewModel
        {
            Field = new MeasurementFieldDefinition { MeasurementTypeId = 1, NomeCampo = "Field1" }
        };
        _mockCustomMeasurementService.Setup(s => s.CreateFieldAsync(pageModel.Field))
            .ReturnsAsync(pageModel.Field);

        // Act
        var result = await _controller.CreateField(pageModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Fields", redirectResult.ActionName);
        Assert.Equal(1, redirectResult.RouteValues?["typeId"]);
    }
}
