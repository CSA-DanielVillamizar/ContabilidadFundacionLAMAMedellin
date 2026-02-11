namespace Server.Constants;

/// <summary>
/// Constantes centralizadas para la aplicación LAMA Medellín.
/// Proporciona valores únicos para roles, políticas, acciones de auditoría y configuraciones.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Roles de autorización del sistema
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Tesorero = "Tesorero";
        public const string Junta = "Junta";
        public const string Consulta = "Consulta";
        public const string Gerente = "Gerente";
        public const string GerenteNegocios = "GerenteNegocios";
        public const string Miembro = "Miembro";
    }

    /// <summary>
    /// Políticas de autorización configuradas en Program.cs
    /// </summary>
    public static class Policies
    {
        public const string AdminTesorero = "AdminTesorero";
        public const string TesoreroJunta = "TesoreroJunta";
        public const string GerenciaNegocios = "GerenciaNegocios";
        public const string AdminOrTesoreroWith2FA = "AdminOrTesoreroWith2FA";
        public const string AdminOrTesorero = "AdminOrTesorero";
        public const string RequireTesorero = "RequireTesorero";
        public const string RequireAdmin = "RequireAdmin";
        public const string RequireJunta = "RequireJunta";
    }

    /// <summary>
    /// Acciones de auditoría registradas en la base de datos
    /// </summary>
    public static class AuditActions
    {
        public const string ReciboCreado = "ReciboCreado";
        public const string ReciboModificado = "ReciboModificado";
        public const string ReciboEliminado = "ReciboEliminado";
        public const string EgresoCreado = "EgresoCreado";
        public const string EgresoModificado = "EgresoModificado";
        public const string EgresoEliminado = "EgresoEliminado";
        public const string TwoFactorEnabled = "TwoFactorEnabled";
        public const string LoginExitoso = "LoginExitoso";
        public const string LoginFallido = "LoginFallido";
        public const string CierreContableRealizado = "CierreContableRealizado";
        public const string CertificadoGenerado = "CertificadoGenerado";
        public const string ConciliacionRealizada = "ConciliacionRealizada";
        public const string DatosImportados = "DatosImportados";
        public const string ConfiguracionActualizada = "ConfiguracionActualizada";
        public const string UsuarioCreado = "UsuarioCreado";
    }

    /// <summary>
    /// Configuración de paginación para grillas y listados
    /// </summary>
    public static class Pagination
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
    }

    /// <summary>
    /// Configuración para carga de archivos
    /// </summary>
    public static class FileUpload
    {
        /// <summary>
        /// Tamaño máximo permitido para archivos: 20 MB
        /// </summary>
        public const long MaxSizeBytes = 20 * 1024 * 1024;

        /// <summary>
        /// Extensiones permitidas para imágenes
        /// </summary>
        public const string AllowedImageExtensions = ".jpg,.jpeg,.png,.gif,.webp";

        /// <summary>
        /// Extensiones permitidas para archivos Excel y CSV
        /// Incluye CSV para compatibilidad con exportaciones de Bancolombia
        /// </summary>
        public const string AllowedExcelExtensions = ".xlsx,.xls,.csv";
    }
}
