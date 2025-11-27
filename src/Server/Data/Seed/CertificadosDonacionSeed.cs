using Microsoft.EntityFrameworkCore;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Data.Seed;

/// <summary>
/// Seed para cargar certificados de donación de ejemplo para pruebas E2E y desarrollo.
/// Crea certificados en estado Borrador y Emitido, asociados a recibos y donantes de prueba.
/// </summary>
public static class CertificadosDonacionSeed
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Solo ejecutar si no hay certificados de donación
        if (await db.CertificadosDonacion.AnyAsync())
        {
            return;
        }

        // Buscar un recibo de prueba existente (si hay)
        var recibo = await db.Recibos.FirstOrDefaultAsync();
        // Buscar un miembro/donante de prueba
        var miembro = await db.Miembros.FirstOrDefaultAsync();

        var certificados = new List<CertificadoDonacion>
        {
            new CertificadoDonacion
            {
                FechaDonacion = DateTime.UtcNow.AddDays(-10),
                TipoIdentificacionDonante = "CC",
                IdentificacionDonante = miembro?.Cedula ?? "1234567890",
                NombreDonante = miembro?.NombreCompleto ?? "Donante Prueba",
                DireccionDonante = miembro?.Direccion ?? "Calle 123 #45-67",
                CiudadDonante = "Medellín",
                TelefonoDonante = miembro?.Celular ?? "3001234567",
                EmailDonante = miembro?.Email ?? "donante@prueba.com",
                DescripcionDonacion = "Donación en efectivo para actividades sociales",
                ValorDonacionCOP = 500000,
                FormaDonacion = "Transferencia bancaria",
                DestinacionDonacion = "Actividades sociales",
                Observaciones = "Certificado de prueba generado por seed.",
                ReciboId = recibo?.Id,
                Estado = EstadoCertificado.Borrador,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seed"
            },
            new CertificadoDonacion
            {
                FechaDonacion = DateTime.UtcNow.AddDays(-30),
                TipoIdentificacionDonante = "NIT",
                IdentificacionDonante = "900123456-7",
                NombreDonante = "Empresa Donante S.A.S.",
                DireccionDonante = "Cra 50 # 10-20",
                CiudadDonante = "Medellín",
                TelefonoDonante = "6041234567",
                EmailDonante = "contacto@empresa.com",
                DescripcionDonacion = "Donación de insumos para evento anual",
                ValorDonacionCOP = 2000000,
                FormaDonacion = "Especie",
                DestinacionDonacion = "Evento anual",
                Observaciones = "Certificado emitido de ejemplo.",
                ReciboId = recibo?.Id,
                Estado = EstadoCertificado.Emitido,
                Ano = DateTime.UtcNow.Year,
                Consecutivo = 1,
                FechaEmision = DateTime.UtcNow.AddDays(-29),
                NombreRepresentanteLegal = "Juan Pérez",
                IdentificacionRepresentante = "12345678",
                CargoRepresentante = "Representante Legal",
                NombreContador = "Ana Gómez",
                TarjetaProfesionalContador = "TP12345",
                CreatedAt = DateTime.UtcNow.AddDays(-29),
                CreatedBy = "seed"
            }
        };

        db.CertificadosDonacion.AddRange(certificados);
        await db.SaveChangesAsync();
        Console.WriteLine($"✓ Certificados de donación de ejemplo creados: {certificados.Count}");
    }
}
