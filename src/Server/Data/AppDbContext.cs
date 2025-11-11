using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

/// <summary>
/// Contexto de datos de la aplicación. Contiene DbSet para Miembros y modelos de tesorería.
/// Hereda de IdentityDbContext para soporte de usuarios y roles.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Miembro> Miembros => Set<Miembro>();
    public DbSet<Concepto> Conceptos => Set<Concepto>();
    public DbSet<TasaCambio> TasasCambio => Set<TasaCambio>();
    public DbSet<Recibo> Recibos => Set<Recibo>();
    public DbSet<ReciboItem> ReciboItems => Set<ReciboItem>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<Egreso> Egresos => Set<Egreso>();
    public DbSet<Ingreso> Ingresos => Set<Ingreso>();
    public DbSet<CierreMensual> CierresMensuales => Set<CierreMensual>();
    public DbSet<CertificadoDonacion> CertificadosDonacion => Set<CertificadoDonacion>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Gerencia de Negocios - Inventario y Ventas
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<CompraProducto> ComprasProductos => Set<CompraProducto>();
    public DbSet<DetalleCompraProducto> DetallesComprasProductos => Set<DetalleCompraProducto>();
    public DbSet<VentaProducto> VentasProductos => Set<VentaProducto>();
    public DbSet<DetalleVentaProducto> DetallesVentasProductos => Set<DetalleVentaProducto>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();
    public DbSet<DetalleCotizacion> DetallesCotizaciones => Set<DetalleCotizacion>();
    public DbSet<HistorialPrecio> HistorialesPrecios => Set<HistorialPrecio>();

    // Gestión financiera avanzada
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<ConciliacionBancaria> ConciliacionesBancarias => Set<ConciliacionBancaria>();
    public DbSet<ItemConciliacion> ItemsConciliacion => Set<ItemConciliacion>();

    // Sistema transversal
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Miembro>(b =>
        {
            b.HasIndex(x => x.Documento).IsUnique(false);
            b.HasIndex(x => x.Cedula).IsUnique(false);
            b.Property(x => x.NombreCompleto).HasMaxLength(250).UseCollation("Modern_Spanish_CI_AS");
            b.Property(x => x.Nombres).HasMaxLength(120).UseCollation("Modern_Spanish_CI_AS");
            b.Property(x => x.Apellidos).HasMaxLength(120).UseCollation("Modern_Spanish_CI_AS");
            b.Property(x => x.Documento).HasMaxLength(40);
            b.Property(x => x.Cedula).HasMaxLength(40);
            b.Property(x => x.Email).HasMaxLength(160);
            b.Property(x => x.Celular).HasMaxLength(50);
            b.Property(x => x.Telefono).HasMaxLength(50);
            b.Property(x => x.Direccion).HasMaxLength(500).UseCollation("Modern_Spanish_CI_AS");
            b.Property(x => x.Cargo).HasMaxLength(100).UseCollation("Modern_Spanish_CI_AS");
            b.Property(x => x.Rango).HasMaxLength(50).UseCollation("Modern_Spanish_CI_AS");
        });

        modelBuilder.Entity<Concepto>(b =>
        {
            b.HasIndex(x => x.Codigo).IsUnique();
            b.Property(x => x.Codigo).HasMaxLength(60);
            b.Property(x => x.Nombre).HasMaxLength(200);
            b.Property(x => x.PrecioBase).HasPrecision(18, 2);
        });

        modelBuilder.Entity<TasaCambio>(b =>
        {
            b.HasIndex(x => x.Fecha).IsUnique();
            b.Property(x => x.UsdCop).HasPrecision(18, 4);
        });

        modelBuilder.Entity<Egreso>(b =>
        {
            b.Property(x => x.Categoria).HasMaxLength(120);
            b.Property(x => x.Proveedor).HasMaxLength(200);
            b.Property(x => x.Descripcion).HasMaxLength(1000);
            b.Property(x => x.ValorCop).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Ingreso>(b =>
        {
            b.HasIndex(i => i.NumeroIngreso).IsUnique();
            b.Property(i => i.NumeroIngreso).HasMaxLength(50).IsRequired();
            b.Property(i => i.Categoria).HasMaxLength(120).IsRequired();
            b.Property(i => i.Descripcion).HasMaxLength(1000).IsRequired();
            b.Property(i => i.ValorCop).HasPrecision(18, 2);
            b.Property(i => i.MetodoPago).HasMaxLength(50).IsRequired();
            b.Property(i => i.ReferenciaTransaccion).HasMaxLength(200);
            b.Property(i => i.Observaciones).HasMaxLength(1000);
            b.Property(i => i.CreatedBy).HasMaxLength(256);
            b.Property(i => i.UpdatedBy).HasMaxLength(256);
        });

        modelBuilder.Entity<Recibo>(b =>
        {
            b.HasMany(r => r.Items).WithOne(i => i.Recibo).HasForeignKey(i => i.ReciboId);
            b.HasOne(r => r.Pago).WithOne(p => p.Recibo).HasForeignKey<Pago>(p => p.ReciboId).IsRequired(false);
            b.Property(r => r.TotalCop).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ReciboItem>(b =>
        {
            b.Property(i => i.PrecioUnitarioMonedaOrigen).HasPrecision(18, 2);
            b.Property(i => i.TrmAplicada).HasPrecision(18, 4);
            b.Property(i => i.SubtotalCop).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Pago>(b =>
        {
            b.Property(p => p.ValorPagadoCop).HasPrecision(18, 2);
        });

        modelBuilder.Entity<CierreMensual>(b =>
        {
            b.HasIndex(c => new { c.Ano, c.Mes }).IsUnique();
            b.Property(c => c.SaldoInicialCalculado).HasPrecision(18, 2);
            b.Property(c => c.TotalIngresos).HasPrecision(18, 2);
            b.Property(c => c.TotalEgresos).HasPrecision(18, 2);
            b.Property(c => c.SaldoFinal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<CertificadoDonacion>(b =>
        {
            b.HasIndex(c => new { c.Ano, c.Consecutivo }).IsUnique();
            b.Property(c => c.ValorDonacionCOP).HasPrecision(18, 2);
            b.Property(c => c.TipoIdentificacionDonante).HasMaxLength(10);
            b.Property(c => c.IdentificacionDonante).HasMaxLength(50);
            b.Property(c => c.NombreDonante).HasMaxLength(300);
            b.Property(c => c.DireccionDonante).HasMaxLength(500);
            b.Property(c => c.CiudadDonante).HasMaxLength(150);
            b.Property(c => c.TelefonoDonante).HasMaxLength(50);
            b.Property(c => c.EmailDonante).HasMaxLength(200);
            b.Property(c => c.DescripcionDonacion).HasMaxLength(2000);
            b.Property(c => c.FormaDonacion).HasMaxLength(100);
            b.Property(c => c.DestinacionDonacion).HasMaxLength(1000);
            b.Property(c => c.NitEntidad).HasMaxLength(50);
            b.Property(c => c.NombreEntidad).HasMaxLength(300);
            b.Property(c => c.ResolucionRTE).HasMaxLength(100);
            b.Property(c => c.NombreRepresentanteLegal).HasMaxLength(300);
            b.Property(c => c.IdentificacionRepresentante).HasMaxLength(50);
            b.Property(c => c.CargoRepresentante).HasMaxLength(100);
            b.Property(c => c.NombreContador).HasMaxLength(300);
            b.Property(c => c.TarjetaProfesionalContador).HasMaxLength(50);
            b.Property(c => c.NombreRevisorFiscal).HasMaxLength(300);
            b.Property(c => c.TarjetaProfesionalRevisorFiscal).HasMaxLength(50);
            b.HasOne(c => c.Recibo).WithMany().HasForeignKey(c => c.ReciboId).IsRequired(false);
        });

        modelBuilder.Entity<AuditLog>(b =>
        {
            b.HasKey(a => a.Id);
            b.HasIndex(a => new { a.EntityType, a.EntityId });
            b.HasIndex(a => a.Timestamp);
            b.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            b.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
            b.Property(a => a.Action).HasMaxLength(100).IsRequired();
            b.Property(a => a.UserName).HasMaxLength(256).IsRequired();
            b.Property(a => a.IpAddress).HasMaxLength(50);
        });

        // Gerencia de Negocios - Configuración de entidades
        modelBuilder.Entity<Producto>(b =>
        {
            b.HasIndex(p => p.Codigo).IsUnique();
            b.Property(p => p.Codigo).HasMaxLength(50).IsRequired();
            b.Property(p => p.Nombre).HasMaxLength(200).IsRequired();
            b.Property(p => p.PrecioVentaCOP).HasPrecision(18, 2);
            b.Property(p => p.PrecioVentaUSD).HasPrecision(18, 2);
            b.Property(p => p.Talla).HasMaxLength(20);
            b.Property(p => p.Descripcion).HasMaxLength(1000);
            b.Property(p => p.ImagenUrl).HasMaxLength(500);
            b.Property(p => p.CreatedBy).HasMaxLength(256);
            b.Property(p => p.UpdatedBy).HasMaxLength(256);
        });

        modelBuilder.Entity<CompraProducto>(b =>
        {
            b.HasIndex(c => c.NumeroCompra).IsUnique();
            b.Property(c => c.NumeroCompra).HasMaxLength(50).IsRequired();
            b.Property(c => c.Proveedor).HasMaxLength(200);
            b.Property(c => c.TotalCOP).HasPrecision(18, 2);
            b.Property(c => c.TotalUSD).HasPrecision(18, 2);
            b.Property(c => c.TrmAplicada).HasPrecision(18, 4);
            b.Property(c => c.NumeroFacturaProveedor).HasMaxLength(100);
            b.Property(c => c.Observaciones).HasMaxLength(1000);
            b.Property(c => c.CreatedBy).HasMaxLength(256);
            b.Property(c => c.UpdatedBy).HasMaxLength(256);
            b.HasOne(c => c.ProveedorObj).WithMany(p => p.Compras).HasForeignKey(c => c.ProveedorId).IsRequired(false);
            b.HasOne(c => c.Egreso).WithMany().HasForeignKey(c => c.EgresoId).IsRequired(false);
        });

        modelBuilder.Entity<DetalleCompraProducto>(b =>
        {
            b.Property(d => d.PrecioUnitarioCOP).HasPrecision(18, 2);
            b.Property(d => d.SubtotalCOP).HasPrecision(18, 2);
            b.Property(d => d.Notas).HasMaxLength(500);
            b.HasOne(d => d.Compra)
                .WithMany(c => c.Detalles)
                .HasForeignKey(d => d.CompraId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(d => d.Producto)
                .WithMany(p => p.DetallesCompra)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VentaProducto>(b =>
        {
            b.HasIndex(v => v.NumeroVenta).IsUnique();
            b.Property(v => v.NumeroVenta).HasMaxLength(50).IsRequired();
            b.Property(v => v.NombreCliente).HasMaxLength(200);
            b.Property(v => v.IdentificacionCliente).HasMaxLength(50);
            b.Property(v => v.EmailCliente).HasMaxLength(100);
            b.Property(v => v.TotalCOP).HasPrecision(18, 2);
            b.Property(v => v.TotalUSD).HasPrecision(18, 2);
            b.Property(v => v.Observaciones).HasMaxLength(1000);
            b.Property(v => v.CreatedBy).HasMaxLength(256);
            b.Property(v => v.UpdatedBy).HasMaxLength(256);
            b.HasOne(v => v.Miembro).WithMany().HasForeignKey(v => v.MiembroId).IsRequired(false);
            b.HasOne(v => v.Cliente).WithMany(c => c.Ventas).HasForeignKey(v => v.ClienteId).IsRequired(false);
            b.HasOne(v => v.Recibo).WithMany().HasForeignKey(v => v.ReciboId).IsRequired(false);
            b.HasOne(v => v.Ingreso).WithMany().HasForeignKey(v => v.IngresoId).IsRequired(false);
        });

        modelBuilder.Entity<DetalleVentaProducto>(b =>
        {
            b.Property(d => d.PrecioUnitarioCOP).HasPrecision(18, 2);
            b.Property(d => d.DescuentoCOP).HasPrecision(18, 2);
            b.Property(d => d.SubtotalCOP).HasPrecision(18, 2);
            b.Property(d => d.Notas).HasMaxLength(500);
            b.HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(d => d.Producto)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MovimientoInventario>(b =>
        {
            b.HasIndex(m => m.NumeroMovimiento).IsUnique();
            b.Property(m => m.NumeroMovimiento).HasMaxLength(50).IsRequired();
            b.Property(m => m.Motivo).HasMaxLength(500).IsRequired();
            b.Property(m => m.Observaciones).HasMaxLength(1000);
            b.Property(m => m.CreatedBy).HasMaxLength(256).IsRequired();
            b.HasOne(m => m.Producto)
                .WithMany(p => p.MovimientosInventario)
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(m => m.Compra)
                .WithMany()
                .HasForeignKey(m => m.CompraId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(m => m.Venta)
                .WithMany()
                .HasForeignKey(m => m.VentaId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Proveedores
        modelBuilder.Entity<Proveedor>(b =>
        {
            b.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
            b.Property(p => p.Nit).HasMaxLength(50);
            b.Property(p => p.ContactoNombre).HasMaxLength(100);
            b.Property(p => p.ContactoTelefono).HasMaxLength(50);
            b.Property(p => p.ContactoEmail).HasMaxLength(100);
            b.Property(p => p.Direccion).HasMaxLength(200);
            b.Property(p => p.Ciudad).HasMaxLength(100);
            b.Property(p => p.Pais).HasMaxLength(50);
            b.Property(p => p.TerminosPago).HasMaxLength(100);
            b.Property(p => p.Notas).HasMaxLength(500);
            b.Property(p => p.CreatedBy).HasMaxLength(256);
            b.Property(p => p.UpdatedBy).HasMaxLength(256);
        });

        // Clientes
        modelBuilder.Entity<Cliente>(b =>
        {
            b.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            b.Property(c => c.Identificacion).HasMaxLength(50);
            b.Property(c => c.Tipo).HasMaxLength(50);
            b.Property(c => c.Telefono).HasMaxLength(50);
            b.Property(c => c.Email).HasMaxLength(100);
            b.Property(c => c.Direccion).HasMaxLength(200);
            b.Property(c => c.Ciudad).HasMaxLength(100);
            b.Property(c => c.LimiteCredito).HasPrecision(18, 2);
            b.Property(c => c.Notas).HasMaxLength(500);
            b.Property(c => c.CreatedBy).HasMaxLength(256);
            b.Property(c => c.UpdatedBy).HasMaxLength(256);
        });

        // Cotizaciones
        modelBuilder.Entity<Cotizacion>(b =>
        {
            b.HasIndex(c => c.NumeroCotizacion).IsUnique();
            b.Property(c => c.NumeroCotizacion).HasMaxLength(50).IsRequired();
            b.Property(c => c.NombreCliente).HasMaxLength(100);
            b.Property(c => c.EmailCliente).HasMaxLength(100);
            b.Property(c => c.TelefonoCliente).HasMaxLength(50);
            b.Property(c => c.SubtotalCOP).HasPrecision(18, 2);
            b.Property(c => c.DescuentoCOP).HasPrecision(18, 2);
            b.Property(c => c.TotalCOP).HasPrecision(18, 2);
            b.Property(c => c.TotalUSD).HasPrecision(18, 2);
            b.Property(c => c.Estado).HasMaxLength(50);
            b.Property(c => c.Observaciones).HasMaxLength(1000);
            b.Property(c => c.CreatedBy).HasMaxLength(256);
            b.Property(c => c.UpdatedBy).HasMaxLength(256);
            b.HasOne(c => c.Cliente).WithMany().HasForeignKey(c => c.ClienteId).IsRequired(false);
            b.HasOne(c => c.Miembro).WithMany().HasForeignKey(c => c.MiembroId).IsRequired(false);
            b.HasOne(c => c.Venta).WithMany().HasForeignKey(c => c.VentaId).IsRequired(false);
        });

        modelBuilder.Entity<DetalleCotizacion>(b =>
        {
            b.Property(d => d.PrecioUnitarioCOP).HasPrecision(18, 2);
            b.Property(d => d.DescuentoCOP).HasPrecision(18, 2);
            b.Property(d => d.SubtotalCOP).HasPrecision(18, 2);
            b.Property(d => d.Notas).HasMaxLength(500);
            b.HasOne(d => d.Cotizacion).WithMany(c => c.Detalles).HasForeignKey(d => d.CotizacionId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(d => d.Producto).WithMany().HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);
        });

        // Historial de precios
        modelBuilder.Entity<HistorialPrecio>(b =>
        {
            b.Property(h => h.PrecioAnteriorCOP).HasPrecision(18, 2);
            b.Property(h => h.PrecioNuevoCOP).HasPrecision(18, 2);
            b.Property(h => h.PrecioAnteriorUSD).HasPrecision(18, 2);
            b.Property(h => h.PrecioNuevoUSD).HasPrecision(18, 2);
            b.Property(h => h.Motivo).HasMaxLength(500);
            b.Property(h => h.CambiadoPor).HasMaxLength(256);
            b.HasOne(h => h.Producto).WithMany().HasForeignKey(h => h.ProductoId).OnDelete(DeleteBehavior.Cascade);
        });

        // Presupuestos
        modelBuilder.Entity<Presupuesto>(b =>
        {
            b.HasIndex(p => new { p.Ano, p.Mes, p.ConceptoId }).IsUnique();
            b.Property(p => p.MontoPresupuestado).HasPrecision(18, 2);
            b.Property(p => p.MontoEjecutado).HasPrecision(18, 2);
            b.Property(p => p.Notas).HasMaxLength(500);
            b.Property(p => p.CreatedBy).HasMaxLength(256);
            b.Property(p => p.UpdatedBy).HasMaxLength(256);
            b.HasOne(p => p.Concepto).WithMany().HasForeignKey(p => p.ConceptoId).OnDelete(DeleteBehavior.Restrict);
        });

        // Conciliación bancaria
        modelBuilder.Entity<ConciliacionBancaria>(b =>
        {
            b.HasIndex(c => new { c.Ano, c.Mes }).IsUnique();
            b.Property(c => c.SaldoLibros).HasPrecision(18, 2);
            b.Property(c => c.SaldoBanco).HasPrecision(18, 2);
            b.Property(c => c.Diferencia).HasPrecision(18, 2);
            b.Property(c => c.Estado).HasMaxLength(50);
            b.Property(c => c.Observaciones).HasMaxLength(1000);
            b.Property(c => c.CreatedBy).HasMaxLength(256);
            b.Property(c => c.UpdatedBy).HasMaxLength(256);
        });

        modelBuilder.Entity<ItemConciliacion>(b =>
        {
            b.Property(i => i.Tipo).HasMaxLength(100);
            b.Property(i => i.Descripcion).HasMaxLength(500);
            b.Property(i => i.Monto).HasPrecision(18, 2);
            b.HasOne(i => i.Conciliacion).WithMany(c => c.Items).HasForeignKey(i => i.ConciliacionId).OnDelete(DeleteBehavior.Cascade);
        });

        // Notificaciones
        modelBuilder.Entity<Notificacion>(b =>
        {
            b.HasIndex(n => new { n.UsuarioId, n.Leida });
            b.HasIndex(n => n.CreatedAt);
            b.Property(n => n.UsuarioId).HasMaxLength(450).IsRequired();
            b.Property(n => n.Tipo).HasMaxLength(50);
            b.Property(n => n.Titulo).HasMaxLength(200);
            b.Property(n => n.Mensaje).HasMaxLength(1000);
            b.Property(n => n.Url).HasMaxLength(500);
            b.HasOne(n => n.Usuario).WithMany().HasForeignKey(n => n.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
