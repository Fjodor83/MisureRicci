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

public class UtentiControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<INegozioService> _mockNegozioService;
    private readonly UtentiController _controller;

    public UtentiControllerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _mockNegozioService = new Mock<INegozioService>();
        var factory = new TestDbContextFactory();
        var dbContext = factory.CreateContext();
        _controller = new UtentiController(_mockUserManager.Object, _mockNegozioService.Object, dbContext);
        
        SetupControllerUser("1", ApplicationRoles.Admin);
    }

    private void SetupControllerUser(string userId, string role)
    {
        SetupControllerUser(_controller, userId, role);
    }

    private static void SetupControllerUser(Controller controller, string userId, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Index_ReturnsViewWithUsers()
    {
        using var factory = new TestDbContextFactory();
        using var dbContext = factory.CreateContext();

        var negozio = new Negozio { Nome = "Roma" };
        var boutiqueRole = new IdentityRole
        {
            Id = "role-boutique-index",
            Name = ApplicationRoles.Boutique,
            NormalizedName = ApplicationRoles.Boutique.ToUpperInvariant()
        };
        var user = new ApplicationUser
        {
            Id = "user-index",
            UserName = "mrossi",
            NormalizedUserName = "MROSSI",
            Email = "mrossi@example.com",
            NormalizedEmail = "MROSSI@EXAMPLE.COM",
            EmailConfirmed = true,
            NomeCompleto = "Mario Rossi",
            Negozio = negozio,
            Attivo = true
        };

        dbContext.Negozi.Add(negozio);
        dbContext.Roles.Add(boutiqueRole);
        dbContext.Users.Add(user);
        dbContext.UserRoles.Add(new IdentityUserRole<string>
        {
            UserId = user.Id,
            RoleId = boutiqueRole.Id
        });
        await dbContext.SaveChangesAsync();

        var store = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        userManager.SetupGet(m => m.Users).Returns(dbContext.Users);

        var controller = new UtentiController(userManager.Object, _mockNegozioService.Object, dbContext);
        SetupControllerUser(controller, "admin-user", ApplicationRoles.Admin);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<ApplicationUser>>(viewResult.Model);
        var returnedUser = Assert.Single(model);
        Assert.Equal("Mario Rossi", returnedUser.NomeCompleto);
        Assert.Equal(ApplicationRoles.Boutique, returnedUser.Ruolo);
        Assert.Equal(negozio.Id, returnedUser.NegozioId);
    }

    [Fact]
    public async Task Create_GET_ReturnsViewWithNegozi()
    {
        // Arrange
        _mockNegozioService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Negozio>());

        // Act
        var result = await _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UtenteAdminPageViewModel>(viewResult.Model);
        Assert.NotNull(model.Negozi);
    }

    [Fact]
    public async Task Create_POST_RedirectsToIndex_WhenSuccessful()
    {
        // Arrange
        var model = new UtenteAdminPageViewModel
        {
            Form = new UtenteAdminViewModel
            {
                UserName = "test",
                Email = "test@example.com",
                NomeCompleto = "Test User",
                Ruolo = ApplicationRoles.Admin,
                Password = "Password123!"
            }
        };

        _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());
        _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
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
    public async Task Edit_GET_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Edit("1");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_POST_RedirectsToIndex_WhenSuccessful()
    {
        // Arrange
        var user = new ApplicationUser { Id = "1", UserName = "test" };
        var model = new UtenteAdminPageViewModel
        {
            Form = new UtenteAdminViewModel
            {
                Id = "1",
                UserName = "test",
                Email = "test@example.com",
                NomeCompleto = "Test User",
                Ruolo = ApplicationRoles.Admin
            }
        };

        _mockUserManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { ApplicationRoles.Admin });

        // Act
        var result = await _controller.Edit("1", model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task DeleteConfirmed_RedirectsToIndex()
    {
        // Arrange
        var user = new ApplicationUser { Id = "1" };
        _mockUserManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.DeleteConfirmed("1");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }
}
