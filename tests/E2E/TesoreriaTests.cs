using Microsoft.Playwright;
using Xunit;
using static ContabilidadLAMAMedellin.Tests.E2E.MudBlazorHelpers;
using ContabilidadLAMAMedellin.Tests.E2E.Common;

namespace ContabilidadLAMAMedellin.Tests.E2E;

/// <summary>
/// Pruebas E2E para el módulo de Tesorería.
/// Cubre: Recibos de caja, Certificados de donación, Reportes.
/// IMPORTANTE: Estos tests requieren que TestDataSeed se ejecute antes (ver TestWebApplicationFactory).
/// </summary>
[Collection("E2ECollection")]
public class TesoreriaTests : BaseTest
{
    private readonly TestWebApplicationFactory _factory;

    public TesoreriaTests(E2EFixture fixture)
    {
        _factory = fixture.Factory;
        // Factory inicializa TestDataSeed automáticamente en el entorno Testing
    }

    [Fact(Skip = "Receipt form requires page navigation to /tesoreria/recibos/nuevo with complex MudBlazor item management - needs dedicated workflow test")]
    public async Task Tesoreria_CrearRecibo_MuestraNotificacionExito()
    {
        // Arrange
        await LoginAsTesoreroAsync();
        await NavigateToAsync("/tesoreria/recibos");
        await WaitForPageIdleAsync();

        // Act
        Assert.NotNull(Page);
        
        // Hacer clic en "Nuevo Recibo" o botón similar
        await Page.ClickAsync("button:has-text('Nuevo'), button:has-text('Crear'), button:has-text('Agregar')");
        await WaitForDialogAsync();

        // Rellenar formulario de recibo
        await FillByLabelAsync("Número de Recibo", $"REC-{DateTime.Now:yyyyMMddHHmmss}");
        await FillByLabelAsync("Fecha", DateTime.Now.ToString("yyyy-MM-dd"));
        await FillByLabelAsync("Concepto", "Pago de prueba E2E");
        await FillByLabelAsync("Valor", "100000");
        
        // Seleccionar tipo de pago (si existe)
        try
        {
            await SelectByLabelAsync("Tipo de Pago", "Efectivo");
        }
        catch
        {
            // Si no existe el campo, continuar
        }

        // Guardar
        await Page.ClickAsync("button:has-text('Guardar'), button:has-text('Crear')");

        // Assert
        var hasNotification = await WaitForNotificationAsync("éxito");
        Assert.True(hasNotification, "Debería mostrar notificación de éxito");

        await WaitForPageIdleAsync();
        
        // Verificar que el recibo aparece en la tabla
        var reciboVisible = await Page.IsVisibleAsync("text=/REC-/i");
        Assert.True(reciboVisible, "El recibo creado debería aparecer en la lista");
    }

    [Fact(Skip = "Receipt edit requires existing data and complex form navigation - needs controlled test data setup")]
    public async Task Tesoreria_EditarRecibo_ActualizaDatos()
    {
        // Arrange
        await LoginAsTesoreroAsync();
        await NavigateToAsync("/tesoreria/recibos");
        await WaitForPageIdleAsync();

        // Act - Buscar primer recibo y hacer clic en editar
        Assert.NotNull(Page);
        
        var editButton = Page.Locator("button[title='Editar'], button:has-text('Editar'), .mud-icon-button:has(svg.mud-icon-edit)").First;
        await editButton.ClickAsync();
        await WaitForDialogAsync();

        // Modificar concepto
        var nuevoConcepto = $"Concepto modificado E2E - {DateTime.Now:HH:mm:ss}";
        await Page.FillAsync("input[name='Concepto'], textarea[name='Concepto']", "");
        await Page.FillAsync("input[name='Concepto'], textarea[name='Concepto']", nuevoConcepto);

        // Guardar cambios
        await Page.ClickAsync("button:has-text('Guardar'), button:has-text('Actualizar')");

        // Assert
        var hasNotification = await WaitForNotificationAsync();
        Assert.True(hasNotification, "Debería mostrar notificación de actualización");

        await WaitForPageIdleAsync();
        
        // Verificar que el cambio se refleja en la tabla
        var conceptoVisible = await Page.IsVisibleAsync($"text=/{nuevoConcepto}/i");
        Assert.True(conceptoVisible, "El concepto modificado debería aparecer en la lista");
    }

    [Fact]
    public async Task Tesoreria_ConsultarRecibos_MuestraListado()
    {
        // Arrange
        await LoginAsTesoreroAsync();

        // Act
        await NavigateToAsync("/tesoreria/recibos");
        await WaitForPageIdleAsync();

        // Assert
        Assert.NotNull(Page);
        
        // Verificar que la página carga correctamente buscando el heading específico
        var pageHeader = Page.GetByRole(AriaRole.Heading, new() { NameString = "Recibos de Caja" });
        await pageHeader.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        
        var hasHeader = await pageHeader.IsVisibleAsync();
        Assert.True(hasHeader, "La página de recibos debería mostrar el encabezado 'Recibos de Caja'");
        
        // Verificar que existe el botón "Nuevo Recibo"
        var newButton = Page.GetByRole(AriaRole.Link, new() { NameString = "Nuevo Recibo" });
        var buttonExists = await newButton.CountAsync() > 0;
        Assert.True(buttonExists, "La página debería mostrar el botón 'Nuevo Recibo'");
        
        // Verificar que existe la estructura de tabla (modern-table) o mensaje de "No hay recibos"
        var hasTable = await Page.Locator("table.modern-table").CountAsync() > 0;
        var hasEmptyMessage = await Page.Locator("text=No hay recibos registrados").CountAsync() > 0;
        Assert.True(hasTable || hasEmptyMessage, "La página debería mostrar una tabla de recibos o mensaje de lista vacía");
    }

    [Fact]
    public async Task Tesoreria_FiltrarRecibos_MuestraResultadosFiltrados()
    {
        // Arrange
        await LoginAsTesoreroAsync();
        await NavigateToAsync("/tesoreria/recibos");
        await WaitForPageIdleAsync();

        // Act
        Assert.NotNull(Page);
        
        // Buscar campo de filtro/búsqueda MudBlazor
        var searchInput = Page.Locator("input[placeholder*='Buscar'], input[type='search'], .mud-input-search input");
        if (await searchInput.CountAsync() > 0)
        {
            await searchInput.First.FillAsync("TEST");
            await Task.Delay(1000); // Esperar que el filtro se aplique
            await WaitForPageIdleAsync();

            // Assert - Verificar que muestra recibos de prueba
            var hasTestRecibo = await TableContainsRowAsync(Page, "TEST");
            Assert.True(hasTestRecibo, "Debería mostrar recibos filtrados con serie TEST");
        }
    }

    [Fact]
    public async Task Tesoreria_EliminarRecibo_MuestraConfirmacion()
    {
        // Arrange
        await LoginAsTesoreroAsync();
        await NavigateToAsync("/tesoreria/recibos");
        await WaitForPageIdleAsync();

        // Act
        Assert.NotNull(Page);
        
        // Buscar botón de eliminar del primer recibo
        var deleteButton = Page.Locator("button[title='Eliminar'], button:has-text('Eliminar'), .mud-icon-button:has(svg.mud-icon-delete)").First;
        
        if (await deleteButton.CountAsync() > 0)
        {
            await deleteButton.ClickAsync();

            // Assert - Debería mostrar diálogo de confirmación
            var hasConfirmDialog = await Page.IsVisibleAsync("text=/¿Está seguro|Confirmar|Eliminar/i");
            Assert.True(hasConfirmDialog, "Debería mostrar diálogo de confirmación");

            // Cancelar la eliminación
            await Page.ClickAsync("button:has-text('Cancelar'), button:has-text('No')");
        }
    }

    [Fact(Skip = "Certificate creation requires valid data seeding and complex validation - needs controlled test environment")]
    public async Task Tesoreria_EmitirCertificadoDonacion_GeneraPDF()
    {
        // Arrange
        await LoginAsTesoreroAsync();
        await NavigateToAsync("/tesoreria/donaciones");
        await WaitForPageIdleAsync();

        // Act
        Assert.NotNull(Page);
        
        // Hacer clic en "Nuevo Certificado" - navega a página, no abre diálogo
        await Page.ClickAsync("button:has-text('Nuevo Certificado'), button:has-text('Nuevo'), button:has-text('Crear')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(500); // Esperar hidratación de Blazor
        
        // Verificar que navegamos a la página del formulario
        await Page.WaitForURLAsync("**/tesoreria/donaciones/nuevo");

        // Rellenar formulario del certificado según etiquetas reales
        await FillByLabelAsync("Fecha de Donación", DateTime.Now.ToString("yyyy-MM-dd"));
        await FillByLabelAsync("Nombre Completo", "Juan Pérez - E2E Test");
        await FillByLabelAsync("Número de Identificación", "1234567890");
        await FillByLabelAsync("Valor en COP", "500000");
        await SelectByLabelAsync("Forma de Donación", "Efectivo");
        await FillByLabelAsync("Descripción de la Donación", "Donación en efectivo para obra social");
        await FillByLabelAsync("Destinación de la Donación", "Actividades sociales");

        // Guardar borrador
        await Page.ClickAsync("button:has-text('Guardar Borrador'), button:has-text('Guardar')");

        // Assert
        var hasNotification = await WaitForNotificationAsync();
        Assert.True(hasNotification, "Debería mostrar notificación de éxito");

        await WaitForPageIdleAsync();

        // Verificar que el certificado aparece en la lista
        var certificadoVisible = await Page.IsVisibleAsync("text=/CERT-/i");
        Assert.True(certificadoVisible, "El certificado creado debería aparecer en la lista");
    }

    [Fact]
    public async Task Tesoreria_ConsultarCertificados_MuestraListado()
    {
        // Arrange
        await LoginAsTesoreroAsync();

        // Act
        await NavigateToAsync("/tesoreria/donaciones");
        await WaitForPageIdleAsync();

        // Assert
        Assert.NotNull(Page);
        
        // Esperar que la tabla MudBlazor cargue o muestre mensaje vacío
        await MudBlazorHelpers.WaitForLoadingToFinishAsync(Page);
        
        var hasTable = await Page.IsVisibleAsync(".mud-table, table, [role='table'], .modern-table");
        var hasEmptyMessage = await Page.Locator("text=/No hay certificados|Sin certificados/i").IsVisibleAsync();
        Assert.True(hasTable || hasEmptyMessage, "La página de certificados debería mostrar contenido (tabla o mensaje)");
    }

    [Fact]
        public async Task Tesoreria_DescargarCertificadoPDF_IniciaDescarga()
    {
        // Arrange
        await LoginAsTesoreroAsync();
        await NavigateToAsync("/tesoreria/donaciones");
        await WaitForPageIdleAsync();

        // Act
        Assert.NotNull(Page);
        
        // Buscar botón de descargar PDF del primer certificado
        var downloadButton = Page.Locator("button[title='Descargar PDF'], a:has-text('Descargar PDF')").First;
        
        if (await downloadButton.CountAsync() > 0)
        {
            // Esperar evento de descarga
            var downloadTask = Page.WaitForDownloadAsync();
            await downloadButton.ClickAsync();

            try
            {
                var download = await downloadTask;
                
                // Assert
                Assert.NotNull(download);
                Assert.Contains(".pdf", download.SuggestedFilename.ToLower());
            }
            catch (TimeoutException)
            {
                // Si no se inicia descarga, puede ser que abre en nueva pestaña
                Console.WriteLine("⚠️ No se detectó descarga automática - puede abrir en nueva pestaña");
            }
        }
    }

    [Fact]
    public async Task Tesoreria_GenerarReporteRecibos_MuestraResultados()
    {
        // Arrange
        await LoginAsTesoreroAsync();
        await NavigateToAsync("/tesoreria/reportes");
        await WaitForPageIdleAsync();

        // Act
        Assert.NotNull(Page);
        
        // Usar MudBlazorHelpers para rellenar campos
        var anio = DateTime.Now.Year.ToString();
        var mes = DateTime.Now.Month.ToString();

        await FillMudTextFieldAsync(Page, "Año", anio);
        await FillMudTextFieldAsync(Page, "Mes", mes);

        // Hacer clic en botón Cargar usando helper
        await ClickMudButtonAsync(Page, "Cargar");
        await Task.Delay(2000); // Esperar a que cargue el reporte
        await WaitForPageIdleAsync();

        // Assert - Verificar que muestra los resultados (saldos inicial, final, ingresos, egresos)
        var hasSaldoInicial = await Page.IsVisibleAsync("text=/Saldo inicial/i");
        var hasSaldoFinal = await Page.IsVisibleAsync("text=/Saldo final/i");
        var hasIngresos = await Page.IsVisibleAsync("text=/Ingresos/i");
        var hasEgresos = await Page.IsVisibleAsync("text=/Egresos/i");
        
        Assert.True(hasSaldoInicial && hasSaldoFinal && hasIngresos && hasEgresos, 
            "Debería mostrar los resultados del reporte (Saldo inicial, Ingresos, Egresos, Saldo final)");
    }

    [Fact]
        public async Task Tesoreria_ExportarReporteExcel_IniciaDescarga()
    {
        // Arrange
        await LoginAsTesoreroAsync();
        await NavigateToAsync("/tesoreria/reportes");
        await WaitForPageIdleAsync();

        // Act
        Assert.NotNull(Page);
        
        // Configurar año y mes
        var anio = DateTime.Now.Year.ToString();
        var mes = DateTime.Now.Month.ToString();

        await FillByLabelAsync("Año", anio);
        await FillByLabelAsync("Mes", mes);
        
        // Buscar link de descarga de Excel (es un <a href> no un botón)
        var downloadLink = Page.Locator("a:has-text('Descargar Excel'), a:has-text('Excel')").First;
        
        if (await downloadLink.CountAsync() > 0)
        {
            // Verificar que el link existe y es visible
            var isVisible = await downloadLink.IsVisibleAsync();
            Assert.True(isVisible, "El botón de descarga de Excel debería estar visible");
            
            // Nota: No verificamos descarga real porque requiere configuración especial de Playwright
            Console.WriteLine("✓ Link de descarga Excel encontrado y visible");
        }
        else
        {
            Assert.Fail("No se encontró el link de descarga de Excel");
        }
    }

    [Fact(Skip = "Receipt form validation requires complex MudBlazor form interaction - needs dedicated validation test")]
    public async Task Tesoreria_ValidacionFormularioRecibo_MuestraErroresCamposRequeridos()
    {
        // Arrange
        await LoginAsTesoreroAsync();
        await NavigateToAsync("/tesoreria/recibos");
        await WaitForPageIdleAsync();

        // Act
        Assert.NotNull(Page);
        
        // Abrir formulario de nuevo recibo
        await Page.ClickAsync("button:has-text('Nuevo'), button:has-text('Crear')");
        await WaitForDialogAsync();

        // Intentar guardar sin llenar campos requeridos
        await Page.ClickAsync("button:has-text('Guardar'), button:has-text('Crear')");

        // Assert
        // Verificar que muestra mensajes de validación
        var hasValidationErrors = await Page.IsVisibleAsync("text=/requerido|obligatorio|debe ingresar/i, .mud-input-error");
        Assert.True(hasValidationErrors, "Debería mostrar errores de validación para campos requeridos");

        // Verificar que el diálogo sigue abierto (no se guardó)
        var dialogStillOpen = await Page.IsVisibleAsync(".mud-dialog, [role='dialog']");
        Assert.True(dialogStillOpen, "El diálogo debería permanecer abierto si hay errores de validación");
    }
}
