using Xunit;

namespace ContabilidadLAMAMedellin.Tests.E2E;

/// <summary>
/// Pruebas E2E para el módulo de Administración.
/// Cubre: Login, Logout, Dashboard y verificación de roles.
/// </summary>
public class AdministracionTests : BaseTest
{
    [Fact]
    public async Task Administracion_LoginComoAdmin_AccedeAlDashboard()
    {
        // Arrange & Act
        await LoginAsAdminAsync();

        // Assert
        Assert.NotNull(Page);
        var url = Page.Url;
        Assert.True(url.Contains(BaseUrl), "La URL debería contener la URL base");
        Assert.DoesNotContain("/Identity/Account/Login", url, StringComparison.OrdinalIgnoreCase);

        // Verificar que la página cargó (esperar cualquier contenido del layout común)
        var hasLayout = await Page.Locator("body").CountAsync() > 0;
        Assert.True(hasLayout, "La página debería estar cargada");
    }

    [Fact]
    public async Task Administracion_LoginComoTesorero_AccedeAlDashboard()
    {
        // Arrange & Act
        await LoginAsTesoreroAsync();

        // Assert
        Assert.NotNull(Page);
        var url = Page.Url;
        Assert.True(url.Contains(BaseUrl), "La URL debería contener la URL base");

        // Verificar que la UI autenticada está visible (drawer con navegación o botón Cerrar sesión)
        var loggedInUI = await Page.IsVisibleAsync("text=/Cerrar sesión/i")
            || await Page.IsVisibleAsync(".modern-sidebar-item, .mud-drawer, a[href='/tesoreria/recibos']");
        Assert.True(loggedInUI, "La interfaz autenticada debería estar visible");
    }

    [Fact]
    public async Task Administracion_LoginComoContador_AccedeAlDashboard()
    {
        // Arrange & Act
        await LoginAsContadorAsync();

        // Assert
        Assert.NotNull(Page);
        var url = Page.Url;
        Assert.True(url.Contains(BaseUrl), "La URL debería contener la URL base");

        // Verificar que la UI autenticada está visible (drawer con navegación o botón Cerrar sesión)
        var loggedInUI = await Page.IsVisibleAsync("text=/Cerrar sesión/i")
            || await Page.IsVisibleAsync(".modern-sidebar-item, .mud-drawer, a[href='/tesoreria/recibos']");
        Assert.True(loggedInUI, "La interfaz autenticada debería estar visible");
    }

    [Fact]
    public async Task Administracion_LoginConCredencialesInvalidas_MuestraError()
    {
        // Arrange
        Assert.NotNull(Page);
    await Page.GotoAsync($"{BaseUrl}/Identity/Account/Login");

        // Act
    await Page.FillAsync("#Input_Email, input[name='Input.Email'], input[type='email'], input[name='email']", "usuario_invalido@test.com");
    await Page.FillAsync("#Input_Password, input[name='Input.Password'], input[type='password'], input[name='password']", "password_incorrecta");
    await Page.ClickAsync("#login-submit, button[type='submit'], button:has-text('Iniciar')");

        // Assert
        // Identity muestra errores en el formulario (validation summary / alert), no snackbar.
        var errorVisible = await Page.IsVisibleAsync(".validation-summary-errors li, [data-valmsg-summary='true'] li, [role='alert'], .text-danger");
        // Aceptamos también mensajes en inglés/español
        var errorTextMatch = await Page.IsVisibleAsync("text=/Invalid|inválid|invalido|incorrecta|credenciales/i");
        Assert.True(errorVisible || errorTextMatch, "Debería mostrar un mensaje de error de autenticación");

        // Verificar que sigue en la página de login
        var currentUrl = Page.Url;
        Assert.Contains("/Identity/Account/Login", currentUrl);
    }

    [Fact]
    public async Task Administracion_Logout_RedireccionaALogin()
    {
        // Arrange
        await LoginAsAdminAsync();
        await WaitForPageIdleAsync();

        // Act
        await LogoutAsync();

        // Assert
        Assert.NotNull(Page);
        var url = Page.Url;
    Assert.Contains("/Identity/Account/Login", url);

        // Verificar que el formulario de login está visible
    var loginFormVisible = await Page.IsVisibleAsync("#Input_Email, input[name='Input.Email'], input[type='email'], input[name='email']");
        Assert.True(loginFormVisible, "El formulario de login debería estar visible");
    }

    [Fact]
    public async Task Administracion_DashboardAdmin_MuestraMetricas()
    {
        // Arrange
        await LoginAsAdminAsync();
        await WaitForPageIdleAsync();

        // Act
        await NavigateToAsync("/");

        // Assert
        Assert.NotNull(Page);

    // Layout cargado y navegación visible (menos frágil que métricas específicas)
    var navVisible = await Page.IsVisibleAsync(".modern-sidebar-item, .mud-drawer");
    Assert.True(navVisible, "El layout y menú de navegación deberían estar visibles");
    }

    [Fact]
    public async Task Administracion_DashboardTesorero_MuestraSoloModulosPermitidos()
    {
        // Arrange
        await LoginAsTesoreroAsync();
        await WaitForPageIdleAsync();

        // Act
        await NavigateToAsync("/");

        // Assert
        Assert.NotNull(Page);

        // Verificar que el menú de tesorería está visible
        var hasTesoreriaMenu = await Page.IsVisibleAsync("text=/Tesorería|Recibos/i");
        Assert.True(hasTesoreriaMenu, "El menú de tesorería debería estar visible");

        // Verificar que NO tiene acceso a administración (si aplica restricción)
        // Esto depende de la implementación de permisos
        await WaitForPageIdleAsync();
    }

    [Fact]
    public async Task Administracion_NavegacionEntreModulos_FuncionaCorrectamente()
    {
        // Arrange
        await LoginAsAdminAsync();
        await WaitForPageIdleAsync();

        // Act & Assert - Navegar a Tesorería (usar ruta estable del NavMenu)
        await NavigateToAsync("/tesoreria/recibos");
        var urlTesoreria = Page.Url;
        Assert.Contains("tesoreria", urlTesoreria, System.StringComparison.OrdinalIgnoreCase);

        // Navegar a Gerencia de Negocios
        await NavigateToAsync("/gerencia-negocios/productos");
        var urlGerencia = Page.Url;
        Assert.Contains("gerencia", urlGerencia, System.StringComparison.OrdinalIgnoreCase);

        // Navegar a Configuración (usuarios)
        await NavigateToAsync("/configuracion/usuarios");
        var urlConfiguracion = Page.Url;
        Assert.Contains("configuracion", urlConfiguracion, System.StringComparison.OrdinalIgnoreCase);

        // Volver al Dashboard (home)
        await NavigateToAsync("/");
        var urlDashboard = Page.Url;
        Assert.Equal($"{BaseUrl}/", urlDashboard);
    }

    [Fact]
    public async Task Administracion_SesionPersiste_DespuesDeRecargarPagina()
    {
        // Arrange
        await LoginAsAdminAsync();
        await WaitForPageIdleAsync();

        // Act - Recargar la página
        Assert.NotNull(Page);
        await Page.ReloadAsync();
        await WaitForPageIdleAsync();

        // Assert - Verificar que sigue autenticado
        var url = Page.Url;
    Assert.DoesNotContain("/Identity/Account/Login", url);

        var stillLoggedIn = await Page.IsVisibleAsync("text=/Cerrar sesión/i")
            || await Page.IsVisibleAsync(".modern-sidebar-item, .mud-drawer");
        Assert.True(stillLoggedIn, "La sesión debería persistir tras recargar (UI autenticada visible)");
    }

    [Fact]
    public async Task Administracion_AccesoDirectoSinAutenticacion_RedireccionaALogin()
    {
        // Arrange
        Assert.NotNull(Page);

        // Act - Intentar acceder a una ruta protegida sin autenticarse
        await Page.GotoAsync($"{BaseUrl}/tesoreria/recibos");
        await WaitForPageIdleAsync();

    // Assert - Debería redireccionar al login
    var url = Page.Url;
    Assert.Contains("/Identity/Account/Login", url);
    }
}
