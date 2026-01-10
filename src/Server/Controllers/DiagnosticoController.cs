using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Configuration;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Server.Controllers;

/// <summary>
/// Controlador de diagnóstico para administradores.
/// Expone estado de configuración en producción SIN mostrar valores sensibles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class DiagnosticoController : ControllerBase
{
    private readonly ILogger<DiagnosticoController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOptions<AzureOptions> _azureOptions;
    private readonly IOptions<BackupOptions> _backupOptions;
    private readonly IWebHostEnvironment _environment;

    public DiagnosticoController(
        ILogger<DiagnosticoController> logger,
        IConfiguration configuration,
        IOptions<AzureOptions> azureOptions,
        IOptions<BackupOptions> backupOptions,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _configuration = configuration;
        _azureOptions = azureOptions;
        _backupOptions = backupOptions;
        _environment = environment;
    }

    /// <summary>
    /// Obtiene estado de configuración de producción (sin valores sensibles).
    /// Solo accesible para rol Admin.
    /// </summary>
    [HttpGet]
    public IActionResult GetDiagnostico()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "unknown";
            
            // Determinar tipo de autenticación SQL
            var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
            var sqlAuthType = connectionString.Contains("Authentication=Active Directory Default", StringComparison.OrdinalIgnoreCase)
                ? "ManagedIdentity"
                : connectionString.Contains("Trusted_Connection", StringComparison.OrdinalIgnoreCase)
                    ? "IntegratedSecurity"
                    : "SqlAuthentication";

            // Estado de configuración de almacenamiento/backup sin exponer secretos
            // Ahora valida URI o AccountName en lugar de connection string
            var hasStorageUri = !string.IsNullOrWhiteSpace(_azureOptions.Value?.StorageBlobServiceUri) 
                || !string.IsNullOrWhiteSpace(_azureOptions.Value?.StorageAccountName);
            var storageConfigured = (_azureOptions.Value?.UseAzureBlobBackup ?? false)
                && hasStorageUri
                && !string.IsNullOrWhiteSpace(_azureOptions.Value?.BackupContainerName);
            var backupReady = storageConfigured && (_backupOptions.Value?.Enabled ?? false);

            var diagnostico = new
            {
                timestamp = DateTime.UtcNow,
                environment = _environment.EnvironmentName,
                version,
                assemblyVersion = assembly.GetName().Version,
                
                // Configuración Azure
                azure = new
                {
                    keyVaultEnabled = _azureOptions.Value?.EnableKeyVault ?? false,
                    keyVaultConfigured = !string.IsNullOrEmpty(_azureOptions.Value?.KeyVaultEndpoint),
                    blobStorageEnabled = _azureOptions.Value?.UseAzureBlobBackup ?? false,
                    blobStorageConfigured = hasStorageUri,
                    blobStorageAuthMethod = "ManagedIdentity", // Ahora usa Managed Identity en lugar de connection string
                    backupContainerName = _azureOptions.Value?.BackupContainerName ?? "not-configured",
                    storageConfigured,
                    backupReady,
                    appInsightsConfigured = !string.IsNullOrEmpty(_configuration["ApplicationInsights:ConnectionString"])
                },
                
                // Configuración SQL
                database = new
                {
                    authenticationType = sqlAuthType,
                    connectionStringSet = !string.IsNullOrEmpty(connectionString)
                },
                
                // Configuración Backup
                backup = new
                {
                    enabled = _backupOptions.Value?.Enabled ?? false,
                    retentionDays = _backupOptions.Value?.RetentionDays ?? 30,
                    schedule = _backupOptions.Value?.CronSchedule ?? "not-configured"
                },
                
                // Health checks status
                healthChecks = new
                {
                    endpoint = "/health",
                    ready = "/health/ready",
                    live = "/health/live"
                }
            };

            _logger.LogInformation("Diagnóstico solicitado por usuario admin desde {IpAddress}", 
                HttpContext.Connection.RemoteIpAddress);

            return Ok(diagnostico);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar diagnóstico");
            return StatusCode(500, new { error = "Error interno al generar diagnóstico", message = ex.Message });
        }
    }

    /// <summary>
    /// Verifica la conectividad con Azure Key Vault (solo en producción).
    /// </summary>
    [HttpGet("keyvault")]
    public IActionResult CheckKeyVault()
    {
        if (!_environment.IsProduction())
        {
            return BadRequest("Solo disponible en producción");
        }

        try
        {
            var keyVaultUrl = _azureOptions.Value?.KeyVaultEndpoint;
            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                return Ok(new { connected = false, reason = "KeyVault no configurado" });
            }

            return Ok(new 
            { 
                connected = true, 
                url = keyVaultUrl,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar Key Vault");
            return StatusCode(500, new { connected = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Retorna metadatos de ensamblado y versión.
    /// Solo disponible en Development o para rol Admin.
    /// </summary>
    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        if (!_environment.IsDevelopment())
        {
            // En producción, requiere autorización de Admin
            var user = User;
            if (!user.IsInRole("Admin"))
            {
                return Forbid();
            }
        }

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var fileVersion = assembly.GetCustomAttribute<System.Reflection.AssemblyFileVersionAttribute>()?.Version ?? "unknown";
            var infoVersion = assembly.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";

            return Ok(new
            {
                version = version?.ToString(),
                fileVersion,
                infoVersion,
                framework = AppDomain.CurrentDomain.GetData("RUNTIME_IDENTIFIER")?.ToString() ?? ".NET 8.0",
                buildDate = new FileInfo(assembly.Location).LastWriteTimeUtc
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener versión");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
