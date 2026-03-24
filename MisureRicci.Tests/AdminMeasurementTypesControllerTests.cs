using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MisureRicci.Controllers;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            _mockLogger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
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
    public async Task EditType_POST_ReturnsNotFound_WhenIdMismatch()
    {
        // Arrange
        var model = new MeasurementType { Id = 2 };
        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypeByIdAsync(1))
            .ReturnsAsync(new MeasurementType { Id = 1 });

        // Act
        var result = await _controller.EditType(1, model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditType_POST_ReturnsNotFound_WhenTypeDoesNotExist()
    {
        // Arrange
        var model = new MeasurementType { Id = 1 };
        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypeByIdAsync(1))
            .ReturnsAsync((MeasurementType?)null);

        // Act
        var result = await _controller.EditType(1, model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditType_POST_ReturnsView_WhenModelStateInvalid()
    {
        // Arrange
        var model = new MeasurementType { Id = 1 };
        var existing = new MeasurementType { Id = 1, ImageUrl = "old.jpg" };
        _controller.ModelState.AddModelError("Error", "Message");
        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypeByIdAsync(1))
            .ReturnsAsync(existing);

        // Act
        var result = await _controller.EditType(1, model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.Equal("old.jpg", model.ImageUrl);
    }

    [Fact]
    public async Task EditType_POST_RedirectsToIndex_WhenSuccessfulUpdate()
    {
        // Arrange
        var model = new MeasurementType { Id = 1, Nome = "Updated", Descrizione = "New Desc" };
        var existing = new MeasurementType { Id = 1, Nome = "Old", Descrizione = "Old Desc", ImageUrl = "old.jpg" };
        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypeByIdAsync(1))
            .ReturnsAsync(existing);
        _mockCustomMeasurementService.Setup(s => s.UpdateMeasurementTypeAsync(existing))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EditType(1, model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Updated", existing.Nome);
        Assert.Equal("old.jpg", existing.ImageUrl);
    }

    [Fact]
    public async Task EditType_POST_RedirectsToIndex_WhenSuccessfulUpdateWithImage()
    {
        // Arrange
        var mockImage = new Mock<IFormFile>();
        mockImage.Setup(i => i.Length).Returns(100);
        var model = new MeasurementType { Id = 1, Nome = "Updated", ImageUpload = mockImage.Object };
        var existing = new MeasurementType { Id = 1, Nome = "Old", ImageUrl = "old.jpg" };
        
        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypeByIdAsync(1))
            .ReturnsAsync(existing);
        _mockImageStorageService.Setup(s => s.SaveImageAsync(mockImage.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync("new.jpg");
        _mockCustomMeasurementService.Setup(s => s.UpdateMeasurementTypeAsync(existing))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EditType(1, model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("new.jpg", existing.ImageUrl);
        _mockImageStorageService.Verify(s => s.DeleteImageAsync("old.jpg", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EditType_POST_ReturnsView_WhenImageValidationException()
    {
        // Arrange
        var mockImage = new Mock<IFormFile>();
        mockImage.Setup(i => i.Length).Returns(100);
        var model = new MeasurementType { Id = 1, ImageUpload = mockImage.Object };
        var existing = new MeasurementType { Id = 1, ImageUrl = "old.jpg" };

        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypeByIdAsync(1))
            .ReturnsAsync(existing);
        _mockImageStorageService.Setup(s => s.SaveImageAsync(mockImage.Object, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MeasurementTypeImageValidationException("Invalid image"));

        // Act
        var result = await _controller.EditType(1, model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.Equal("old.jpg", model.ImageUrl);
    }

    [Fact]
    public async Task DeleteType_POST_RedirectsToIndex()
    {
        // Arrange
        var existing = new MeasurementType { Id = 1, ImageUrl = "test.jpg", IsSystem = false };
        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypeByIdAsync(1))
            .ReturnsAsync(existing);

        // Act
        var result = await _controller.DeleteType(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        _mockCustomMeasurementService.Verify(s => s.DeleteMeasurementTypeAsync(1), Times.Once);
        _mockImageStorageService.Verify(s => s.DeleteImageAsync("test.jpg", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EditField_GET_ReturnsViewWithModel()
    {
        // Arrange
        var field = new MeasurementFieldDefinition { Id = 10, MeasurementTypeId = 1 };
        var type = new MeasurementType { Id = 1, Nome = "TestType" };
        _mockCustomMeasurementService.Setup(s => s.GetFieldByIdAsync(10))
            .ReturnsAsync(field);
        _mockCustomMeasurementService.Setup(s => s.GetMeasurementTypeByIdAsync(1))
            .ReturnsAsync(type);

        // Act
        var result = await _controller.EditField(10);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<MeasurementFieldPageViewModel>(viewResult.Model);
        Assert.Equal("TestType", vm.TypeName);
        Assert.Equal(field, vm.Field);
    }

    [Fact]
    public async Task EditField_POST_RedirectsToFields_WhenSuccessful()
    {
        // Arrange
        var pageModel = new MeasurementFieldPageViewModel
        {
            Field = new MeasurementFieldDefinition { Id = 10, MeasurementTypeId = 1 }
        };
        _mockCustomMeasurementService.Setup(s => s.UpdateFieldAsync(pageModel.Field))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EditField(10, pageModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Fields", redirectResult.ActionName);
        Assert.Equal(1, redirectResult.RouteValues?["typeId"]);
    }

    [Fact]
    public async Task DeleteField_POST_RedirectsToFields()
    {
        // Act
        var result = await _controller.DeleteField(10, 1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Fields", redirectResult.ActionName);
        Assert.Equal(1, redirectResult.RouteValues?["typeId"]);
        _mockCustomMeasurementService.Verify(s => s.DeleteFieldAsync(10), Times.Once);
    }
}
