using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Azure.Storage.Blobs;
using Server.Configuration;

namespace Server.Services.Backup;

/// <summary>
/// Servicio para realizar backups automáticos programados de la base de datos.
/// Soporta tanto Azure Blob Storage como almacenamiento local.
/// </summary>
public class BackupHostedService : IHostedService, IDisposable
{
    private readonly ILogger<BackupHostedService> _logger;
    private readonly BackupOptions _options;
    private Timer? _timer;
    private readonly IServiceScopeFactory _scopeFactory;

    public BackupHostedService(
        ILogger<BackupHostedService> logger,
        IOptions<BackupOptions> options,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _options = options.Value;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Backup automático deshabilitado en configuración");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Backup automático iniciado. Programación: {Schedule}", _options.CronSchedule);

        // Por simplicidad, ejecutar cada 24 horas (en producción usar una librería de cron como Cronos o Quartz)
        _timer = new Timer(DoBackup, null, TimeSpan.FromHours(1), TimeSpan.FromHours(24));

        return Task.CompletedTask;
    }

    private async void DoBackup(object? state)
    {
        try
        {
            _logger.LogInformation("Iniciando backup automático...");

            using var scope = _scopeFactory.CreateScope();
            var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();

            var fileName = await backupService.CreateBackupAsync();
            _logger.LogInformation("Backup creado exitosamente: {FileName}", fileName);

            // Limpiar backups antiguos (del almacenamiento activo)
            await backupService.CleanOldBackupsAsync(_options.RetentionDays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar backup automático");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Backup automático detenido");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

/// <summary>
/// Servicio para crear y gestionar backups de la base de datos.
/// Soporta tanto Azure Blob Storage como almacenamiento local.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Crea un backup de la base de datos y lo almacena en Azure Blob o localmente.
    /// </summary>
    Task<string> CreateBackupAsync();

    /// <summary>
    /// Limpia backups antiguo según la política de retención (en días).
    /// </summary>
    Task CleanOldBackupsAsync(int retentionDays);

    /// <summary>
    /// Obtiene la lista de backups disponibles.
    /// </summary>
    Task<List<string>> GetAvailableBackupsAsync();
}

/// <summary>
/// Implementación del servicio de backup con soporte para Azure Blob Storage.
/// Fallback a almacenamiento local si Azure no está disponible.
/// </summary>
public class BackupService : IBackupService
{
    private readonly BackupOptions _backupOptions;
    private readonly AzureOptions _azureOptions;
    private readonly ILogger<BackupService> _logger;
    private readonly string _connectionString;
    private readonly BlobContainerClient? _blobContainerClient;
    private readonly bool _useAzureBlob;

    public BackupService(
        IOptions<BackupOptions> backupOptions,
        IOptions<AzureOptions> azureOptions,
        ILogger<BackupService> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        BlobServiceClient? blobServiceClient = null)
    {
        _backupOptions = backupOptions.Value;
        _azureOptions = azureOptions.Value;
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Cadena de conexión no encontrada");

        // Intentar inicializar Blob Storage si está habilitado y disponible
        _useAzureBlob = _azureOptions.UseAzureBlobBackup && blobServiceClient != null;
        
        if (_useAzureBlob)
        {
            try
            {
                _blobContainerClient = blobServiceClient.GetBlobContainerClient(_azureOptions.BackupContainerName);
                _logger.LogInformation("Servicio de backup configurado para usar Azure Blob Storage: {Container}", 
                    _azureOptions.BackupContainerName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo inicializar Azure Blob Storage");
                
                // En Producción, exigir que Azure Blob esté disponible (sin fallback local)
                if (environment.IsProduction())
                {
                    _logger.LogError("CRÍTICO: En Producción, Azure Blob Storage es OBLIGATORIO y no está disponible. El servicio de backup se deshabilitará.");
                    throw new InvalidOperationException(
                        "Azure Blob Storage es OBLIGATORIO en Producción. Verifique la configuración y la conectividad a Azure Storage Account.",
                        ex);
                }
                
                // En Development/Test: permitir fallback a almacenamiento local
                _useAzureBlob = false;
                _logger.LogInformation("Usando almacenamiento local como fallback (solo en ambientes no-producción)");
            }
        }
        else if (_azureOptions.UseAzureBlobBackup && environment.IsProduction())
        {
            // En Producción: si UseAzureBlobBackup está habilitado pero blobServiceClient es null, error crítico
            _logger.LogError("CRÍTICO: UseAzureBlobBackup=true en Producción pero BlobServiceClient no está registrado. Revise Program.cs");
            throw new InvalidOperationException(
                "Configuración de Producción inválida: UseAzureBlobBackup=true pero Azure Storage no está disponible.");
        }
    }

    public async Task<string> CreateBackupAsync()
    {
        // Crear backup en archivo temporal
        var tempBackupPath = Path.Combine(Path.GetTempPath(), $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
        var database = _backupOptions.Database ?? "LamaMedellin";
        var fileName = Path.GetFileName(tempBackupPath);

        // Ejecutar comando T-SQL BACKUP DATABASE
        var sql = $"BACKUP DATABASE [{database}] TO DISK = @backupPath WITH FORMAT, COMPRESSION;";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 600; // 10 minutos
            command.Parameters.AddWithValue("@backupPath", tempBackupPath);

            await command.ExecuteNonQueryAsync();
            _logger.LogInformation("Backup de base de datos creado: {Database}", database);

            // Subir a Azure Blob si está disponible
            if (_useAzureBlob && _blobContainerClient != null)
            {
                await UploadToAzureBlobAsync(tempBackupPath, fileName);
            }
            else
            {
                // Fallback: copiar a almacenamiento local
                await SaveToLocalStorageAsync(tempBackupPath, fileName);
            }

            return fileName;
        }
        finally
        {
            // Eliminar archivo temporal
            if (File.Exists(tempBackupPath))
            {
                try
                {
                    File.Delete(tempBackupPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo eliminar archivo temporal de backup: {Path}", tempBackupPath);
                }
            }
        }
    }

    /// <summary>
    /// Sube el archivo de backup a Azure Blob Storage.
    /// </summary>
    private async Task UploadToAzureBlobAsync(string localPath, string blobName)
    {
        try
        {
            if (_blobContainerClient == null)
                return;

            using var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            
            await blobClient.UploadAsync(fileStream, overwrite: true);
            _logger.LogInformation("Backup subido a Azure Blob Storage: {BlobName}", blobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir backup a Azure Blob Storage, usando almacenamiento local");
            // Fallback: guardar localmente en caso de error
            await SaveToLocalStorageAsync(localPath, blobName);
        }
    }

    /// <summary>
    /// Guarda el backup en almacenamiento local.
    /// Se usa como fallback si Azure Blob no está disponible.
    /// </summary>
    private async Task SaveToLocalStorageAsync(string sourcePath, string fileName)
    {
        try
        {
            Directory.CreateDirectory(_backupOptions.BackupPath);
            var destinationPath = Path.Combine(_backupOptions.BackupPath, fileName);
            
            await Task.Run(() => File.Copy(sourcePath, destinationPath, overwrite: true));
            _logger.LogInformation("Backup guardado en almacenamiento local: {Path}", destinationPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar backup en almacenamiento local");
            throw;
        }
    }

    public async Task CleanOldBackupsAsync(int retentionDays)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-retentionDays);

            if (_useAzureBlob && _blobContainerClient != null)
            {
                await CleanAzureBlobsAsync(cutoffDate);
            }

            // También limpiar local por si hay backups antiguos
            CleanLocalBackups(cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar backups antiguos");
        }
    }

    /// <summary>
    /// Elimina blobs antiguos de Azure Blob Storage según la fecha de retención.
    /// </summary>
    private async Task CleanAzureBlobsAsync(DateTime cutoffDate)
    {
        try
        {
            if (_blobContainerClient == null)
                return;

            var blobsToDelete = new List<string>();

            // Listar todos los blobs y filtrar por antigüedad
            await foreach (var blobItem in _blobContainerClient.GetBlobsAsync())
            {
                if (blobItem.Properties.CreatedOn.HasValue && 
                    blobItem.Properties.CreatedOn.Value.DateTime < cutoffDate)
                {
                    blobsToDelete.Add(blobItem.Name);
                }
            }

            // Eliminar blobs antiguos
            foreach (var blobName in blobsToDelete)
            {
                try
                {
                    var blobClient = _blobContainerClient.GetBlobClient(blobName);
                    await blobClient.DeleteAsync();
                    _logger.LogInformation("Backup antiguo eliminado de Azure Blob Storage: {BlobName}", blobName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo eliminar blob antiguo: {BlobName}", blobName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar backups en Azure Blob Storage");
        }
    }

    /// <summary>
    /// Elimina archivos de backup antiguos del almacenamiento local.
    /// </summary>
    private void CleanLocalBackups(DateTime cutoffDate)
    {
        try
        {
            if (!Directory.Exists(_backupOptions.BackupPath))
                return;

            var files = Directory.GetFiles(_backupOptions.BackupPath, "Backup_*.bak");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    try
                    {
                        File.Delete(file);
                        _logger.LogInformation("Backup antiguo eliminado del almacenamiento local: {File}", file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudo eliminar backup local: {File}", file);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar backups locales");
        }
    }

    public async Task<List<string>> GetAvailableBackupsAsync()
    {
        var backups = new List<string>();

        try
        {
            if (_useAzureBlob && _blobContainerClient != null)
            {
                // Listar de Azure Blob Storage
                await foreach (var blobItem in _blobContainerClient.GetBlobsAsync())
                {
                    backups.Add(blobItem.Name);
                }
            }
            else if (Directory.Exists(_backupOptions.BackupPath))
            {
                // Listar del almacenamiento local
                var files = Directory.GetFiles(_backupOptions.BackupPath, "Backup_*.bak")
                    .Select(Path.GetFileName)
                    .OrderByDescending(f => f)
                    .ToList();
                
                backups.AddRange(files ?? new List<string>());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar backups disponibles");
        }

        return backups;
    }
}
