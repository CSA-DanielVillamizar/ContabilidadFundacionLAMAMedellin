namespace Server.Configuration;

/// <summary>
/// Opciones de configuración para integración con Azure.
/// Se cargan desde appsettings o Azure Key Vault en producción.
/// </summary>
public class AzureOptions
{
    /// <summary>
    /// Endpoint (URL) del Azure Key Vault (ej: https://{vaultName}.vault.azure.net/).
    /// Se usa para cargar secretos en entorno de producción.
    /// Soporta tanto Azure:KeyVaultEndpoint como Azure__KeyVaultEndpoint (env vars).
    /// </summary>
    public string? KeyVaultEndpoint { get; set; }

    /// <summary>
    /// Nombre del contenedor de Azure Blob Storage para almacenar backups (sql-backups).
    /// </summary>
    public string BackupContainerName { get; set; } = "sql-backups";

    /// <summary>
    /// URI del servicio Blob de Azure Storage Account.
    /// Formato: https://{storageAccountName}.blob.core.windows.net/
    /// Se usa con Managed Identity (DefaultAzureCredential) en producción.
    /// </summary>
    public string? StorageBlobServiceUri { get; set; }

    /// <summary>
    /// Nombre de la Storage Account de Azure (alternativa a StorageBlobServiceUri).
    /// Si se especifica, se construirá el URI automáticamente.
    /// Ej: "lamaprodstorage2025"
    /// </summary>
    public string? StorageAccountName { get; set; }

    /// <summary>
    /// Clave de instrumentación para Application Insights (se lee de Key Vault en producción).
    /// Formato: InstrumentationKey=XXXX-XXXX-XXXX
    /// </summary>
    public string? ApplicationInsightsInstrumentationKey { get; set; }

    /// <summary>
    /// Indica si se debe usar Azure Key Vault para cargar configuración en producción.
    /// </summary>
    public bool EnableKeyVault { get; set; } = true;

    /// <summary>
    /// Indica si se debe usar Azure Blob Storage para backups.
    /// </summary>
    public bool UseAzureBlobBackup { get; set; } = true;
}
