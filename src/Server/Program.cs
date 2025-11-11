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

var builder = WebApplication.CreateBuilder(args);
// Habilitar Static Web Assets (necesario para servir _content/* de paquetes como MudBlazor en cualquier entorno)
builder.WebHost.UseStaticWebAssets();
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
builder.Services.AddSingleton<ToastService>();
builder.Services.AddSingleton<ModalService>();

// Usar únicamente la factory para evitar conflicto de lifetimes entre DbContextOptions Scoped y Factory Singleton
builder.Services.AddDbContextFactory<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=LamaMedellin;Trusted_Connection=True;TrustServerCertificate=True;"));
// Permitir inyección directa de AppDbContext a partir de la factory (scoped por request/circuito)
builder.Services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
// MudBlazor servicios (dialog, snackbar, resize, etc.)
builder.Services.AddMudServices();
builder.Services.AddHttpClient();
// Configurar HttpClient con BaseAddress para llamadas internas de la API
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
builder.Services.AddHttpContextAccessor();
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

// Identity
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Inicio de sesión y confirmaciones
        options.SignIn.RequireConfirmedAccount = true;
        options.SignIn.RequireConfirmedEmail = false; // Cambiar a true si se habilita confirmación por correo

        // Políticas de contraseña (reforzadas pero razonables)
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 4;

        // Lockout para mitigar fuerza bruta
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
    // Registrar esquema de autenticación de pruebas para permitir Forbid/Challenge sin Identity
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Testing";
        options.DefaultChallengeScheme = "Testing";
        options.DefaultForbidScheme = "Testing";
    }).AddScheme<AuthenticationSchemeOptions, Server.Services.Auth.TestingAuthenticationHandler>("Testing", _ => { });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TesoreroJunta", policy => policy.RequireRole("Tesorero", "Junta"));
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
            MembersSeed.CopyLogo();
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
            await IdentitySeed.SeedAsync(userManager, roleManager);
            Console.WriteLine("✓ Seed completado exitosamente (producción: octubre 2025)");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR en seed: {ex.Message}");
        Console.WriteLine($"Stack: {ex.StackTrace}");
        // No relanzar la excepción para permitir que la app continúe
    }
}

    Console.WriteLine("🚀 Iniciando aplicación...");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Enable developer exception page during tests to aid debugging
if (app.Environment.IsEnvironment("Testing"))
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
// Asegurar carpeta para logs de import
var importLogsPath = Path.Combine(builder.Environment.WebRootPath ?? "wwwroot", "data", "import_logs");
Directory.CreateDirectory(importLogsPath);

// Siempre autenticar y autorizar (en Testing se usa el esquema 'Testing')
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapRazorPages();
app.MapControllers();

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

app.MapFallbackToPage("/_Host");

app.Run();

// Expose the implicit Program class to testing projects
public partial class Program { }
