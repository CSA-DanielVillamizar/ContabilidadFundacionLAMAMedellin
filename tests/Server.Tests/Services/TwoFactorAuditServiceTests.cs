using Moq;
using Server.Services.Audit;
using Server.Services.Auth;
using Xunit;

namespace Server.Tests.Services;

/// <summary>
/// Tests unitarios para TwoFactorAuditService
/// Valida registro correcto de eventos 2FA en auditoría
/// </summary>
public class TwoFactorAuditServiceTests
{
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly TwoFactorAuditService _service;

    public TwoFactorAuditServiceTests()
    {
        _mockAuditService = new Mock<IAuditService>();
        _service = new TwoFactorAuditService(_mockAuditService.Object);
    }

    [Fact]
    public async Task LogTwoFactorEnabledAsync_RegistraEvento_ConGracePeriod()
    {
        // Arrange
        var userName = "admin@test.com";
        var gracePeriodDays = 30;

        // Act
        await _service.LogTwoFactorEnabledAsync(userName, gracePeriodDays);

        // Assert
        _mockAuditService.Verify(x => x.LogAsync(
            "2FA",
            userName,
            "2FA_ENABLED",
            userName,
            null,
            null,
            It.Is<string>(info => info.Contains("30 días de gracia"))
        ), Times.Once);
    }

    [Fact]
    public async Task LogTwoFactorEnabledAsync_SinGracePeriod_RegistraSinMencion()
    {
        // Arrange
        var userName = "user@test.com";

        // Act
        await _service.LogTwoFactorEnabledAsync(userName, gracePeriodDays: null);

        // Assert
        _mockAuditService.Verify(x => x.LogAsync(
            "2FA",
            userName,
            "2FA_ENABLED",
            userName,
            null,
            null,
            It.Is<string>(info => !info.Contains("gracia"))
        ), Times.Once);
    }

    [Fact]
    public async Task LogTwoFactorDisabledAsync_RegistraEvento()
    {
        // Arrange
        var userName = "tesorero@test.com";

        // Act
        await _service.LogTwoFactorDisabledAsync(userName);

        // Assert
        _mockAuditService.Verify(x => x.LogAsync(
            "2FA",
            userName,
            "2FA_DISABLED",
            userName,
            null,
            null,
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async Task LogRecoveryCodesGeneratedAsync_RegistraEvento()
    {
        // Arrange
        var userName = "admin@test.com";

        // Act
        await _service.LogRecoveryCodesGeneratedAsync(userName);

        // Assert
        _mockAuditService.Verify(x => x.LogAsync(
            "2FA",
            userName,
            "2FA_RECOVERY_CODES_GENERATED",
            userName,
            null,
            null,
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async Task LogAuthenticatorResetAsync_RegistraEvento()
    {
        // Arrange
        var userName = "user@test.com";

        // Act
        await _service.LogAuthenticatorResetAsync(userName);

        // Assert
        _mockAuditService.Verify(x => x.LogAsync(
            "2FA",
            userName,
            "2FA_AUTHENTICATOR_RESET",
            userName,
            null,
            null,
            It.IsAny<string>()
        ), Times.Once);
    }

    [Theory]
    [InlineData("admin@lama.com")]
    [InlineData("tesorero@lama.com")]
    [InlineData("user.test@example.com")]
    public async Task LogTwoFactorEnabledAsync_ConDiferentesUsuarios_RegistraCorrectamente(string userName)
    {
        // Arrange & Act
        await _service.LogTwoFactorEnabledAsync(userName, 30);

        // Assert
        _mockAuditService.Verify(x => x.LogAsync(
            "2FA",
            userName,
            "2FA_ENABLED",
            userName,
            null,
            null,
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async Task LogTwoFactorEnabledAsync_ConGracePeriodCero_RegistraSinGracePeriod()
    {
        // Arrange
        var userName = "test@test.com";

        // Act
        await _service.LogTwoFactorEnabledAsync(userName, gracePeriodDays: 0);

        // Assert
        _mockAuditService.Verify(x => x.LogAsync(
            "2FA",
            userName,
            "2FA_ENABLED",
            userName,
            null,
            null,
            It.Is<string>(info => info == "2FA habilitado exitosamente")
        ), Times.Once);
    }

    [Fact]
    public async Task MultiplesLlamadas_RegistraTodosLosEventos()
    {
        // Arrange
        var userName = "admin@test.com";

        // Act
        await _service.LogTwoFactorEnabledAsync(userName, 30);
        await _service.LogRecoveryCodesGeneratedAsync(userName);
        await _service.LogTwoFactorDisabledAsync(userName);

        // Assert
        _mockAuditService.Verify(x => x.LogAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Exactly(3));
    }
}
