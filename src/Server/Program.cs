using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Seed;
using Server.Models;
using Server.Services;
using Server.Services.Exchange;
using Server.Services.Import;
using Server.Services.Recibos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Server.Configuration;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Server.Security;
using MudBlazor.Services;
using Microsoft.AspNetCore.OutputCaching;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Microsoft.AspNetCore.RateLimiting;
using Server.Infrastructure;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
// Habilitar Static Web Assets (necesario para servir _content/* de paquetes como MudBlazor en cualquier entorno)
builder.WebHost.UseStaticWebAssets();

// ========== INTEGRACIÓN DE AZURE KEY VAULT ==========
// En producción, usar Managed Identity de Azure para autenticarse sin credenciales hardcodeadas
if (builder.Environment.IsProduction())
{
    try
    {
        var azureOptions = new AzureOptions();
        builder.Configuration.GetSection("Azure").Bind(azureOptions);
        
        if (!string.IsNullOrEmpty(azureOptions.KeyVaultEndpoint) && azureOptions.EnableKeyVault)
        {
            var keyVaultUri = new Uri(azureOptions.KeyVaultEndpoint);
            var credential = new DefaultAzureCredential();
            builder.Configuration.AddAzureKeyVault(keyVaultUri, credential);
            Log.Logger.Information("✓ Key Vault configurado: {KeyVaultEndpoint}", azureOptions.KeyVaultEndpoint);
        }
    }
    catch (Exception ex)
    {
        Log.Logger.Warning(ex, "⚠️ Warning: No se pudo conectar a Key Vault");
        // Continuará usando appsettings en lugar de Key Vault
    }
}

// Registrar opciones de configuración (antes de usarlas)
builder.Services.Configure<AzureOptions>(
    builder.Configuration.GetSection("Azure"));
builder.Services.Configure<SerilogOptions>(
    builder.Configuration.GetSection("Serilog"));
builder.Services.Configure<RateLimitingOptions>(
    builder.Configuration.GetSection("RateLimiting"));

// ========== CONFIGURACIÓN DE SERILOG (Structured Logging) ==========
var serilogOptions = new SerilogOptions();
builder.Configuration.GetSection("Serilog").Bind(serilogOptions);

var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Is(Enum.Parse<LogEventLevel>(serilogOptions.MinimumLevel ?? "Information"))
    .WriteTo.Console(outputTemplate: serilogOptions.OutputTemplate)
    .WriteTo.File(
        serilogOptions.FilePath ?? "Logs/app-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: serilogOptions.RetainedFileCountLimit,
        fileSizeLimitBytes: serilogOptions.FileSizeLimitBytes, // 100 MB
        outputTemplate: serilogOptions.OutputTemplate);

Log.Logger = loggerConfig.CreateLogger();
builder.Host.UseSerilog();

// Configuración de logging mejorada (legacy - compatible con ILogger)
builder.Logging.ClearProviders();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
}

// Permitir deshabilitar servicios hospedados para escenarios de diagnóstico/local
var disableHostedServices =
    builder.Configuration.GetValue<bool>("DisableHostedServices") ||
    string.Equals(Environment.GetEnvironmentVariable("DISABLE_HOSTED_SERVICES"), "true", StringComparison.OrdinalIgnoreCase);

// Configuración de opciones para RTE
builder.Services.Configure<EntidadRTEOptions>(
    builder.Configuration.GetSection("EntidadRTE"));
builder.Services.Configure<BackupOptions>(
    builder.Configuration.GetSection("Backup"));
builder.Services.Configure<TwoFactorEnforcementOptions>(
    builder.Configuration.GetSection("TwoFactorEnforcement"));

// ========== REGISTRAR AZURE BLOB STORAGE CLIENT (Opcional con Fallback) ==========
// Si AzureOptions.UseAzureBlobBackup = true y tenemos StorageConnectionString, registrar BlobServiceClient
var azureOpts = builder.Configuration.GetSection("Azure").Get<AzureOptions>();
if (azureOpts?.UseAzureBlobBackup == true && !string.IsNullOrEmpty(azureOpts.StorageConnectionString))
{
    try
    {
        var blobServiceClient = new BlobServiceClient(azureOpts.StorageConnectionString);
        builder.Services.AddSingleton(blobServiceClient);
        Log.Logger.Information("✓ Azure Blob Storage configurado (contenedor: {ContainerName})", azureOpts.BackupContainerName);
    }
    catch (Exception ex)
    {
        Log.Logger.Warning(ex, "⚠️ Warning: No se pudo configurar Azure Blob Storage");
        // BackupService usará fallback a almacenamiento local
    }
}
else
{
    // Registrar factory que retorna null para permitir fallback local en BackupService
    builder.Services.AddSingleton<BlobServiceClient>(_ => null!);
    Log.Logger.Information("ℹ️ Azure Blob Storage deshabilitado - Se usará almacenamiento local");
}
builder.Services.AddSingleton<ToastService>();
builder.Services.AddSingleton<ModalService>();

// ========== CONFIGURAR DbContext CON MANAGED IDENTITY EN PRODUCCIÓN ==========
// En producción, usar Managed Identity (DefaultAzureCredential) en lugar de Trusted_Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (builder.Environment.IsProduction() && !string.IsNullOrEmpty(connectionString))
{
    // En producción, si la connection string no especifica autenticación, usar Managed Identity
    if (!connectionString.Contains("Authentication=", StringComparison.OrdinalIgnoreCase))
    {
        connectionString += ";Authentication=Active Directory Default;";
    }
    Console.WriteLine("✓ DbContext configurado para Managed Identity");
}

// Usar únicamente la factory para evitar conflicto de lifetimes entre DbContextOptions Scoped y Factory Singleton
builder.Services.AddDbContextFactory<AppDbContext>(opt =>
    opt.UseSqlServer(connectionString ?? "Server=localhost;Database=LamaMedellin;Trusted_Connection=True;TrustServerCertificate=True;"));
// Permitir inyección directa de AppDbContext a partir de la factory (scoped por request/circuito)
builder.Services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options =>
{
    // Errores detallados configurables vía appsettings (DetailedErrors=true) o modo Development
    var detailedErrors = builder.Configuration.GetValue<bool?>("DetailedErrors") ?? builder.Environment.IsDevelopment();
    options.DetailedErrors = detailedErrors;
});
builder.Services.AddControllers();
// Compresión de respuestas HTTP (mejora tiempos de transferencia)
builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = true;
});
// Output Caching para respuestas que cambian poco
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(b => b.Expire(TimeSpan.FromMinutes(5)));
});

// ========== HEALTH CHECKS ==========
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(
        name: "database",
        tags: new[] { "ready", "live" });

// ========== RATE LIMITING ==========
var rateLimitingOpts = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingOptions>() 
    ?? new RateLimitingOptions();
builder.Services.AddRateLimiter(options =>
{
    // Política global: X solicitudes por minuto por IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitingOpts.GlobalRequestsPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    
    // Política de login: X intentos cada Y minutos por IP
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitingOpts.LoginMaxAttempts,
                Window = TimeSpan.FromMinutes(rateLimitingOpts.LoginLimitWindowMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Demasiadas solicitudes. Intenta más tarde.", cancellationToken);
    };
});

// ========== APPLICATION INSIGHTS & TELEMETRY ==========
// Se lee de appsettings (ApplicationInsights:ConnectionString) o Key Vault en producción
var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
    {
        ConnectionString = appInsightsConnectionString,
        EnableAdaptiveSampling = builder.Environment.IsProduction(),
        EnableDependencyTrackingTelemetryModule = true
    });
    var logger = Log.Logger;
    logger.Information("✓ Application Insights configurado para telemetría en {Environment}", builder.Environment.EnvironmentName);
}
else
{
    var logger = Log.Logger;
    logger.Warning("⚠️ Application Insights no configurado - No se enviarán telemetrías a Azure");
}

// MudBlazor servicios (dialog, snackbar, resize, etc.)
builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();

// Registrar el handler de cookies
builder.Services.AddTransient<Server.Infrastructure.CookieForwardingHandler>();

// HttpClient con cookie forwarding para llamadas autenticadas
builder.Services.AddHttpClient("AuthenticatedClient", (sp, client) =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;
    
    if (httpContext != null)
    {
        var request = httpContext.Request;
        client.BaseAddress = new Uri($"{request.Scheme}://{request.Host}");
    }
    else
    {
        client.BaseAddress = new Uri("http://localhost:5000");
    }
})
.AddHttpMessageHandler<Server.Infrastructure.CookieForwardingHandler>();

// HttpClient predeterminado (sin autenticación, para compatibilidad)
builder.Services.AddScoped(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;
    var client = new HttpClient();
    
    // Si estamos en un contexto de request, usar la URL base actual
    if (httpContext != null)
    {
        var request = httpContext.Request;
        client.BaseAddress = new Uri($"{request.Scheme}://{request.Host}");
    }
    else
    {
        // Fallback para escenarios sin HttpContext (ej. background services)
        client.BaseAddress = new Uri("http://localhost:5000");
    }
    
    return client;
});
builder.Services.AddScoped<Server.Services.Reportes.IReportesService, Server.Services.Reportes.ReportesService>();
builder.Services.AddScoped<Server.Services.Reportes.IVerificacionTesoreriaService, Server.Services.Reportes.VerificacionTesoreriaService>();
builder.Services.AddScoped<Server.Services.Donaciones.ICertificadosDonacionService, Server.Services.Donaciones.CertificadosDonacionService>();
// Email
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<Server.Services.Email.IEmailService, Server.Services.Email.SmtpEmailService>();
builder.Services.AddScoped<Server.Services.Email.IEmailDiagnosticsService, Server.Services.Email.EmailDiagnosticsService>();
if (!builder.Environment.IsEnvironment("Testing") && !disableHostedServices)
{
    builder.Services.AddHostedService<Server.Services.Exchange.ExchangeRateHostedService>();
}

// Identity (habilitable en Testing mediante EnableIdentityInTesting=true en appsettings.Test.json)
var enableIdentityInTesting = builder.Configuration.GetValue<bool>("EnableIdentityInTesting");
if (!builder.Environment.IsEnvironment("Testing") || enableIdentityInTesting)
{
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.SignIn.RequireConfirmedEmail = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 4;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders()
        .AddDefaultUI();
}
else
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Testing";
        options.DefaultChallengeScheme = "Testing";
        options.DefaultForbidScheme = "Testing";
    }).AddScheme<AuthenticationSchemeOptions, Server.Services.Auth.TestingAuthenticationHandler>("Testing", _ => { });
}

builder.Services.AddAuthorization(options =>
{
    // Políticas básicas por rol
    options.AddPolicy("TesoreroJunta", policy => policy.RequireRole("Tesorero", "Junta"));
    options.AddPolicy("TesoreroJuntaConsulta", policy => policy.RequireRole("Tesorero", "Junta", "Consulta"));
    options.AddPolicy("AdminTesorero", policy => policy.RequireRole("Admin", "Tesorero"));
    options.AddPolicy("AdminGerente", policy => policy.RequireRole("Admin", "Gerente"));
    options.AddPolicy("AdminGerenteTesorero", policy => policy.RequireRole("Admin", "Gerente", "Tesorero"));
    
    // Política para diagnóstico: solo Admin
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    
    // Policy unificada para Gerencia de Negocios: admite el rol histórico "Gerente" y el rol actual "gerentenegocios"; Admin también tiene acceso
    options.AddPolicy("GerenciaNegocios", policy =>
    {
        policy.RequireAuthenticatedUser();
        // Permitir también al rol Tesorero consultar entidades de Gerencia de Negocios
        // (acciones de creación/edición siguen controladas por AuthorizeView en UI y/o endpoints específicos)
        policy.RequireRole("Admin", "Gerente", "gerentenegocios", "Tesorero");
    });
    
    // Política para exigir que el usuario tenga 2FA habilitado
    options.AddPolicy("Require2FA", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new TwoFactorEnabledRequirement());
    });
    
    // Política combinada para Admin/Tesorero con 2FA obligatorio
    options.AddPolicy("AdminOrTesoreroWith2FA", policy =>
    {
        policy.RequireRole("Admin", "Tesorero");
        policy.Requirements.Add(new TwoFactorEnabledRequirement());
    });
});

// Servicios
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
builder.Services.AddScoped<IRecibosService, RecibosService>();
builder.Services.AddScoped<Server.Services.Egresos.IEgresosService, Server.Services.Egresos.EgresosService>();
builder.Services.AddScoped<Server.Services.Deudores.IDeudoresService, Server.Services.Deudores.DeudoresService>();
builder.Services.AddScoped<Server.Services.Deudores.IDeudoresExportService, Server.Services.Deudores.DeudoresExportService>();
builder.Services.AddScoped<Server.Services.CuentasCobro.ICuentasCobroService, Server.Services.CuentasCobro.CuentasCobroService>();
builder.Services.AddScoped<Server.Services.DashboardService>();
builder.Services.AddScoped<Server.Services.Miembros.IMiembrosService, Server.Services.Miembros.MiembrosService>();
builder.Services.AddScoped<Server.Services.Miembros.IMiembrosExportService, Server.Services.Miembros.MiembrosExportService>();
builder.Services.AddScoped<Server.Services.CierreContable.CierreContableService>();
builder.Services.AddScoped<Server.Services.Exportaciones.ExportacionesService>();

// Servicios de Gerencia de Negocios
builder.Services.AddScoped<Server.Services.Productos.IProductosService, Server.Services.Productos.ProductosService>();
builder.Services.AddScoped<Server.Services.Compras.IComprasService, Server.Services.Compras.ComprasService>();
builder.Services.AddScoped<Server.Services.Ventas.IVentasService, Server.Services.Ventas.VentasService>();
builder.Services.AddScoped<Server.Services.Inventario.IInventarioService, Server.Services.Inventario.InventarioService>();
builder.Services.AddScoped<Server.Services.Proveedores.IProveedoresService, Server.Services.Proveedores.ProveedoresService>();
builder.Services.AddScoped<Server.Services.Clientes.IClientesService, Server.Services.Clientes.ClientesService>();
builder.Services.AddScoped<Server.Services.Cotizaciones.ICotizacionesService, Server.Services.Cotizaciones.CotizacionesService>();
builder.Services.AddScoped<IPresupuestosService, PresupuestosService>();
builder.Services.AddScoped<Server.Services.ConciliacionBancaria.IConciliacionBancariaService, Server.Services.ConciliacionBancaria.ConciliacionBancariaService>();

// Servicios nuevos
builder.Services.AddScoped<Server.Services.Auth.ICurrentUserService, Server.Services.Auth.CurrentUserService>();
builder.Services.AddScoped<Server.Services.Auth.ITwoFactorAuditService, Server.Services.Auth.TwoFactorAuditService>();
builder.Services.AddScoped<Server.Services.Audit.IAuditService, Server.Services.Audit.AuditService>();
builder.Services.AddScoped<Server.Services.Export.ICsvExportService, Server.Services.Export.CsvExportService>();
builder.Services.AddScoped<Server.Services.Backup.IBackupService, Server.Services.Backup.BackupService>();

// Servicios de UI
builder.Services.AddScoped<Server.Services.UI.IThemeService, Server.Services.UI.ThemeService>();

// Autorización: handler para políticas de 2FA (registrar sólo fuera de Testing para evitar dependencias de Identity)
// Cambio a Scoped porque TwoFactorEnabledHandler consume UserManager<ApplicationUser> (scoped)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddScoped<IAuthorizationHandler, TwoFactorEnabledHandler>();
}

// Servicios en segundo plano
if (!builder.Environment.IsEnvironment("Testing") && !disableHostedServices)
{
    builder.Services.AddHostedService<Server.Services.Backup.BackupHostedService>();
}

var app = builder.Build();

// Ejecutar seed (omitido en pruebas para permitir WebApplicationFactory con DB en memoria)
// NOTA: SEED DE DATOS FINANCIEROS DESHABILITADO - Base de datos limpia
if (!app.Environment.IsEnvironment("Testing"))
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
            await TreasurySeed.SeedAsync(db); // Conceptos (mantener)
            await MembersSeed.SeedAsync(db);  // Miembros (mantener)
            // await Recibos2025Seed.SeedAsync(db);  // ❌ DESHABILITADO - datos de prueba
            await HistoricoTesoreria2025Seed.SeedAsync(db);  // ✅ HABILITADO - Saldo inicial octubre + movimientos producción
            await ProductosSeed.SeedAsync(db);  // ✅ Productos de ejemplo
            await ProductosSeed.SeedVentaEjemploAsync(db);  // ✅ Venta de ejemplo para cuenta de cobro
            await GerenciaNegociosSeed.SeedClienteEjemploAsync(db);  // ✅ Cliente demo para E2E
            await GerenciaNegociosSeed.SeedCompraEjemploAsync(db);  // ✅ Compra demo para E2E
            await CertificadosDonacionSeed.SeedAsync(db);  // ✅ Certificados de donación de ejemplo
            MembersSeed.CopyLogo();
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
            // Deshabilitar 2FA para cuentas semilla en Development salvo que se defina la variable DISABLE_2FA_SEED=false
            var enable2FAForSeed = !app.Environment.IsDevelopment() &&
                                   !string.Equals(Environment.GetEnvironmentVariable("DISABLE_2FA_SEED"), "true", StringComparison.OrdinalIgnoreCase) &&
                                   !string.Equals(Environment.GetEnvironmentVariable("DISABLE_2FA_SEED"), "1", StringComparison.OrdinalIgnoreCase);
            await IdentitySeed.SeedAsync(userManager, roleManager, enable2FAForSeed);
            Log.Logger.Information("✓ Seed completado exitosamente (producción: octubre 2025) - 2FASeed={TwoFaStatus}", 
                enable2FAForSeed ? "ON" : "OFF");
        }
    }
    catch (Exception ex)
    {
        Log.Logger.Error(ex, "❌ ERROR en seed");
        // No relanzar la excepción para permitir que la app continúe
    }
}

    Log.Logger.Information("🚀 Iniciando aplicación en ambiente {Environment}", builder.Environment.EnvironmentName);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // HSTS: HTTP Strict Transport Security (requiere HTTPS en producción)
    app.UseHsts();
}

// Enable developer exception page during tests to aid debugging
if (app.Environment.IsEnvironment("Testing"))
{
    app.UseDeveloperExceptionPage();
}

// Redirección HTTPS en producción
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Middleware de compresión (debe ir antes de StaticFiles)
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
// Output cache (antes de mapear endpoints)
app.UseOutputCache();

// ========== SECURITY HEADERS MIDDLEWARE (Solo en Producción) ==========
if (!app.Environment.IsDevelopment())
{
    app.UseSecurityHeaders();
}

// ========== RATE LIMITING MIDDLEWARE ==========
app.UseRateLimiter();
// Asegurar carpeta para logs de import
var importLogsPath = Path.Combine(builder.Environment.WebRootPath ?? "wwwroot", "data", "import_logs");
Directory.CreateDirectory(importLogsPath);

// Siempre autenticar y autorizar (en Testing se usa el esquema 'Testing')
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapRazorPages();
app.MapControllers();

// ========== HEALTH CHECKS ENDPOINTS ==========
// /health - General health status
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
}).AllowAnonymous();

// /health/ready - Readiness probe (includes database check)
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
}).AllowAnonymous();

// /health/live - Liveness probe (basic system health)
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("live"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
}).AllowAnonymous();

// Endpoint para descargar PDF del recibo
// PDF endpoint is provided by RecibosController to avoid duplicate route mappings

// Página pública de verificación (HTML mínimo)
app.MapGet("/recibo/{id:guid}/verificacion", async (Guid id, Server.Data.AppDbContext db) =>
{
    var recibo = await db.Recibos
        .Where(r => r.Id == id)
        .Select(r => new
        {
            r.Id,
            r.Serie,
            r.Ano,
            r.Consecutivo,
            r.FechaEmision,
            r.TotalCop,
            r.Estado
        })
        .FirstOrDefaultAsync();

    if (recibo is null) return Results.NotFound("Recibo no encontrado");

    var html = $"<html><head><title>Verificación Recibo</title></head><body><h2>Recibo {recibo.Serie}-{recibo.Ano}-{recibo.Consecutivo:D6}</h2><p>Fecha: {recibo.FechaEmision:yyyy-MM-dd}</p><p>Total COP: {recibo.TotalCop:N0}</p><p>Estado: {recibo.Estado}</p></body></html>";
    return Results.Content(html, "text/html");
});

// Página pública de verificación de certificado de donación (HTML mínimo)
app.MapGet("/certificado/{id:guid}/verificacion", async (Guid id, Server.Data.AppDbContext db) =>
{
    var cert = await db.CertificadosDonacion
        .Where(c => c.Id == id)
        .Select(c => new
        {
            c.Id,
            c.Ano,
            c.Consecutivo,
            c.FechaEmision,
            c.NombreDonante,
            c.IdentificacionDonante,
            c.ValorDonacionCOP,
            c.Estado,
            c.RazonAnulacion,
            c.FechaAnulacion
        })
        .FirstOrDefaultAsync();

    if (cert is null) return Results.NotFound("Certificado no encontrado");

    // Enmascarar identificación del donante (mostrar últimos 3 dígitos si es >= 6)
    string MaskId(string idStr)
    {
        if (string.IsNullOrWhiteSpace(idStr)) return "";
        var clean = new string(idStr.Where(char.IsDigit).ToArray());
        if (clean.Length <= 3) return new string('*', clean.Length);
        var visible = clean[^3..];
        return new string('*', clean.Length - 3) + visible;
    }

    var maskedId = MaskId(cert.IdentificacionDonante ?? "");
    var nombreDonante = WebUtility.HtmlEncode(cert.NombreDonante ?? "");
    var estadoColor = cert.Estado == EstadoCertificado.Emitido ? "#059669" : 
                      cert.Estado == EstadoCertificado.Anulado ? "#dc2626" : "#f59e0b";
    var estadoBg = cert.Estado == EstadoCertificado.Emitido ? "#d1fae5" : 
                   cert.Estado == EstadoCertificado.Anulado ? "#fee2e2" : "#fef3c7";
    
    var anuladoSection = cert.Estado == EstadoCertificado.Anulado && !string.IsNullOrEmpty(cert.RazonAnulacion)
        ? $"<div style='background: #fee2e2; padding: 12px; border-radius: 6px; margin-top: 16px;'>" +
          $"<p style='color: #991b1b; font-weight: bold; margin: 0 0 8px 0;'>Motivo de anulación:</p>" +
          $"<p style='color: #7f1d1d; margin: 0;'>{WebUtility.HtmlEncode(cert.RazonAnulacion)}</p>" +
          $"<p style='color: #7f1d1d; margin: 8px 0 0 0; font-size: 0.875rem;'>Fecha: {cert.FechaAnulacion:yyyy-MM-dd HH:mm}</p>" +
          $"</div>"
        : "";
    
    var html = $"<html><head><title>Verificación Certificado Donación</title>" +
               "<style>body {{ font-family: Arial, sans-serif; max-width: 600px; margin: 40px auto; padding: 20px; }}</style></head>" +
               $"<body><h2 style='color: {estadoColor};'>Certificado CD-{cert.Ano}-{cert.Consecutivo:D5}</h2>" +
               $"<div style='background: {estadoBg}; padding: 8px 16px; border-radius: 6px; display: inline-block; margin-bottom: 16px;'>" +
               $"<strong style='color: {estadoColor};'>Estado: {cert.Estado}</strong></div>" +
               $"<p><strong>Fecha emisión:</strong> {cert.FechaEmision:yyyy-MM-dd}</p>" +
               $"<p><strong>Donante:</strong> {nombreDonante} - ID: {maskedId}</p>" +
               $"<p><strong>Valor donación COP:</strong> {cert.ValorDonacionCOP:N0}</p>" +
               anuladoSection +
               $"<p style='margin-top: 24px; font-size: 0.875rem; color: #64748b;'>Fundación L.A.M.A. Medellín - Certificado oficial de donación</p>" +
               $"</body></html>";
    return Results.Content(html, "text/html");
});

// Endpoint público de salud/performance (solo lectura) para validación de tiempos sin autenticación
// Ejecuta consultas típicas de lectura con AsNoTracking y retorna conteos y tiempo total.
app.MapGet("/api/health/perf", async (IDbContextFactory<AppDbContext> dbFactory) =>
{
    using var db = await dbFactory.CreateDbContextAsync();

    var sw = System.Diagnostics.Stopwatch.StartNew();

    var conceptosCount = await db.Conceptos.AsNoTracking().CountAsync();
    var productosCount = await db.Productos.AsNoTracking().CountAsync();
    var activosCount = await db.Productos.AsNoTracking().Where(p => p.Activo).CountAsync();
    var bajoStockCount = await db.Productos.AsNoTracking().Where(p => p.StockActual < p.StockMinimo).CountAsync();

    sw.Stop();

    return Results.Ok(new
    {
        conceptosCount,
        productosCount,
        activosCount,
        bajoStockCount,
        elapsedMs = sw.ElapsedMilliseconds
    });
}).AllowAnonymous();

app.MapFallbackToPage("/_Host");

// Endpoints espejo (solo Development) para validar performance sin autenticación
if (app.Environment.IsDevelopment())
{
    // Conceptos - listado completo (con caché de salida base)
    app.MapGet("/dev/api/conceptos", async (IDbContextFactory<AppDbContext> dbFactory) =>
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var conceptos = await db.Conceptos
            .AsNoTracking()
            .OrderBy(c => c.EsIngreso ? 0 : 1)
            .ThenBy(c => c.Nombre)
            .Select(c => new
            {
                c.Id,
                c.Codigo,
                c.Nombre,
                c.Descripcion,
                c.EsIngreso,
                c.EsRecurrente,
                Moneda = c.Moneda.ToString(),
                c.PrecioBase,
                Periodicidad = c.Periodicidad.ToString()
            })
            .ToListAsync();
        return Results.Ok(conceptos);
    }).AllowAnonymous();

    // Conceptos - simples
    app.MapGet("/dev/api/conceptos/simples", async (IDbContextFactory<AppDbContext> dbFactory) =>
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var conceptos = await db.Conceptos
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new
            {
                c.Id,
                c.Codigo,
                c.Nombre,
                c.EsIngreso
            })
            .ToListAsync();
        return Results.Ok(conceptos);
    }).AllowAnonymous();

    // Productos - todos
    app.MapGet("/dev/api/productos", async (IDbContextFactory<AppDbContext> dbFactory) =>
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var productos = await db.Productos
            .AsNoTracking()
            .OrderBy(p => p.Nombre)
            .Select(p => new
            {
                p.Id,
                p.Codigo,
                p.Nombre,
                p.Tipo,
                p.PrecioVentaCOP,
                p.PrecioVentaUSD,
                p.StockActual,
                p.StockMinimo,
                p.Activo
            })
            .ToListAsync();
        return Results.Ok(productos);
    }).AllowAnonymous();

    // Productos - activos
    app.MapGet("/dev/api/productos/activos", async (IDbContextFactory<AppDbContext> dbFactory) =>
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var productos = await db.Productos
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new
            {
                p.Id,
                p.Codigo,
                p.Nombre,
                p.Tipo,
                p.PrecioVentaCOP,
                p.PrecioVentaUSD,
                p.StockActual,
                p.StockMinimo,
                p.Activo
            })
            .ToListAsync();
        return Results.Ok(productos);
    }).AllowAnonymous();

    // Productos - bajo stock
    app.MapGet("/dev/api/productos/bajo-stock", async (IDbContextFactory<AppDbContext> dbFactory) =>
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var productos = await db.Productos
            .AsNoTracking()
            .Where(p => p.StockActual < p.StockMinimo)
            .OrderBy(p => p.Nombre)
            .Select(p => new
            {
                p.Id,
                p.Codigo,
                p.Nombre,
                p.Tipo,
                p.PrecioVentaCOP,
                p.PrecioVentaUSD,
                p.StockActual,
                p.StockMinimo,
                p.Activo
            })
            .ToListAsync();
        return Results.Ok(productos);
    }).AllowAnonymous();
}

app.Run();

// Expose the implicit Program class to testing projects
public partial class Program { }
