using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Services.MovimientosTesoreria;
using Server.Services.CierreContable;
using Xunit;

namespace UnitTests;

/// <summary>
/// Tests unitarios para MovimientosTesoreriaService
/// Verifica que el blindaje del cierre contable funcione en todas las operaciones
/// </summary>
public class MovimientosTesoreriaServiceTests
{
    internal sealed class FakeAuditService : Server.Services.Audit.IAuditService
    {
        public Task LogAsync(string entityType, string entityId, string action, string userName, object? oldValues = null, object? newValues = null, string? additionalInfo = null)
            => Task.CompletedTask;
        public Task<System.Collections.Generic.List<Server.Models.AuditLog>> GetEntityLogsAsync(string entityType, string entityId)
            => Task.FromResult(new System.Collections.Generic.List<Server.Models.AuditLog>());
        public Task<System.Collections.Generic.List<Server.Models.AuditLog>> GetRecentLogsAsync(int count = 100)
            => Task.FromResult(new System.Collections.Generic.List<Server.Models.AuditLog>());
    }

    internal sealed class TestDbFactory : IDbContextFactory<AppDbContext>
    {
        private readonly SqliteConnection _conn;
        public TestDbFactory(SqliteConnection conn) { _conn = conn; }
        public AppDbContext CreateDbContext()
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
            var db = new AppDbContext(opts);
            db.Database.EnsureCreated();
            return db;
        }
        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbContext());
    }

    private static (AppDbContext db, SqliteConnection conn, CierreContableService cierre, MovimientosTesoreriaService service) CreateInMemoryDb()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        conn.CreateCollation("Modern_Spanish_CI_AS", (x, y) => string.Compare(x, y, new System.Globalization.CultureInfo("es-ES"), System.Globalization.CompareOptions.IgnoreCase));
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        var audit = new FakeAuditService();
        var factory = new TestDbFactory(conn);
        var cierre = new CierreContableService(factory, audit);
        var service = new MovimientosTesoreriaService(factory, cierre, audit);
        return (db, conn, cierre, service);
    }

    [Fact]
    public async Task CreateAsync_MesCerrado_ThrowsInvalidOperationException()
    {
        // Arrange: Crear DB, cerrar el mes actual
        var (db, conn, cierreService, service) = CreateInMemoryDb();
        try
        {
            var cuenta = new CuentaFinanciera { Id = Guid.NewGuid(), Nombre = "Cuenta Test", Tipo = TipoCuenta.Bancaria };
            db.CuentasFinancieras.Add(cuenta);
            await db.SaveChangesAsync();

            var hoy = DateTime.UtcNow;
            var cierre = new CierreMensual
            {
                Ano = hoy.Year,
                Mes = hoy.Month,
                FechaCierre = DateTime.UtcNow,
                UsuarioCierre = "admin",
                SaldoFinal = 100000m
            };
            db.CierresMensuales.Add(cierre);
            await db.SaveChangesAsync();

            var movimiento = new MovimientoTesoreria
            {
                NumeroMovimiento = "MV-2026-TEST001",
                Fecha = hoy,
                CuentaFinancieraId = cuenta.Id,
                Tipo = TipoMovimientoTesoreria.Ingreso,
                Valor = 50000m,
                Descripcion = "Test ingreso",
                Medio = MedioPagoTesoreria.Transferencia
            };

            // Act & Assert: Intentar crear movimiento en mes cerrado debe fallar
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.CreateAsync(movimiento, "testuser"));
            
            Assert.Contains("Mes cerrado", ex.Message);
            Assert.Contains("no se permiten cambios", ex.Message);
        }
        finally
        {
            conn.Close();
        }
    }

    [Fact]
    public async Task UpdateAsync_MesCerrado_ThrowsInvalidOperationException()
    {
        // Arrange: Crear movimiento en mes abierto, luego cerrar el mes
        var (db, conn, cierreService, service) = CreateInMemoryDb();
        try
        {
            var cuenta = new CuentaFinanciera { Id = Guid.NewGuid(), Nombre = "Cuenta Test", Tipo = TipoCuenta.Bancaria };
            db.CuentasFinancieras.Add(cuenta);
            await db.SaveChangesAsync();

            var hoy = DateTime.UtcNow;
            var movimiento = new MovimientoTesoreria
            {
                Id = Guid.NewGuid(),
                NumeroMovimiento = "MV-2026-TEST002",
                Fecha = hoy,
                CuentaFinancieraId = cuenta.Id,
                Tipo = TipoMovimientoTesoreria.Ingreso,
                Valor = 50000m,
                Descripcion = "Test ingreso",
                Medio = MedioPagoTesoreria.Transferencia,
                Estado = EstadoMovimientoTesoreria.Borrador
            };
            db.MovimientosTesoreria.Add(movimiento);
            await db.SaveChangesAsync();

            // Cerrar el mes
            var cierre = new CierreMensual
            {
                Ano = hoy.Year,
                Mes = hoy.Month,
                FechaCierre = DateTime.UtcNow,
                UsuarioCierre = "admin",
                SaldoFinal = 100000m
            };
            db.CierresMensuales.Add(cierre);
            await db.SaveChangesAsync();

            // Intentar actualizar el movimiento
            var movimientoActualizado = new MovimientoTesoreria
            {
                NumeroMovimiento = "MV-2026-TEST002",
                Fecha = hoy,
                CuentaFinancieraId = cuenta.Id,
                Tipo = TipoMovimientoTesoreria.Ingreso,
                Valor = 75000m, // Cambio de valor
                Descripcion = "Test ingreso modificado",
                Medio = MedioPagoTesoreria.Transferencia,
                Estado = EstadoMovimientoTesoreria.Borrador
            };

            // Act & Assert: Intentar actualizar movimiento en mes cerrado debe fallar
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.UpdateAsync(movimiento.Id, movimientoActualizado, "testuser"));
            
            Assert.Contains("Mes cerrado", ex.Message);
        }
        finally
        {
            conn.Close();
        }
    }

    [Fact]
    public async Task AnularAsync_MesCerrado_ThrowsInvalidOperationException()
    {
        // Arrange: Crear movimiento en mes abierto, luego cerrar el mes
        var (db, conn, cierreService, service) = CreateInMemoryDb();
        try
        {
            var cuenta = new CuentaFinanciera { Id = Guid.NewGuid(), Nombre = "Cuenta Test", Tipo = TipoCuenta.Bancaria };
            db.CuentasFinancieras.Add(cuenta);
            await db.SaveChangesAsync();

            var hoy = DateTime.UtcNow;
            var movimiento = new MovimientoTesoreria
            {
                Id = Guid.NewGuid(),
                NumeroMovimiento = "MV-2026-TEST003",
                Fecha = hoy,
                CuentaFinancieraId = cuenta.Id,
                Tipo = TipoMovimientoTesoreria.Egreso,
                Valor = 30000m,
                Descripcion = "Test egreso",
                Medio = MedioPagoTesoreria.Efectivo,
                Estado = EstadoMovimientoTesoreria.Aprobado
            };
            db.MovimientosTesoreria.Add(movimiento);
            await db.SaveChangesAsync();

            // Cerrar el mes
            var cierre = new CierreMensual
            {
                Ano = hoy.Year,
                Mes = hoy.Month,
                FechaCierre = DateTime.UtcNow,
                UsuarioCierre = "admin",
                SaldoFinal = 100000m
            };
            db.CierresMensuales.Add(cierre);
            await db.SaveChangesAsync();

            // Act & Assert: Intentar anular movimiento en mes cerrado debe fallar
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.AnularAsync(movimiento.Id, "Error en registro", "testuser"));
            
            Assert.Contains("Mes cerrado", ex.Message);
            Assert.Contains("no se permiten cambios", ex.Message);
        }
        finally
        {
            conn.Close();
        }
    }

    [Fact]
    public async Task DeleteAsync_MesCerrado_ThrowsInvalidOperationException()
    {
        // Arrange: Crear movimiento en mes abierto, luego cerrar el mes
        var (db, conn, cierreService, service) = CreateInMemoryDb();
        try
        {
            var cuenta = new CuentaFinanciera { Id = Guid.NewGuid(), Nombre = "Cuenta Test", Tipo = TipoCuenta.Bancaria };
            db.CuentasFinancieras.Add(cuenta);
            await db.SaveChangesAsync();

            var hoy = DateTime.UtcNow;
            var movimiento = new MovimientoTesoreria
            {
                Id = Guid.NewGuid(),
                NumeroMovimiento = "MV-2026-TEST004",
                Fecha = hoy,
                CuentaFinancieraId = cuenta.Id,
                Tipo = TipoMovimientoTesoreria.Ingreso,
                Valor = 10000m,
                Descripcion = "Test eliminar",
                Medio = MedioPagoTesoreria.Nequi,
                Estado = EstadoMovimientoTesoreria.Borrador
            };
            db.MovimientosTesoreria.Add(movimiento);
            await db.SaveChangesAsync();

            // Cerrar el mes
            var cierre = new CierreMensual
            {
                Ano = hoy.Year,
                Mes = hoy.Month,
                FechaCierre = DateTime.UtcNow,
                UsuarioCierre = "admin",
                SaldoFinal = 100000m
            };
            db.CierresMensuales.Add(cierre);
            await db.SaveChangesAsync();

            // Act & Assert: Intentar eliminar movimiento en mes cerrado debe fallar
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.DeleteAsync(movimiento.Id, "testuser"));
            
            Assert.Contains("Mes cerrado", ex.Message);
            Assert.Contains("no se permiten cambios", ex.Message);
        }
        finally
        {
            conn.Close();
        }
    }

    [Fact]
    public async Task CreateAsync_MesAbierto_Success()
    {
        // Arrange: DB sin cierre mensual
        var (db, conn, cierreService, service) = CreateInMemoryDb();
        try
        {
            var cuenta = new CuentaFinanciera { Id = Guid.NewGuid(), Nombre = "Cuenta Test", Tipo = TipoCuenta.Bancaria };
            db.CuentasFinancieras.Add(cuenta);
            await db.SaveChangesAsync();

            var movimiento = new MovimientoTesoreria
            {
                NumeroMovimiento = "MV-2026-TEST005",
                Fecha = DateTime.UtcNow,
                CuentaFinancieraId = cuenta.Id,
                Tipo = TipoMovimientoTesoreria.Ingreso,
                Valor = 50000m,
                Descripcion = "Test ingreso exitoso",
                Medio = MedioPagoTesoreria.Transferencia
            };

            // Act: Crear movimiento en mes abierto debe funcionar
            var result = await service.CreateAsync(movimiento, "testuser");

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("MV-2026-TEST005", result.NumeroMovimiento);
            Assert.Equal(50000m, result.Valor);
        }
        finally
        {
            conn.Close();
        }
    }

    [Fact]
    public async Task UpdateAsync_CambiarFechaAMesCerrado_ThrowsInvalidOperationException()
    {
        // Arrange: Movimiento en mes abierto (enero), cerrar febrero, intentar mover a febrero
        var (db, conn, cierreService, service) = CreateInMemoryDb();
        try
        {
            var cuenta = new CuentaFinanciera { Id = Guid.NewGuid(), Nombre = "Cuenta Test", Tipo = TipoCuenta.Bancaria };
            db.CuentasFinancieras.Add(cuenta);
            await db.SaveChangesAsync();

            var fechaEnero = new DateTime(2026, 1, 15);
            var movimiento = new MovimientoTesoreria
            {
                Id = Guid.NewGuid(),
                NumeroMovimiento = "MV-2026-TEST006",
                Fecha = fechaEnero,
                CuentaFinancieraId = cuenta.Id,
                Tipo = TipoMovimientoTesoreria.Ingreso,
                Valor = 50000m,
                Descripcion = "Test enero",
                Medio = MedioPagoTesoreria.Transferencia,
                Estado = EstadoMovimientoTesoreria.Borrador
            };
            db.MovimientosTesoreria.Add(movimiento);
            await db.SaveChangesAsync();

            // Cerrar febrero
            var cierreFebrero = new CierreMensual
            {
                Ano = 2026,
                Mes = 2,
                FechaCierre = DateTime.UtcNow,
                UsuarioCierre = "admin",
                SaldoFinal = 100000m
            };
            db.CierresMensuales.Add(cierreFebrero);
            await db.SaveChangesAsync();

            // Intentar mover el movimiento a febrero (mes cerrado)
            var movimientoActualizado = new MovimientoTesoreria
            {
                NumeroMovimiento = "MV-2026-TEST006",
                Fecha = new DateTime(2026, 2, 10), // ❌ Intentar mover a febrero cerrado
                CuentaFinancieraId = cuenta.Id,
                Tipo = TipoMovimientoTesoreria.Ingreso,
                Valor = 50000m,
                Descripcion = "Test febrero",
                Medio = MedioPagoTesoreria.Transferencia,
                Estado = EstadoMovimientoTesoreria.Borrador
            };

            // Act & Assert: Debe fallar porque la nueva fecha está en mes cerrado
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.UpdateAsync(movimiento.Id, movimientoActualizado, "testuser"));
            
            Assert.Contains("Mes cerrado", ex.Message);
        }
        finally
        {
            conn.Close();
        }
    }

    [Fact]
    public async Task AnularAsync_SetsAllCancellationFields()
    {
        // Arrange: Crear movimiento en mes abierto
        var (db, conn, cierreService, service) = CreateInMemoryDb();
        try
        {
            var cuenta = new CuentaFinanciera { Id = Guid.NewGuid(), Nombre = "Cuenta Test", Tipo = TipoCuenta.Bancaria };
            db.CuentasFinancieras.Add(cuenta);
            await db.SaveChangesAsync();

            var movimiento = new MovimientoTesoreria
            {
                NumeroMovimiento = "MV-2026-TEST007",
                Fecha = DateTime.UtcNow,
                CuentaFinancieraId = cuenta.Id,
                Tipo = TipoMovimientoTesoreria.Ingreso,
                Valor = 100000m,
                Descripcion = "Test anulación",
                Medio = MedioPagoTesoreria.Transferencia
            };

            var created = await service.CreateAsync(movimiento, "creador");

            // Act: Anular el movimiento
            var motivoAnulacion = "Error en el registro del valor";
            var usuarioAnulacion = "supervisor";
            var resultado = await service.AnularAsync(created.Id, motivoAnulacion, usuarioAnulacion);

            // Assert: Verificar que todos los campos de anulación están seteados
            Assert.Equal(EstadoMovimientoTesoreria.Anulado, resultado.Estado);
            Assert.Equal(motivoAnulacion, resultado.MotivoAnulacion);
            Assert.NotNull(resultado.FechaAnulacion);
            Assert.Equal(usuarioAnulacion, resultado.UsuarioAnulacion);
            
            // Verificar que la fecha de anulación es reciente (dentro del último minuto)
            Assert.True((DateTime.UtcNow - resultado.FechaAnulacion.Value).TotalMinutes < 1);
        }
        finally
        {
            conn.Close();
        }
    }
}
