# Tests E2E (End-to-End)

## Descripción
Tests de navegador automatizados con Playwright que validan flujos completos de la aplicación.

## Arquitectura

### TestWebApplicationFactory
- Configura el entorno "Testing" con Identity habilitado
- Ejecuta `TestDataSeed` una sola vez para inicializar:
  - Roles: Admin, Tesorero, gerentenegocios, Consulta
  - Usuarios de prueba: `test.admin@lama.test` (contraseña: `Test123!`)
  - Datos de prueba con prefijo `TEST_`

### E2ECollection y E2EFixture
- `[Collection("E2ECollection")]` agrupa todos los tests E2E
- `E2EFixture` inicializa la factory y el seed una vez para toda la colección
- Evita reinicios y reseeds innecesarios

## Ejecución

### 1. Iniciar el servidor manualmente

El servidor debe ejecutarse en una terminal separada en modo Testing:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Testing"
dotnet run --project src/Server/Server.csproj --urls "http://localhost:5001"
```

**Importante**: La primera vez que inicie el servidor en modo Testing:
- Se ejecutará automáticamente `TestDataSeed`
- Verá en los logs: "✅ TestDataSeed: Datos de prueba E2E creados exitosamente"
- Los usuarios y datos de prueba estarán disponibles

### 2. Ejecutar los tests

En otra terminal (con el servidor corriendo):

```powershell
# Todos los tests E2E
dotnet test --filter "FullyQualifiedName~ContabilidadLAMAMedellin.Tests.E2E"

# Un test específico
dotnet test --filter "FullyQualifiedName~GerenciaTests.Gerencia_ConsultarClientes"

# Una clase completa
dotnet test --filter "FullyQualifiedName~GerenciaTests"
```

## Estructura

```
tests/E2E/
├── Common/
│   ├── TestWebApplicationFactory.cs  # Factory para Testing environment
│   ├── E2ECollection.cs              # Fixture compartido
│   └── MudBlazorHelpers.cs           # Helpers para componentes MudBlazor
├── BaseTest.cs                       # Clase base con navegación y login
├── GerenciaTests.cs                  # Tests del módulo Gerencia
├── TesoreriaTests.cs                 # Tests del módulo Tesorería
└── ConfiguracionTests.cs             # Tests de Configuración
```

## Usuarios de Prueba

Creados automáticamente por `TestDataSeed`:

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| test.admin@lama.test | Test123! | Admin |
| test.tesorero@lama.test | Test123! | Tesorero |
| test.gerente@lama.test | Test123! | gerentenegocios |
| test.consulta@lama.test | Test123! | Consulta |

## Datos de Prueba

Todos los datos de prueba tienen prefijo `TEST_`:
- Conceptos: TEST_Concepto_*
- Miembros: TEST_Cliente_*, TEST_Proveedor_*, etc.
- Productos: TEST_Producto_*
- TRM con fechas específicas de prueba

## Limpieza

Para limpiar los datos de prueba manualmente (opcional):

```csharp
// En un test o setup
_factory.Clean();
await _factory.EnsureSeedAsync();
```

Normalmente no es necesario limpiar entre tests, ya que cada test usa datos independientes con prefijos únicos.

## Notas Técnicas

### ¿Por qué servidor manual?

WebApplicationFactory usa `TestServer` (servidor en memoria), que no expone URLs HTTP accesibles para navegadores Playwright. Los tests E2E requieren un servidor HTTP real.

### Alternativas evaluadas

- ❌ `UseUrls("http://127.0.0.1:0")`: Puerto dinámico no es alcanzable externamente
- ❌ `IServerAddressesFeature`: Devuelve direcciones del TestServer, no de Kestrel
- ❌ Override `CreateHost`: WebApplicationFactory siempre crea TestServer

### Integración vs E2E

Para tests de integración (sin navegador), sí puedes usar la factory directamente:

```csharp
var client = _factory.CreateClient();
var response = await client.GetAsync("/api/endpoint");
```

Estos tests usan `HttpClient` y funcionan con el TestServer en memoria.
