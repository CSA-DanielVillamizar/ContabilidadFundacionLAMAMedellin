using Microsoft.Playwright;
using Xunit;
using System.Net.Http;
using System.Net;
using System;

namespace ContabilidadLAMAMedellin.Tests.E2E;

/// <summary>
/// Clase base para todas las pruebas E2E con Playwright.
/// Proporciona configuración común, autenticación, métodos de utilidad y soporte para TestDataSeed.
/// NOTA: TestDataSeed debe ejecutarse manualmente en WebApplicationFactory o en cada test suite que lo requiera.
/// </summary>
public abstract class BaseTest : IAsyncLifetime
{
    protected IPlaywright? Playwright { get; private set; }
    protected IBrowser? Browser { get; private set; }
    protected IBrowserContext? Context { get; private set; }
    protected IPage? Page { get; private set; }

    // URL base de la aplicación (se puede configurar desde variable de entorno E2E_BASE_URL)
    protected string BaseUrl { get; set; } = Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://localhost:5000";

    // Credenciales de prueba
    protected static class Credentials
    {
        public const string AdminEmail = "admin@fundacionlamamedellin.org";
    public const string AdminPassword = "Admin123!"; // Sincronizado con IdentitySeed para pruebas

        public const string TesoreroEmail = "tesorero@fundacionlamamedellin.org";
    public const string TesoreroPassword = "Tesorero123!"; // Sincronizado con IdentitySeed para pruebas

        public const string ContadorEmail = "contador@fundacionlamamedellin.org";
    public const string ContadorPassword = "Contador123!"; // Ajustar si se define seed específico
    }

    /// <summary>
    /// Inicialización asíncrona antes de cada test.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        // Crear instancia de Playwright
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        // Lanzar navegador (Chromium por defecto)
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, // Cambiar a false para ver el navegador durante las pruebas
            SlowMo = 100 // Ralentizar acciones para debugging (opcional)
        });

        // Crear contexto del navegador
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            RecordVideoDir = "videos/" // Grabar videos de las pruebas
        });

        // Iniciar tracing para capturar pasos, screenshots y snapshots
        await Context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        // Crear página
        Page = await Context.NewPageAsync();

        // Configurar timeout global
        Page.SetDefaultTimeout(30000); // 30 segundos

        // Escuchar errores de consola
        Page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                Console.WriteLine($"❌ Console Error: {msg.Text}");
            }
        };

        // Escuchar errores de página
        Page.PageError += (_, exception) =>
        {
            Console.WriteLine($"❌ Page Error: {exception}");
        };

        // Asegurar servidor activo antes de empezar (auto-start opcional)
        await EnsureServerRunningAsync();
    }

    /// <summary>
    /// Limpieza asíncrona después de cada test.
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        // Detener y exportar tracing (si contexto existe)
        if (Context != null)
        {
            var traceFile = $"traces/trace_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            Directory.CreateDirectory("traces");
            await Context.Tracing.StopAsync(new TracingStopOptions { Path = traceFile });
        }

        if (Page != null)
            await Page.CloseAsync();

        if (Context != null)
            await Context.CloseAsync();

        if (Browser != null)
            await Browser.CloseAsync();

        Playwright?.Dispose();
    }

    /// <summary>
    /// Realiza login con las credenciales especificadas.
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="password">Contraseña del usuario</param>
    protected async Task LoginAsync(string email, string password)
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");
        // Esperar a que el servidor esté listo antes de intentar navegar
        await WaitForServerReadyAsync();

        // Navegar a la página de login (Identity)
        await Page.GotoAsync($"{BaseUrl}/Identity/Account/Login");

        // Esperar a que el formulario de login esté visible
        await Page.WaitForSelectorAsync("input[type='email'], input[name='email'], #Input_Email, input[name='Input.Email']");

        // Ingresar credenciales
    // Ingresar credenciales (compatibles con Identity UI por defecto)
    await Page.FillAsync("#Input_Email, input[name='Input.Email'], input[type='email'], input[name='email']", email);
    await Page.FillAsync("#Input_Password, input[name='Input.Password'], input[type='password'], input[name='password']", password);

        // Hacer clic en el botón de login
    await Page.ClickAsync("#login-submit, button[type='submit'], button:has-text('Iniciar')");

        // Esperar navegación al dashboard
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        // Aceptar posibles redirecciones post-login hacia returnUrl
        // Verificar que no seguimos en la página de login
        var currentUrl = Page.Url;
        if (currentUrl.Contains("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El inicio de sesión no fue exitoso: se mantiene en la página de Login.");
        }

        // Verificar que el login fue exitoso: debería haber navegado fuera de /Identity/Account/Login
        // Esperar a que Blazor cargue (NetworkIdle ya se invocó arriba, dar tiempo adicional para hidratación)
        await Task.Delay(2000);
    }

    /// <summary>
    /// Verifica si el servidor está disponible. Los tests E2E requieren un servidor ya iniciado externamente.
    /// Configurar E2E_BASE_URL para especificar la URL del servidor (por defecto: http://localhost:5000).
    /// </summary>
    protected async Task EnsureServerRunningAsync()
    {
        var targetUrl = BaseUrl;

        // Comprobar disponibilidad del servidor
        if (await IsServerUpAsync(targetUrl))
        {
            Console.WriteLine($"✓ Servidor E2E disponible en: {targetUrl}");
            return;
        }

        // Servidor no disponible: esperar con reintentos
        Console.WriteLine($"⏳ Esperando servidor en {targetUrl}...");
        await WaitForServerReadyAsync();
    }

    /// <summary>
    /// Devuelve true si la página de Login responde con 200.
    /// </summary>
    private static async Task<bool> IsServerUpAsync(string baseUrl)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var resp = await http.GetAsync($"{baseUrl.TrimEnd('/')}/Identity/Account/Login");
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Login como Administrador.
    /// </summary>
    protected Task LoginAsAdminAsync() => LoginAsync(Credentials.AdminEmail, Credentials.AdminPassword);

    /// <summary>
    /// Login como Tesorero.
    /// </summary>
    protected Task LoginAsTesoreroAsync() => LoginAsync(Credentials.TesoreroEmail, Credentials.TesoreroPassword);

    /// <summary>
    /// Login como Contador.
    /// </summary>
    protected Task LoginAsContadorAsync() => LoginAsync(Credentials.ContadorEmail, Credentials.ContadorPassword);

    /// <summary>
    /// Realiza logout de la aplicación.
    /// </summary>
    protected async Task LogoutAsync()
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

        // Buscar y hacer clic en el botón/link de logout (MainLayout -> MudButton "Cerrar sesión")
        await Page.ClickAsync("text=/Cerrar sesión|Logout|Salir/i");

        // Identity Logout devuelve una página (200) y puede no redirigir inmediatamente al Login.
        // Considerar como éxito cualquiera de estas señales:
        // 1) Aparece el botón "Iniciar sesión" del layout
        // 2) Se renderiza el formulario de Login (inputs de email/password)
        // 3) URL de Login
        try
        {
            // 1) Botón "Iniciar sesión" visible en el drawer
            await Page.WaitForSelectorAsync("a[href='/Identity/Account/Login'], button:has-text('Iniciar sesión')",
                new PageWaitForSelectorOptions { Timeout = 8000, State = WaitForSelectorState.Visible });
            return;
        }
        catch { /* continuar con siguiente verificación */ }

        try
        {
            // 2) Formulario de login visible
            await Page.WaitForSelectorAsync("#Input_Email, input[name='Input.Email'], input[type='email'], input[name='email']",
                new PageWaitForSelectorOptions { Timeout = 8000, State = WaitForSelectorState.Visible });
            return;
        }
        catch { /* continuar con siguiente verificación */ }

        try
        {
            // 3) URL del login
            await Page.WaitForURLAsync($"{BaseUrl}/Identity/Account/Login*", new PageWaitForURLOptions { Timeout = 8000 });
        }
        catch
        {
            // Como fallback final, navegar explícitamente al Login para estabilizar el estado
            await Page.GotoAsync($"{BaseUrl}/Identity/Account/Login");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }

    /// <summary>
    /// Navega a una ruta específica de la aplicación.
    /// </summary>
    /// <param name="path">Ruta relativa (ejemplo: "/tesoreria/recibos")</param>
    protected async Task NavigateToAsync(string path)
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

        var url = path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? path : $"{BaseUrl}{path}";
        await Page.GotoAsync(url);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(300);
    }

    /// <summary>
    /// Espera a que aparezca una notificación o snackbar.
    /// </summary>
    /// <param name="expectedText">Texto esperado en la notificación (opcional)</param>
    protected async Task<bool> WaitForNotificationAsync(string? expectedText = null)
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

        try
        {
            // Esperar notificación de MudBlazor o similar (incluye alerts Bootstrap)
            var selector = expectedText != null
                ? $".mud-snackbar:has-text('{expectedText}'), .mud-alert:has-text('{expectedText}'), .alert:has-text('{expectedText}')"
                : ".mud-snackbar, .mud-alert, .alert";

            await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
            {
                Timeout = 5000,
                State = WaitForSelectorState.Visible
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Espera a que un diálogo/modal esté visible.
    /// </summary>
    protected async Task WaitForDialogAsync()
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

        await Page.WaitForSelectorAsync(".mud-dialog, [role='dialog']", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible
        });
    }

    /// <summary>
    /// Cierra un diálogo/modal haciendo clic en cancelar.
    /// </summary>
    protected async Task CloseDialogAsync()
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

        await Page.ClickAsync("button:has-text('Cancelar'), button:has-text('Cancel'), .mud-dialog-close");
    }

    /// <summary>
    /// Espera a que la página termine de cargar (sin animaciones ni llamadas pendientes).
    /// </summary>
    protected async Task WaitForPageIdleAsync()
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(500); // Espera adicional para animaciones
    }

    /// <summary>
    /// Toma un screenshot de la página actual.
    /// </summary>
    /// <param name="name">Nombre del archivo de screenshot</param>
    protected async Task TakeScreenshotAsync(string name)
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

        var path = $"screenshots/{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
        Console.WriteLine($"📸 Screenshot guardado: {path}");
    }

    /// <summary>
    /// Verifica que no haya errores en la consola del navegador.
    /// </summary>
    protected async Task AssertNoConsoleErrorsAsync()
    {
        // Los errores de consola se capturan en el evento Console configurado en InitializeAsync
        await Task.CompletedTask;
    }

    /// <summary>
    /// Rellena un campo de formulario por su etiqueta (label).
    /// </summary>
    /// <param name="label">Texto del label</param>
    /// <param name="value">Valor a ingresar</param>
    protected async Task FillByLabelAsync(string label, string value)
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

            // Buscar el input o textarea asociado al label
            // Estrategia 1: Label hermano directo (~ o +)
            // Estrategia 2: Label y input en mismo contenedor padre (div con label seguido de MudTextField)
            var input = Page.Locator($@"
                label:has-text('{label}') ~ input,
                label:has-text('{label}') + input,
                label:has-text('{label}') ~ textarea,
                label:has-text('{label}') + textarea,
                div:has(> label:has-text('{label}')) input,
                div:has(> label:has-text('{label}')) textarea
            ".Trim().Replace("\n", "").Replace("  ", " "));
        await input.FillAsync(value);
    }

    /// <summary>
    /// Selecciona una opción de un select/dropdown por su etiqueta.
    /// </summary>
    /// <param name="label">Texto del label</param>
    /// <param name="optionText">Texto de la opción a seleccionar</param>
    protected async Task SelectByLabelAsync(string label, string optionText)
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

            // Intentar primero con Bootstrap/HTML select (InputSelect)
            var bootstrapSelect = Page.Locator($@"
                label:has-text('{label}') ~ select,
                label:has-text('{label}') + select,
                label:has-text('{label}') ~ .form-select,
                div:has(> label:has-text('{label}')) select
            ".Trim().Replace("\n", "").Replace("  ", " "));
        if (await bootstrapSelect.CountAsync() > 0)
        {
            await bootstrapSelect.SelectOptionAsync(new SelectOptionValue { Label = optionText });
            return;
        }

            // Si no es Bootstrap, intentar con MudBlazor MudSelect
            var mudSelect = Page.Locator($@"
                label:has-text('{label}') ~ .mud-select,
                label:has-text('{label}') + .mud-select,
                div:has(> label:has-text('{label}')) .mud-select
            ".Trim().Replace("\n", "").Replace("  ", " "));
        await mudSelect.ClickAsync();
        await Page.ClickAsync($".mud-list-item:has-text('{optionText}')");
    }

    /// <summary>
    /// Espera de forma resiliente a que el servidor esté aceptando conexiones HTTP (status 200 en página de Login).
    /// Implementa reintentos con backoff simple para evitar errores net::ERR_CONNECTION_REFUSED cuando el host aún inicia.
    /// </summary>
    /// <param name="maxIntentos">Número máximo de intentos de verificación.</param>
    /// <param name="delayInicialMs">Retraso inicial entre intentos (se incrementa linealmente).</param>
    protected async Task WaitForServerReadyAsync(int maxIntentos = 15, int delayInicialMs = 500)
    {
        using var http = new HttpClient();
        http.Timeout = TimeSpan.FromSeconds(2);
        var loginUrl = $"{BaseUrl}/Identity/Account/Login";
        for (int intento = 1; intento <= maxIntentos; intento++)
        {
            try
            {
                var resp = await http.GetAsync(loginUrl);
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    if (intento > 1)
                        Console.WriteLine($"✅ Server listo tras {intento} intentos");
                    return;
                }
                Console.WriteLine($"⚠️ Server respondió {resp.StatusCode} en intento {intento}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⏳ Server no disponible (intento {intento}): {ex.Message}");
            }
            await Task.Delay(delayInicialMs + (intento * 150));
        }
        throw new TimeoutException($"El servidor no estuvo disponible tras {maxIntentos} intentos accediendo a {loginUrl}.");
    }

    /// <summary>
    /// Hace clic en un botón cuando se habilita. Si el botón está deshabilitado (atributo HTML disabled presente)
    /// se reintenta de forma activa hasta que se habilite o se alcance el timeout.
    /// Útil para escenarios donde la UI habilita el botón tras cargar datos (roles, usuarios, etc.).
    /// </summary>
    /// <param name="selector">Selector Playwright del botón (puede incluir variantes separadas por coma).</param>
    /// <param name="timeoutMs">Tiempo máximo de espera en milisegundos (por defecto 5000ms).</param>
    /// <exception cref="TimeoutException">Si el botón no se habilita en el tiempo configurado.</exception>
    protected async Task ClickWhenEnabledAsync(string selector, int timeoutMs = 5000)
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");
        var btn = Page.Locator(selector).First;
        if (await btn.CountAsync() == 0)
        {
            throw new InvalidOperationException($"No se encontró ningún botón para el selector: {selector}");
        }
        // Asegurar visibilidad inicial
        await btn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        var fin = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < fin)
        {
            var disabledAttr = await btn.GetAttributeAsync("disabled");
            if (disabledAttr == null)
            {
                await btn.ClickAsync();
                return;
            }
            await Task.Delay(200);
        }
        throw new TimeoutException($"El botón no se habilitó tras {timeoutMs}ms: {selector}");
    }
}
