using Microsoft.AspNetCore.Mvc;
using MisureRicci.Controllers;
using MisureRicci.Models;
using MisureRicci.Services;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MisureRicci.Tests;

public class NegoziControllerTests
{
    private readonly Mock<INegozioService> _mockNegozioService;
    private readonly NegoziController _controller;

    public NegoziControllerTests()
    {
        _mockNegozioService = new Mock<INegozioService>();
        _controller = new NegoziController(_mockNegozioService.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewWithNegozi()
    {
        // Arrange
        _mockNegozioService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Negozio>());

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<IEnumerable<Negozio>>(viewResult.Model);
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
    public async Task Details_ReturnsView_WhenNegozioExists()
    {
        // Arrange
        var negozio = new Negozio { Id = 1, Nome = "Test" };
        _mockNegozioService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(negozio);

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(negozio, viewResult.Model);
    }

    [Fact]
    public async Task Create_POST_RedirectsToIndex_WhenModelStateIsValid()
    {
        // Arrange
        var negozio = new Negozio { Nome = "Test" };
        _mockNegozioService.Setup(s => s.CreateAsync(negozio)).ReturnsAsync(Result<Negozio>.Ok(negozio));

        // Act
        var result = await _controller.Create(negozio);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Edit_POST_RedirectsToIndex_WhenSuccessful()
    {
        // Arrange
        var negozio = new Negozio { Id = 1, Nome = "Test" };
        _mockNegozioService.Setup(s => s.UpdateAsync(negozio)).ReturnsAsync(Result.Ok());

        // Act
        var result = await _controller.Edit(1, negozio);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task DeleteConfirmed_RedirectsToIndex()
    {
        // Arrange
        _mockNegozioService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(Result.Ok());

        // Act
        var result = await _controller.DeleteConfirmed(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }
}
