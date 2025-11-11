using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Moq;
using Server.Security;
using Server.Models;
using Xunit;

namespace Server.Tests.Authorization;

/// <summary>
/// Tests unitarios para TwoFactorEnabledHandler
/// Valida lógica de grace period y enforcement de 2FA
/// </summary>
public class TwoFactorEnabledHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly TwoFactorEnabledHandler _handler;

    public TwoFactorEnabledHandlerTests()
    {
        // Setup UserManager mock (requiere múltiples dependencias)
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null, null, null, null, null, null, null, null
        );

        _handler = new TwoFactorEnabledHandler(_mockUserManager.Object);
    }

    [Fact]
    public async Task HandleRequirementAsync_Usuario_NoEsAdmin_NiTesorero_Permite()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "user@test.com"),
            new Claim(ClaimTypes.Role, "Miembro") // Rol sin requisito 2FA
        }, "mock"));

        var context = new AuthorizationHandlerContext(
            new[] { new TwoFactorEnabledRequirement() },
            user,
            null
        );

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded, "Usuario sin rol Admin/Tesorero debe tener acceso sin 2FA");
    }

    [Fact]
    public async Task HandleRequirementAsync_AdminCon2FA_Permite()
    {
        // Arrange
        var appUser = new ApplicationUser
        {
            Id = "user-123",
            Email = "admin@test.com",
            TwoFactorEnabled = true,
            TwoFactorRequiredSince = null // No requiere grace period si ya tiene 2FA
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, "admin@test.com"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));

        _mockUserManager.Setup(x => x.GetUserAsync(user))
            .ReturnsAsync(appUser);

        var context = new AuthorizationHandlerContext(
            new[] { new TwoFactorEnabledRequirement() },
            user,
            null
        );

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded, "Admin con 2FA habilitado debe tener acceso");
    }

    [Fact]
    public async Task HandleRequirementAsync_TesoreroCon2FA_Permite()
    {
        // Arrange
        var appUser = new ApplicationUser
        {
            Id = "user-456",
            Email = "tesorero@test.com",
            TwoFactorEnabled = true,
            TwoFactorRequiredSince = null
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-456"),
            new Claim(ClaimTypes.Name, "tesorero@test.com"),
            new Claim(ClaimTypes.Role, "Tesorero")
        }, "mock"));

        _mockUserManager.Setup(x => x.GetUserAsync(user))
            .ReturnsAsync(appUser);

        var context = new AuthorizationHandlerContext(
            new[] { new TwoFactorEnabledRequirement() },
            user,
            null
        );

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded, "Tesorero con 2FA habilitado debe tener acceso");
    }

    [Fact]
    public async Task HandleRequirementAsync_AdminSin2FA_DentroGracePeriod_Permite()
    {
        // Arrange
        var appUser = new ApplicationUser
        {
            Id = "user-789",
            Email = "newadmin@test.com",
            TwoFactorEnabled = false,
            TwoFactorRequiredSince = DateTime.UtcNow.AddDays(-5) // 5 días atrás (dentro de 30 días)
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-789"),
            new Claim(ClaimTypes.Name, "newadmin@test.com"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));

        _mockUserManager.Setup(x => x.GetUserAsync(user))
            .ReturnsAsync(appUser);

        var context = new AuthorizationHandlerContext(
            new[] { new TwoFactorEnabledRequirement() },
            user,
            null
        );

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded, 
            "Admin sin 2FA dentro del grace period (30 días) debe tener acceso");
    }

    [Fact]
    public async Task HandleRequirementAsync_AdminSin2FA_FueraGracePeriod_Deniega()
    {
        // Arrange
        var appUser = new ApplicationUser
        {
            Id = "user-999",
            Email = "oldadmin@test.com",
            TwoFactorEnabled = false,
            TwoFactorRequiredSince = DateTime.UtcNow.AddDays(-31) // 31 días atrás (fuera de grace period)
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-999"),
            new Claim(ClaimTypes.Name, "oldadmin@test.com"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));

        _mockUserManager.Setup(x => x.GetUserAsync(user))
            .ReturnsAsync(appUser);

        var context = new AuthorizationHandlerContext(
            new[] { new TwoFactorEnabledRequirement() },
            user,
            null
        );

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded, 
            "Admin sin 2FA fuera del grace period debe ser denegado");
    }

    [Fact]
    public async Task HandleRequirementAsync_AdminSin2FA_SinFechaRequerida_Permite()
    {
        // Arrange: Usuario promovido a Admin pero sin fecha de requerimiento registrada
        var appUser = new ApplicationUser
        {
            Id = "user-legacy",
            Email = "legacy@test.com",
            TwoFactorEnabled = false,
            TwoFactorRequiredSince = null // No hay registro de cuándo se le requirió
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-legacy"),
            new Claim(ClaimTypes.Name, "legacy@test.com"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));

        _mockUserManager.Setup(x => x.GetUserAsync(user))
            .ReturnsAsync(appUser);

        var context = new AuthorizationHandlerContext(
            new[] { new TwoFactorEnabledRequirement() },
            user,
            null
        );

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded, 
            "Admin sin 2FA y sin fecha de requerimiento debe tener acceso (legacy user)");
    }

    [Fact]
    public async Task HandleRequirementAsync_TesoreroSin2FA_FueraGracePeriod_Deniega()
    {
        // Arrange
        var appUser = new ApplicationUser
        {
            Id = "user-tesorero",
            Email = "tesorero.old@test.com",
            TwoFactorEnabled = false,
            TwoFactorRequiredSince = DateTime.UtcNow.AddDays(-35)
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-tesorero"),
            new Claim(ClaimTypes.Name, "tesorero.old@test.com"),
            new Claim(ClaimTypes.Role, "Tesorero")
        }, "mock"));

        _mockUserManager.Setup(x => x.GetUserAsync(user))
            .ReturnsAsync(appUser);

        var context = new AuthorizationHandlerContext(
            new[] { new TwoFactorEnabledRequirement() },
            user,
            null
        );

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded, 
            "Tesorero sin 2FA fuera del grace period debe ser denegado");
    }

    [Fact]
    public async Task HandleRequirementAsync_UsuarioNoAutenticado_Deniega()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // Sin autenticar

        var context = new AuthorizationHandlerContext(
            new[] { new TwoFactorEnabledRequirement() },
            user,
            null
        );

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded, 
            "Usuario no autenticado debe ser denegado");
    }

    [Fact]
    public async Task HandleRequirementAsync_AdminYTesorero_Con2FA_Permite()
    {
        // Arrange: Usuario con múltiples roles
        var appUser = new ApplicationUser
        {
            Id = "user-multi",
            Email = "multi@test.com",
            TwoFactorEnabled = true
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-multi"),
            new Claim(ClaimTypes.Name, "multi@test.com"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "Tesorero")
        }, "mock"));

        _mockUserManager.Setup(x => x.GetUserAsync(user))
            .ReturnsAsync(appUser);

        var context = new AuthorizationHandlerContext(
            new[] { new TwoFactorEnabledRequirement() },
            user,
            null
        );

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded, 
            "Usuario con múltiples roles y 2FA debe tener acceso");
    }

    [Theory]
    [InlineData(-29, true)]  // 29 días = dentro del grace period
    [InlineData(-30, true)]  // 30 días = límite del grace period
    [InlineData(-31, false)] // 31 días = fuera del grace period
    [InlineData(-45, false)] // 45 días = muy fuera del grace period
    [InlineData(0, true)]    // Hoy mismo = dentro del grace period
    public async Task HandleRequirementAsync_GracePeriodBoundaries_ValidaCorrectamente(
        int diasAtras, 
        bool debePermitir)
    {
        // Arrange
        var appUser = new ApplicationUser
        {
            Id = "user-boundary",
            Email = "boundary@test.com",
            TwoFactorEnabled = false,
            TwoFactorRequiredSince = DateTime.UtcNow.AddDays(diasAtras)
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-boundary"),
            new Claim(ClaimTypes.Name, "boundary@test.com"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));

        _mockUserManager.Setup(x => x.GetUserAsync(user))
            .ReturnsAsync(appUser);

        var context = new AuthorizationHandlerContext(
            new[] { new TwoFactorEnabledRequirement() },
            user,
            null
        );

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.Equal(debePermitir, context.HasSucceeded);
    }
}
