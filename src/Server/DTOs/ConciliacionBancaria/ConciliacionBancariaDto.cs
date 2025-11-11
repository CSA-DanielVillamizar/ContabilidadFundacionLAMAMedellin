namespace Server.DTOs.ConciliacionBancaria;

/// <summary>
/// DTO para visualización de conciliaciones en listados
/// </summary>
public class ConciliacionBancariaDto
{
    public Guid Id { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string Periodo { get; set; } = string.Empty; // "Enero 2025"
    public DateTime FechaConciliacion { get; set; }
    public decimal SaldoLibros { get; set; }
    public decimal SaldoBanco { get; set; }
    public decimal Diferencia { get; set; }
    public string Estado { get; set; } = string.Empty; // Pendiente, EnProceso, Conciliado, ConDiferencias
    public int TotalItems { get; set; }
    public int ItemsConciliados { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para crear/editar conciliación bancaria
/// </summary>
public class ConciliacionBancariaFormDto
{
    public Guid? Id { get; set; }
    public int Ano { get; set; } = DateTime.Now.Year;
    public int Mes { get; set; } = DateTime.Now.Month;
    public DateTime FechaConciliacion { get; set; } = DateTime.Now;
    public decimal SaldoLibros { get; set; }
    public decimal SaldoBanco { get; set; }
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = "Pendiente";
    
    // Items
    public List<ItemConciliacionFormDto> Items { get; set; } = new();
}

/// <summary>
/// DTO para detalle completo de conciliación
/// </summary>
public class ConciliacionBancariaDetalleDto
{
    public Guid Id { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public DateTime FechaConciliacion { get; set; }
    
    // Saldos
    public decimal SaldoLibros { get; set; }
    public decimal SaldoBanco { get; set; }
    public decimal Diferencia { get; set; }
    
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = string.Empty;
    
    // Items
    public List<ItemConciliacionDto> Items { get; set; } = new();
    
    // Estadísticas
    public int TotalItems { get; set; }
    public int ItemsConciliados { get; set; }
    public int ItemsPendientes { get; set; }
    public decimal TotalNotasDebito { get; set; }
    public decimal TotalNotasCredito { get; set; }
    public decimal TotalChequesTransito { get; set; }
    public decimal TotalDepositosTransito { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO para item de conciliación
/// </summary>
public class ItemConciliacionDto
{
    public Guid Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty; // NotaDebito, NotaCredito, ChequeTransito, DepositoTransito, Ajuste
    public string Concepto { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public decimal Monto { get; set; }
    public bool Conciliado { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para crear/editar item de conciliación
/// </summary>
public class ItemConciliacionFormDto
{
    public Guid? Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string Tipo { get; set; } = string.Empty;
    public string Concepto { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public decimal Monto { get; set; }
    public bool Conciliado { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para importar extracto bancario
/// </summary>
public class ImportarExtractoDto
{
    public Guid ConciliacionId { get; set; }
    public string FormatoArchivo { get; set; } = "CSV"; // CSV, Excel, PDF
    public byte[] ArchivoBytes { get; set; } = Array.Empty<byte>();
    public string NombreArchivo { get; set; } = string.Empty;
    public bool CrearItemsAutomaticamente { get; set; } = true;
    public bool IntentarMatchingAutomatico { get; set; } = true;
}

/// <summary>
/// DTO para resumen de conciliación
/// </summary>
public class ResumenConciliacionDto
{
    public int Mes { get; set; }
    public string NombreMes { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal SaldoLibros { get; set; }
    public decimal SaldoBanco { get; set; }
    public decimal Diferencia { get; set; }
    public int ItemsConciliados { get; set; }
    public int ItemsPendientes { get; set; }
}
