namespace Server.Models;

/// <summary>
/// Catálogo de categorías de egreso de la fundación (gasto social, compras, administrativos, etc.).
/// </summary>
public class CategoriaEgreso
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EsGastoSocial { get; set; } = false;
    public bool Activa { get; set; } = true;

    // Contabilidad futura (Fase 2)
    public Guid? CuentaContableId { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "seed";
}
