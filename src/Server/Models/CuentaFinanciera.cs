namespace Server.Models;

/// <summary>
/// Cuenta financiera (bancaria o caja) donde se registran movimientos reales de tesorería.
/// Incluye auditoría básica y control de activación.
/// </summary>
public enum TipoCuenta { Bancaria = 1, Caja = 2 }

public class CuentaFinanciera
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty; // ej: BANCO-BCOL-001
    public string Nombre { get; set; } = string.Empty; // ej: "Bancolombia Cuenta Corriente Principal"
    public TipoCuenta Tipo { get; set; } = TipoCuenta.Bancaria;
    public string? Banco { get; set; } // "Bancolombia"
    public string? NumeroCuenta { get; set; } // Enmascarado: "****5678"
    public string? TitularCuenta { get; set; }
    public decimal SaldoInicial { get; set; }
    public decimal SaldoActual { get; set; }
    public DateTime FechaApertura { get; set; } = DateTime.UtcNow;
    public bool Activa { get; set; } = true;
    public string? Observaciones { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "seed";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
