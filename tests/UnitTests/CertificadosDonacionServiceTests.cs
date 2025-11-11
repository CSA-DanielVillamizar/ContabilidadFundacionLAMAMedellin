using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Server.Configuration;
using Server.Data;
using Server.DTOs.Donaciones;
using Server.Models;
using Server.Services.Donaciones;
using Server.Services.Email;
using Xunit;

namespace UnitTests;

public class CertificadosDonacionServiceTests
{
    private sealed class TestEnv : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "TestApp";
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private sealed class FakeEmailService : IEmailService
    {
        public int SentCount { get; private set; }
        public string? LastTo { get; private set; }
        public string? LastSubject { get; private set; }
        public Task SendAsync(string to, string subject, string htmlBody, (string fileName, byte[] content, string contentType)? attachment = null, System.Threading.CancellationToken ct = default)
        {
            SentCount++;
            LastTo = to;
            LastSubject = subject;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuditService : Server.Services.Audit.IAuditService
    {
        public Task LogAsync(string entityType, string entityId, string action, string userName, object? oldValues = null, object? newValues = null, string? additionalInfo = null)
            => Task.CompletedTask;
        public Task<System.Collections.Generic.List<Server.Models.AuditLog>> GetEntityLogsAsync(string entityType, string entityId)
            => Task.FromResult(new System.Collections.Generic.List<Server.Models.AuditLog>());
        public Task<System.Collections.Generic.List<Server.Models.AuditLog>> GetRecentLogsAsync(int count = 100)
            => Task.FromResult(new System.Collections.Generic.List<Server.Models.AuditLog>());
    }

    private static (AppDbContext db, SqliteConnection conn) CreateInMemoryDb()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
    conn.CreateCollation("Modern_Spanish_CI_AS", (x, y) => string.Compare(x, y, new System.Globalization.CultureInfo("es-ES"), System.Globalization.CompareOptions.IgnoreCase));
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return (db, conn);
    }

    private static EntidadRTEOptions MakeEntidad()
    {
        return new EntidadRTEOptions
        {
            NombreCompleto = "Fundación L.A.M.A. Medellín",
            NIT = "900123456-7",
            EsRTE = true,
            NumeroResolucionRTE = "123",
            FechaResolucionRTE = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            RepresentanteLegal = new RepresentanteLegalOptions { NombreCompleto = "REP LEGAL", NumeroIdentificacion = "1000", Cargo = "Representante" },
            ContadorPublico = new ContadorPublicoOptions { NombreCompleto = "CONTADOR", TarjetaProfesional = "TP-1" },
            RevisorFiscal = new RevisorFiscalOptions { NombreCompleto = "", TarjetaProfesional = "" }
        };
    }

    private static CreateCertificadoDonacionDto MakeDraft()
    {
        return new CreateCertificadoDonacionDto
        {
            FechaDonacion = DateTime.UtcNow.Date,
            TipoIdentificacionDonante = "CC",
            IdentificacionDonante = "1234567890",
            NombreDonante = "Juan Pérez",
            DireccionDonante = "Calle 1",
            CiudadDonante = "Medellín",
            TelefonoDonante = "3000000000",
            EmailDonante = "juan@example.com",
            DescripcionDonacion = "Donación en dinero",
            ValorDonacionCOP = 500000m,
            FormaDonacion = "Transferencia",
            DestinacionDonacion = "Programas sociales",
            Observaciones = null,
            ReciboId = null
        };
    }

    [Fact]
    public async Task Emitir_AsignaConsecutivoYFecha_CuandoEsBorrador()
    {
        var (db, conn) = CreateInMemoryDb();
        await using var _ = conn;
        var email = new FakeEmailService();
        var svc = new CertificadosDonacionService(db, Options.Create(MakeEntidad()), email, Options.Create(new SmtpOptions { SendOnCertificateEmission = false }), new TestEnv(), new FakeAuditService());

        var id = await svc.CreateAsync(MakeDraft(), "tester");
        var dto = new EmitirCertificadoDto
        {
            Id = id,
            NombreRepresentanteLegal = "REP LEGAL",
            IdentificacionRepresentante = "1000",
            CargoRepresentante = "Representante",
            NombreContador = "CONTADOR",
            TarjetaProfesionalContador = "TP-1",
            NombreRevisorFiscal = null,
            TarjetaProfesionalRevisorFiscal = null
        };

        var ok = await svc.EmitirAsync(dto, "tester");
        Assert.True(ok);

        var cert = await db.CertificadosDonacion.FirstAsync(c => c.Id == id);
        Assert.Equal(EstadoCertificado.Emitido, cert.Estado);
        Assert.True(cert.Consecutivo > 0);
        Assert.Equal(DateTime.UtcNow.Year, cert.Ano);
        Assert.True(cert.FechaEmision > DateTime.UtcNow.AddMinutes(-5));
    }

    [Fact]
    public async Task Emitir_EnviaEmail_SoloSiSmtpHabilitadoYEmailDonante()
    {
        var (db, conn) = CreateInMemoryDb();
        await using var _ = conn;
        var email = new FakeEmailService();
        var svc = new CertificadosDonacionService(db, Options.Create(MakeEntidad()), email, Options.Create(new SmtpOptions { SendOnCertificateEmission = true }), new TestEnv(), new FakeAuditService());

        // con email
        var id = await svc.CreateAsync(MakeDraft(), "tester");
        var dto = new EmitirCertificadoDto
        {
            Id = id,
            NombreRepresentanteLegal = "REP LEGAL",
            IdentificacionRepresentante = "1000",
            CargoRepresentante = "Representante",
            NombreContador = "CONTADOR",
            TarjetaProfesionalContador = "TP-1",
            NombreRevisorFiscal = null,
            TarjetaProfesionalRevisorFiscal = null
        };
        var ok = await svc.EmitirAsync(dto, "tester");
        Assert.True(ok);
        Assert.Equal(1, email.SentCount);

        // sin email del donante
    var id2 = await svc.CreateAsync(new CreateCertificadoDonacionDto { FechaDonacion = DateTime.UtcNow.Date, TipoIdentificacionDonante = "CC", IdentificacionDonante = "1", NombreDonante = "Ana", ValorDonacionCOP = 1000, FormaDonacion = "Efectivo", DescripcionDonacion = "Donación" }, "tester");
        var dto2 = new EmitirCertificadoDto
        {
            Id = id2,
            NombreRepresentanteLegal = "REP LEGAL",
            IdentificacionRepresentante = "1000",
            CargoRepresentante = "Representante",
            NombreContador = "CONTADOR",
            TarjetaProfesionalContador = "TP-1",
            NombreRevisorFiscal = null,
            TarjetaProfesionalRevisorFiscal = null
        };
        var ok2 = await svc.EmitirAsync(dto2, "tester");
        Assert.True(ok2);
        Assert.Equal(1, email.SentCount); // no incrementa
    }

    [Fact]
    public async Task Anular_CambiaEstadoYMotivo_SoloSiEmitido()
    {
        var (db, conn) = CreateInMemoryDb();
        await using var _ = conn;
        var email = new FakeEmailService();
        var svc = new CertificadosDonacionService(db, Options.Create(MakeEntidad()), email, Options.Create(new SmtpOptions { SendOnCertificateEmission = false }), new TestEnv());

        var id = await svc.CreateAsync(MakeDraft(), "tester");
        // Intentar anular en borrador debe fallar con excepción
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.AnularAsync(new AnularCertificadoDto { Id = id, RazonAnulacion = "Error" }, "tester"));

        // Emitir y luego anular
        await svc.EmitirAsync(new EmitirCertificadoDto
        {
            Id = id,
            NombreRepresentanteLegal = "REP LEGAL",
            IdentificacionRepresentante = "1000",
            CargoRepresentante = "Representante",
            NombreContador = "CONTADOR",
            TarjetaProfesionalContador = "TP-1",
            NombreRevisorFiscal = null,
            TarjetaProfesionalRevisorFiscal = null
        }, "tester");

        var ok = await svc.AnularAsync(new AnularCertificadoDto { Id = id, RazonAnulacion = "Corrección" }, "tester");
        Assert.True(ok);
        var cert = await db.CertificadosDonacion.FirstAsync(c => c.Id == id);
        Assert.Equal(EstadoCertificado.Anulado, cert.Estado);
        Assert.Equal("Corrección", cert.RazonAnulacion);
        Assert.NotNull(cert.FechaAnulacion);
    }

    [Fact]
    public async Task ReenviarEmailAsync_RegresaFalse_CuandoNoAplica()
    {
        var (db, conn) = CreateInMemoryDb();
        await using var _ = conn;
        var email = new FakeEmailService();
        var svc = new CertificadosDonacionService(db, Options.Create(MakeEntidad()), email, Options.Create(new SmtpOptions { SendOnCertificateEmission = false }), new TestEnv());

        var id = await svc.CreateAsync(MakeDraft(), "tester");
        // Borrador -> false
        var r1 = await svc.ReenviarEmailAsync(id);
        Assert.False(r1);

        // Emitido sin email -> false
        await svc.UpdateAsync(new UpdateCertificadoDonacionDto
        {
            Id = id,
            FechaDonacion = DateTime.UtcNow.Date,
            TipoIdentificacionDonante = "CC",
            IdentificacionDonante = "2",
            NombreDonante = "Juan",
            DireccionDonante = null,
            CiudadDonante = null,
            TelefonoDonante = null,
            EmailDonante = null,
            DescripcionDonacion = "",
            ValorDonacionCOP = 1000,
            FormaDonacion = "Efectivo",
            DestinacionDonacion = null,
            Observaciones = null,
            ReciboId = null
        }, "tester");

        await svc.EmitirAsync(new EmitirCertificadoDto
        {
            Id = id,
            NombreRepresentanteLegal = "REP LEGAL",
            IdentificacionRepresentante = "1000",
            CargoRepresentante = "Representante",
            NombreContador = "CONTADOR",
            TarjetaProfesionalContador = "TP-1",
            NombreRevisorFiscal = null,
            TarjetaProfesionalRevisorFiscal = null
        }, "tester");

        var r2 = await svc.ReenviarEmailAsync(id);
        Assert.False(r2);
        Assert.Equal(0, email.SentCount);
    }
}
