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
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MisureRicci.Tests;

public class HomeControllerTests
{
    private readonly Mock<IAuditLogQueryService> _mockAuditLogQueryService;
    private readonly Mock<IDashboardService> _mockDashboardService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _mockAuditLogQueryService = new Mock<IAuditLogQueryService>();

        _mockDashboardService = new Mock<IDashboardService>();
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _controller = new HomeController(_mockAuditLogQueryService.Object, _mockDashboardService.Object, _mockUserManager.Object);
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test@example.com"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, ApplicationRoles.Admin)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task Index_ReturnsViewWithDashboardKpi()
    {
        // Arrange
        var user = new ApplicationUser { Id = "1", NegozioId = 1 };
        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _mockDashboardService.Setup(s => s.GetKpiAsync(It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardKpiViewModel());

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<DashboardKpiViewModel>(viewResult.Model);
    }

    [Fact]
    public void Privacy_ReturnsView()
    {
        // Act
        var result = _controller.Privacy();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void UserGuide_ReturnsView()
    {
        var result = _controller.UserGuide();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void TailoringStandards_ReturnsView()
    {
        var result = _controller.TailoringStandards();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void TechnicalSupport_ReturnsView()
    {
        var result = _controller.TechnicalSupport();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task ActivityLog_ReturnsLatestEntriesOrderedDescending()
    {
        var entries = new List<AuditLog>
        {
            new() { EntityName = "Cliente", Action = "Create", Timestamp = new DateTime(2026, 4, 8, 8, 0, 0, DateTimeKind.Utc) },
            new() { EntityName = "Misura", Action = "Update", Timestamp = new DateTime(2026, 4, 10, 12, 0, 0, DateTimeKind.Utc) },
            new() { EntityName = "Commessa", Action = "Delete", Timestamp = new DateTime(2026, 4, 9, 9, 30, 0, DateTimeKind.Utc) }
        };
        _mockAuditLogQueryService
            .Setup(service => service.GetLatestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries.OrderByDescending(entry => entry.Timestamp).ToList());

        var result = await _controller.ActivityLog(CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IReadOnlyList<AuditLog>>(viewResult.Model);
        Assert.Equal(new[] { "Misura", "Commessa", "Cliente" }, model.Select(entry => entry.EntityName).ToArray());
    }

    [Fact]
    public void Error_ReturnsViewWithErrorViewModel()
    {
        // Act
        var result = _controller.Error();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ErrorViewModel>(viewResult.Model);
    }
}
