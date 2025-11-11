
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Server.Models;

namespace Server.DTOs.Recibos
{
    /// <summary>
    /// DTO para mostrar conceptos disponibles en recibos.
    /// </summary>
    public class ConceptoListItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Codigo { get; set; } = "";
    }

    /// <summary>
    /// DTO para crear un nuevo recibo.
    /// </summary>
    public class CreateReciboDto
    {
        [Required(ErrorMessage = "La serie es requerida")]
        [StringLength(10, ErrorMessage = "La serie no puede exceder 10 caracteres")]
        public string Serie { get; set; } = "LM";

        [Required(ErrorMessage = "El año es requerido")]
        [Range(2000, 2100, ErrorMessage = "El año debe estar entre 2000 y 2100")]
        public int Ano { get; set; } = DateTime.UtcNow.Year;

        [Required(ErrorMessage = "La fecha de emisión es requerida")]
        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

        public Guid? MiembroId { get; set; }

        [StringLength(200, ErrorMessage = "El tercero libre no puede exceder 200 caracteres")]
        public string? TerceroLibre { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "Debe agregar al menos un item")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un item")]
        public List<CreateReciboItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO para un item de recibo.
    /// </summary>
    public class CreateReciboItemDto
    {
        [Required(ErrorMessage = "El concepto es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un concepto válido")]
        public int ConceptoId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, 999, ErrorMessage = "La cantidad debe estar entre 1 y 999")]
        public int Cantidad { get; set; } = 1;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioUnitarioMonedaOrigen { get; set; }

        [Required(ErrorMessage = "La moneda es requerida")]
        public Moneda MonedaOrigen { get; set; } = Moneda.COP;

        /// <summary>
        /// TRM aplicada (solo si MonedaOrigen == USD).
        /// Se calcula automáticamente desde la tabla TasasCambio.
        /// </summary>
        public decimal? TrmAplicada { get; set; }

        [StringLength(200, ErrorMessage = "Las notas no pueden exceder 200 caracteres")]
        public string? Notas { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un recibo existente.
    /// Solo se pueden editar recibos en estado Borrador.
    /// </summary>
    public class UpdateReciboDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "La serie es requerida")]
        [StringLength(10, ErrorMessage = "La serie no puede exceder 10 caracteres")]
        public string Serie { get; set; } = null!;

        [Required(ErrorMessage = "El año es requerido")]
        [Range(2000, 2100, ErrorMessage = "El año debe estar entre 2000 y 2100")]
        public int Ano { get; set; }

        [Required(ErrorMessage = "La fecha de emisión es requerida")]
        public DateTime FechaEmision { get; set; }

        public Guid? MiembroId { get; set; }

        [StringLength(200, ErrorMessage = "El tercero libre no puede exceder 200 caracteres")]
        public string? TerceroLibre { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "Debe agregar al menos un item")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un item")]
        public List<CreateReciboItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO con los detalles completos de un recibo.
    /// </summary>
    public class ReciboDetailDto
    {
        public Guid Id { get; set; }
        public string Serie { get; set; } = null!;
        public int Ano { get; set; }
        public int Consecutivo { get; set; }
        public string NumeroCompleto => $"{Serie}-{Ano}-{Consecutivo:D5}";
    
        public DateTime FechaEmision { get; set; }
        public EstadoRecibo Estado { get; set; }

        public Guid? MiembroId { get; set; }
        public string? MiembroNombre { get; set; }
        public string? TerceroLibre { get; set; }
    
        /// <summary>
        /// Retorna el nombre del pagador (Miembro o Tercero Libre).
        /// </summary>
        public string NombrePagador => MiembroNombre ?? TerceroLibre ?? "Sin especificar";

        public decimal TotalCop { get; set; }
        public string? Observaciones { get; set; }

        public List<ReciboItemDetailDto> Items { get; set; } = new();

        // Auditoría
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
    }

    /// <summary>
    /// DTO para un item de recibo con detalles completos.
    /// </summary>
    public class ReciboItemDetailDto
    {
        public int Id { get; set; }
        public int ConceptoId { get; set; }
        public string ConceptoNombre { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal PrecioUnitarioMonedaOrigen { get; set; }
        public Moneda MonedaOrigen { get; set; }
        public decimal? TrmAplicada { get; set; }
        public decimal SubtotalCop { get; set; }
        public string? Notas { get; set; }
    }

    /// <summary>
    /// DTO para lista de recibos (vista general).
    /// </summary>
    public class ReciboListItem
    {
        public Guid Id { get; set; }
        public string NumeroCompleto { get; set; } = null!;
        public DateTime FechaEmision { get; set; }
        public EstadoRecibo Estado { get; set; }
        public string NombrePagador { get; set; } = null!;
        public decimal TotalCop { get; set; }
    }

    /// <summary>
    /// DTO para cambiar el estado de un recibo.
    /// </summary>
    public class CambiarEstadoReciboDto
    {
        [Required]
        public Guid ReciboId { get; set; }

        [Required]
        public EstadoRecibo NuevoEstado { get; set; }

        [StringLength(500, ErrorMessage = "La razón no puede exceder 500 caracteres")]
        public string? Razon { get; set; }
    }
}
