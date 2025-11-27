
⚠️ **LIMITACIÓN IMPORTANTE - AUTORIZACIÓN EN BLAZOR SERVER:**
Los tests de integración HTTP **NO pueden validar autorización en páginas Blazor Server**. 
Esto es porque:
- Blazor Server NO protege rutas con HTTP status codes (302/401) en peticiones GET directas
- La protección real ocurre cuando se establece el **circuito SignalR**
- Las páginas siempre retornan 200 OK en HTTP GET, incluso si requieren autenticación
- La autorización se valida en el cliente después de renderizar la página

**Para validar autorización correctamente:**
- ✅ Usa tests E2E con Playwright (simulan navegador real + SignalR)
- ❌ NO uses HttpClient directamente para validar autorización en Blazor
# Tests de Integración

## Descripción
Tests de integración que usan `WebApplicationFactory` con `TestServer` en memoria. No requieren Playwright ni un servidor HTTP real, usan `HttpClient` directamente.

## Diferencia con Tests E2E

| Aspecto | Tests de Integración | Tests E2E |
|---------|---------------------|-----------|
| Tecnología | WebApplicationFactory + HttpClient | Playwright + Navegador real |
| Servidor | TestServer (en memoria) | Kestrel HTTP real |
| Velocidad | ⚡ Muy rápidos (milisegundos) | 🐌 Más lentos (segundos) |
| Alcance | API, endpoints, páginas Blazor | UI completa, interacciones usuario |
| Cuándo usar | Verificar lógica backend, APIs | Validar flujos completos de usuario |

## Arquitectura

### IntegrationTestWebApplicationFactory
- Configura el entorno "Testing" con Identity habilitado
- Ejecuta `TestDataSeed` una sola vez
- Usa TestServer en memoria (no expone puertos HTTP)

### IntegrationCollection y IntegrationFixture
- `[Collection("IntegrationCollection")]` agrupa todos los tests
- `IntegrationFixture` inicializa la factory y el seed una vez
- Comparte la factory entre todos los tests de integración

## Ejecución

```powershell
# Todos los tests de integración
dotnet test tests/Integration/ContabilidadLAMAMedellin.Tests.Integration.csproj

# Un test específico
dotnet test tests/Integration --filter "FullyQualifiedName~ApiIntegrationTests.Api_HealthCheck_ReturnsOk"

# Una clase completa
dotnet test tests/Integration --filter "FullyQualifiedName~ApiIntegrationTests"
```

## Estructura

```
tests/Integration/
├── Common/
│   ├── IntegrationTestWebApplicationFactory.cs  # Factory con TestServer
│   └── IntegrationCollection.cs                 # Fixture compartido
├── ApiIntegrationTests.cs                       # Tests de API REST
└── BlazorPagesIntegrationTests.cs               # Tests de páginas Blazor
```

## Tests Incluidos

### ApiIntegrationTests
- ✅ Health check endpoint
- ✅ TRM API endpoint
- ✅ Autorización (endpoints protegidos)

### BlazorPagesIntegrationTests
- ✅ Página principal accesible
- ✅ Página de login renderiza
- ✅ Páginas protegidas redirigen a login
- ✅ Archivos estáticos accesibles

## Ventajas

1. **Velocidad**: Se ejecutan en milisegundos
2. **Sin dependencias externas**: No requiere navegadores Playwright
3. **Fácil debugging**: Todo en el mismo proceso
4. **TestServer**: Usa la misma infraestructura que E2E para datos de prueba
5. **CI/CD friendly**: Ideales para pipelines de integración continua

## Cuándo Usar

✅ **Usa tests de integración para:**
- Verificar que endpoints API retornan respuestas correctas
- Validar códigos de estado HTTP
- Probar autorización y autenticación
- Verificar que páginas Blazor se renderizan sin errores
- Tests rápidos de regresión en CI/CD

❌ **NO uses tests de integración para:**
- Validar interacciones complejas de UI
- Verificar flujos de usuario completos
- Probar componentes MudBlazor específicos
- Simular comportamiento real del navegador

Para esos casos, usa los tests E2E con Playwright.

## Ejemplo de Test

```csharp
[Collection("IntegrationCollection")]
public class MyIntegrationTests
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MyIntegrationTests(IntegrationFixture fixture)
    {
        _factory = fixture.Factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task MyEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/my-endpoint");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

## Datos de Prueba

Al igual que en E2E, los tests de integración usan `TestDataSeed` que crea:
- Roles: Admin, Tesorero, gerentenegocios, Consulta
- Usuarios de prueba: `test.admin@lama.test` (contraseña: `Test123!`)
- Datos con prefijo `TEST_`

Los datos se comparten entre todos los tests de la colección.
