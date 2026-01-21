namespace Server.Models;

/// <summary>
/// Catálogo de fuentes de ingreso de la fundación (aportes, ventas, donaciones, etc.).
/// </summary>
public class FuenteIngreso
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty; // ej: APORTE-MEN, VENTA-MERCH
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activa { get; set; } = true;

    // Contabilidad futura (Fase 2)
    public Guid? CuentaContableId { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "seed";
}
