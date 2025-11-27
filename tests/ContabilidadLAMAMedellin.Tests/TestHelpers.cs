using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Services.Audit;
using Server.Services.Exchange;

namespace ContabilidadLAMAMedellin.Tests;

/// <summary>
/// Fábrica simple de DbContext para pruebas usando opciones provistas.
/// </summary>
public class TestDbFactory : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> _options;
    public TestDbFactory(DbContextOptions<AppDbContext> options) => _options = options;
    public AppDbContext CreateDbContext() => new AppDbContext(_options);
}

/// <summary>
/// Implementación mínima de IWebHostEnvironment para pruebas.
/// </summary>
public class TestWebHostEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "Tests";
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = "Development";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string ContentRootPath { get; set; } = string.Empty;
}

/// <summary>
/// Implementación No-Op de IAuditService para no tocar la base en tests salvo cuando se requiera.
/// </summary>
public class NoOpAuditService : IAuditService
{
    public Task LogAsync(string entityType, string entityId, string action, string userName, object? oldValues = null, object? newValues = null, string? additionalInfo = null)
        => Task.CompletedTask;

    public Task<List<Server.Models.AuditLog>> GetEntityLogsAsync(string entityType, string entityId)
        => Task.FromResult(new List<Server.Models.AuditLog>());

    public Task<List<Server.Models.AuditLog>> GetRecentLogsAsync(int count = 100)
        => Task.FromResult(new List<Server.Models.AuditLog>());
}

/// <summary>
/// Servicio TRM de prueba que devuelve un valor fijo.
/// </summary>
public class TestExchangeRateService : IExchangeRateService
{
    private readonly decimal _value;
    public TestExchangeRateService(decimal value) => _value = value;
    public Task<decimal> GetUsdCopAsync(DateOnly fecha, CancellationToken ct = default) => Task.FromResult(_value);
}
