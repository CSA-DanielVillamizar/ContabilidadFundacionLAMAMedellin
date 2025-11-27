# Fixtures de Tests E2E

## Descripción
Fixtures especializados para preparar datos de prueba específicos por módulo. Complementan el `TestDataSeed` general con datos adicionales necesarios para tests más complejos.

## Fixtures Disponibles

### TesoreriaFixture
Prepara datos específicos para el módulo de Tesorería:
- ✅ 2 Recibos de caja de prueba (TEST-001, TEST-002)
- ✅ 2 Certificados de donación (Borrador y Emitido)
- ✅ Conceptos de Mensualidad y Donación
- ✅ Miembros asociados a los recibos

**Cuándo usar**: Tests que requieren recibos o certificados existentes para editar, consultar o validar flujos complejos.

### GerenciaFixture
Prepara datos específicos para el módulo de Gerencia de Negocios:
- ✅ 3 Productos de prueba (diferentes tipos y stocks)
- ✅ 1 Compra a proveedor con detalles
- ✅ 1 Venta a miembro con detalles
- ✅ Relaciones completas entre entidades

**Cuándo usar**: Tests que requieren productos, compras o ventas existentes para flujos complejos de edición, reportes o validaciones.

## Uso en Tests

### Opción 1: E2EFixture General (Recomendado para la mayoría)

Para tests simples que solo necesitan usuarios y datos base:

```csharp
[Collection("E2ECollection")]
public class MisTests : BaseTest
{
    private readonly TestWebApplicationFactory _factory;

    public MisTests(E2EFixture fixture)
    {
        _factory = fixture.Factory;
    }

    [Fact]
    public async Task Mi_Test_Simple()
    {
        // TestDataSeed ya ejecutado con datos base
        await LoginAsAdminAsync();
        // ... resto del test
    }
}
```

**Datos disponibles con E2EFixture:**
- Usuarios de prueba (test.admin@lama.test, etc.)
- Roles (Admin, Tesorero, gerentenegocios, Consulta)
- Conceptos básicos (TEST_Concepto_*)
- Miembros (TEST-001, TEST-CLI-001, TEST-PROV-001)
- Productos básicos (TEST-PROD-001)
- TRM de prueba

### Opción 2: Fixtures Especializados

Para tests que requieren datos complejos preexistentes:

#### Usando TesoreriaFixture

```csharp
// Nota: TesoreriaFixture requiere inyección de dependencias,
// por lo que debe usarse con un host de pruebas o crearse manualmente.
// Para simplificar, se recomienda crear datos específicos en el test mismo.

[Collection("E2ECollection")]
public class TesoreriaComplexTests : BaseTest
{
    private readonly TestWebApplicationFactory _factory;

    public TesoreriaComplexTests(E2EFixture fixture)
    {
        _factory = fixture.Factory;
    }

    [Fact]
    public async Task Tesoreria_EditarReciboExistente()
    {
        // TestDataSeed ya creó miembros y conceptos base
        // Puedes buscar un recibo existente o crear uno en el test
        await LoginAsTesoreroAsync();
        await NavigateToAsync("/tesoreria/recibos");
        
        // Buscar recibo TEST
        await MudBlazorHelpers.WaitForMudTableDataAsync(Page);
        await MudBlazorHelpers.ClickTableEditButtonAsync(Page, "TEST");
        // ... resto del test
    }
}
```

#### Usando GerenciaFixture

Similar al anterior, GerenciaFixture está diseñado para uso manual o con inyección de dependencias completa.

## Recomendación: Crear Datos en el Test

Para tests E2E con Playwright, es más simple y mantenible crear los datos necesarios directamente en el test usando la UI:

```csharp
[Fact]
public async Task Gerencia_EditarProducto_Completo()
{
    // Arrange - Crear producto de prueba
    await LoginAsAdminAsync();
    await NavigateToAsync("/gerencia-negocios/productos");
    
    // Crear producto para luego editarlo
    await MudBlazorHelpers.ClickMudButtonAsync(Page, "Nuevo");
    await MudBlazorHelpers.WaitForMudDialogAsync(Page);
    
    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
    await MudBlazorHelpers.FillMudTextFieldAsync(Page, "Código", $"TEST-{timestamp}");
    await MudBlazorHelpers.FillMudTextFieldAsync(Page, "Nombre", $"Producto Test {timestamp}");
    await MudBlazorHelpers.SaveMudDialogAsync(Page);
    await MudBlazorHelpers.WaitForMudDialogToCloseAsync(Page);

    // Act - Editar el producto recién creado
    await MudBlazorHelpers.ClickTableEditButtonAsync(Page, timestamp);
    await MudBlazorHelpers.WaitForMudDialogAsync(Page);
    await MudBlazorHelpers.FillMudNumericFieldAsync(Page, "Precio", "99999");
    await MudBlazorHelpers.SaveMudDialogAsync(Page);

    // Assert
    var updated = await MudBlazorHelpers.WaitForMudSnackbarAsync(Page, "éxito");
    Assert.True(updated);
}
```

## Cuándo Usar Cada Enfoque

| Enfoque | Ventajas | Desventajas | Cuándo Usar |
|---------|----------|-------------|-------------|
| **E2EFixture (General)** | Simple, rápido, datos limpios | Solo datos base | Tests simples de consulta y CRUD básico |
| **Fixtures Especializados** | Datos complejos listos | Más setup, mayor acoplamiento | Tests que requieren relaciones complejas preexistentes |
| **Crear en el Test** | Independiente, claro, mantenible | Más código en cada test | Tests E2E de flujos completos (recomendado) |

## Patrón Recomendado

Para la mayoría de tests E2E con Playwright:

1. ✅ Usa `E2EFixture` general (ya configurado en todos los tests)
2. ✅ Crea datos específicos en el test usando helpers de MudBlazor
3. ✅ Usa los datos base de `TestDataSeed` (TEST-CLI-001, TEST-PROD-001, etc.) cuando sea suficiente
4. ❌ Evita fixtures especializados complejos en tests E2E (reserva para tests de integración)

## Ejemplo Completo

```csharp
[Collection("E2ECollection")]
public class VentasComplexTests : BaseTest
{
    private readonly TestWebApplicationFactory _factory;

    public VentasComplexTests(E2EFixture fixture)
    {
        _factory = fixture.Factory;
        // TestDataSeed ya ejecutado automáticamente
    }

    [Fact]
    public async Task Venta_FlujoCompleto_DeCreacionAAprobacion()
    {
        // Arrange - Login
        await LoginAsAdminAsync();

        // Act 1 - Crear venta
        await NavigateToAsync("/gerencia-negocios/ventas");
        await MudBlazorHelpers.ClickMudButtonAsync(Page, "Nueva Venta");
        await MudBlazorHelpers.WaitForMudDialogAsync(Page);
        
        // Usar cliente TEST existente del seed
        await MudBlazorHelpers.SelectMudSelectAsync(Page, "Cliente", "TEST-CLI-001");
        await MudBlazorHelpers.SelectMudSelectAsync(Page, "Producto", "TEST-PROD-001");
        await MudBlazorHelpers.FillMudNumericFieldAsync(Page, "Cantidad", "2");
        await MudBlazorHelpers.SaveMudDialogAsync(Page);
        
        // Assert - Venta creada
        var created = await MudBlazorHelpers.WaitForMudSnackbarAsync(Page, "éxito");
        Assert.True(created);
        
        // Act 2 - Aprobar venta (cambiar estado)
        await MudBlazorHelpers.ClickTableEditButtonAsync(Page, "TEST-CLI-001");
        await MudBlazorHelpers.SelectMudSelectAsync(Page, "Estado", "Aprobada");
        await MudBlazorHelpers.SaveMudDialogAsync(Page);
        
        // Assert - Venta aprobada
        var approved = await MudBlazorHelpers.TableContainsRowAsync(Page, "Aprobada");
        Assert.True(approved);
    }
}
```

## Limpieza de Datos

Los datos de `TestDataSeed` persisten durante toda la ejecución de la colección de tests. No necesitas limpiarlos manualmente entre tests.

Si necesitas limpieza específica:

```csharp
public override async Task DisposeAsync()
{
    // Limpieza específica si es necesario
    // Nota: E2EFixture ya maneja la limpieza general
    await base.DisposeAsync();
}
```

## Conclusión

Para tests E2E con Playwright, **mantén las cosas simples**:
- Usa `E2EFixture` general
- Aprovecha los datos base de `TestDataSeed`
- Crea datos específicos en el test cuando sea necesario
- Los fixtures especializados (TesoreriaFixture, GerenciaFixture) están disponibles para casos excepcionales o tests de integración más complejos
