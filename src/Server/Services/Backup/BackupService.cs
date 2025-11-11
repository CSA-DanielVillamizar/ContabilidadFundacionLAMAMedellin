using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Server.Configuration;

namespace Server.Services.Backup;

/// <summary>
/// Servicio para realizar backups automáticos programados de la base de datos.
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

            // Limpiar backups antiguos
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
/// </summary>
public interface IBackupService
{
    Task<string> CreateBackupAsync();
    Task CleanOldBackupsAsync(int retentionDays);
    Task<List<string>> GetAvailableBackupsAsync();
}

public class BackupService : IBackupService
{
    private readonly BackupOptions _options;
    private readonly ILogger<BackupService> _logger;
    private readonly string _connectionString;

    public BackupService(
        IOptions<BackupOptions> options,
        ILogger<BackupService> logger,
        IConfiguration configuration)
    {
        _options = options.Value;
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<string> CreateBackupAsync()
    {
        // Asegurar que existe el directorio de backups
        Directory.CreateDirectory(_options.BackupPath);

        // Nombre del archivo de backup con timestamp
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var database = _options.Database ?? "LamaMedellin";
        var fileName = $"Backup_{database}_{timestamp}.bak";
        var fullPath = Path.Combine(_options.BackupPath, fileName);

        // Crear backup usando T-SQL
        var sql = $"BACKUP DATABASE [{database}] TO DISK = @backupPath WITH FORMAT, COMPRESSION;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        command.CommandTimeout = 600; // 10 minutos
        command.Parameters.AddWithValue("@backupPath", fullPath);

        await command.ExecuteNonQueryAsync();

        _logger.LogInformation("Backup creado en: {Path}", fullPath);
        return fileName;
    }

    public Task CleanOldBackupsAsync(int retentionDays)
    {
        if (!Directory.Exists(_options.BackupPath))
            return Task.CompletedTask;

        var cutoffDate = DateTime.Now.AddDays(-retentionDays);
        var files = Directory.GetFiles(_options.BackupPath, "Backup_*.bak");

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            if (fileInfo.CreationTime < cutoffDate)
            {
                try
                {
                    File.Delete(file);
                    _logger.LogInformation("Backup antiguo eliminado: {File}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo eliminar el backup: {File}", file);
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<List<string>> GetAvailableBackupsAsync()
    {
        if (!Directory.Exists(_options.BackupPath))
            return Task.FromResult(new List<string>());

        var files = Directory.GetFiles(_options.BackupPath, "Backup_*.bak")
            .Select(f => Path.GetFileName(f))
            .OrderByDescending(f => f)
            .ToList();

        return Task.FromResult(files);
    }
}
