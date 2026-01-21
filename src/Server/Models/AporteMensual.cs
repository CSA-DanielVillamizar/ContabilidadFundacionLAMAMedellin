namespace Server.Models;

/// <summary>
/// Aporte mensual por miembro y período (año/mes). Se marca como Pagado al registrar el ingreso correspondiente.
/// </summary>
public enum EstadoAporte { Pendiente = 0, Pagado = 1, Exonerado = 2 }

public class AporteMensual
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MiembroId { get; set; }
    public Miembro? Miembro { get; set; }

    public int Ano { get; set; }
    public int Mes { get; set; }

    public decimal ValorEsperado { get; set; } = 20000m;
    public EstadoAporte Estado { get; set; } = EstadoAporte.Pendiente;

    public DateTime? FechaPago { get; set; }
    public Guid? MovimientoTesoreriaId { get; set; }
    public MovimientoTesoreria? MovimientoTesoreria { get; set; }

    public string? Observaciones { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
