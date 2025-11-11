namespace Server.Configuration;

/// <summary>
/// Configuración de la entidad con Régimen Tributario Especial (RTE).
/// Esta información se utiliza en los certificados de donación y otros documentos oficiales.
/// </summary>
public class EntidadRTEOptions
{
    /// <summary>
    /// Número de Identificación Tributaria (NIT) de la entidad.
    /// </summary>
    public string NIT { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo oficial de la entidad.
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Nombre corto o razón social abreviada.
    /// </summary>
    public string NombreCorto { get; set; } = string.Empty;

    /// <summary>
    /// Ciudad donde está domiciliada la entidad.
    /// </summary>
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>
    /// Departamento donde está domiciliada la entidad.
    /// </summary>
    public string Departamento { get; set; } = string.Empty;

    /// <summary>
    /// Dirección física de la entidad.
    /// </summary>
    public string Direccion { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono de contacto principal.
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico de contacto.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Sitio web de la entidad.
    /// </summary>
    public string WebSite { get; set; } = string.Empty;

    /// <summary>
    /// Indica si la entidad está acogida al Régimen Tributario Especial (RTE).
    /// </summary>
    public bool EsRTE { get; set; }

    /// <summary>
    /// Número de la resolución de la DIAN que otorga el RTE.
    /// </summary>
    public string NumeroResolucionRTE { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de expedición de la resolución RTE (formato ISO: YYYY-MM-DD).
    /// </summary>
    public string FechaResolucionRTE { get; set; } = string.Empty;

    /// <summary>
    /// Datos del representante legal de la entidad.
    /// </summary>
    public RepresentanteLegalOptions RepresentanteLegal { get; set; } = new();

    /// <summary>
    /// Datos del contador público certificado.
    /// </summary>
    public ContadorPublicoOptions ContadorPublico { get; set; } = new();

    /// <summary>
    /// Datos del revisor fiscal (si aplica).
    /// </summary>
    public RevisorFiscalOptions RevisorFiscal { get; set; } = new();
}

/// <summary>
/// Información del representante legal de la entidad.
/// </summary>
public class RepresentanteLegalOptions
{
    /// <summary>
    /// Nombre completo del representante legal.
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de identificación (CC, CE, NIT, etc.).
    /// </summary>
    public string TipoIdentificacion { get; set; } = string.Empty;

    /// <summary>
    /// Número de identificación.
    /// </summary>
    public string NumeroIdentificacion { get; set; } = string.Empty;

    /// <summary>
    /// Cargo que desempeña (ej: "Representante Legal", "Director Ejecutivo").
    /// </summary>
    public string Cargo { get; set; } = string.Empty;
}

/// <summary>
/// Información del contador público de la entidad.
/// </summary>
public class ContadorPublicoOptions
{
    /// <summary>
    /// Nombre completo del contador público.
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Número de tarjeta profesional del contador.
    /// </summary>
    public string TarjetaProfesional { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono de contacto del contador.
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del contador.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Información del revisor fiscal de la entidad (opcional).
/// </summary>
public class RevisorFiscalOptions
{
    /// <summary>
    /// Nombre completo del revisor fiscal.
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Número de tarjeta profesional del revisor fiscal.
    /// </summary>
    public string TarjetaProfesional { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono de contacto del revisor fiscal.
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del revisor fiscal.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
