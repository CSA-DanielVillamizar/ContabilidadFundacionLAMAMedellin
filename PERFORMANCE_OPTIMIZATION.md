# ⚡ Guía de Optimización de Performance - Sistema Contabilidad LAMA Medellín

## 📋 Objetivo

Identificar y resolver cuellos de botella de performance para garantizar tiempos de respuesta óptimos en producción, especialmente en operaciones CRUD frecuentes, reportes y consultas de alto volumen.

---

## 🎯 Áreas de Optimización

### 1. 🗄️ Entity Framework Core - Queries

#### Problema: N+1 Queries

**Síntoma:** Múltiples queries a la base de datos cuando se carga una entidad con relaciones.

**Ejemplo de código problemático:**

```csharp
// ❌ Genera N+1 queries
var clientes = await _context.Clientes.ToListAsync();
foreach (var cliente in clientes)
{
    var recibos = cliente.Recibos; // Lazy loading → query por cada cliente
}
```

**Solución: Eager Loading con Include()**

```csharp
// ✅ Una sola query con JOIN
var clientes = await _context.Clientes
    .Include(c => c.Recibos)
    .ToListAsync();
```

**Archivos a revisar:**

- [ ] `Services/Clientes/ClientesService.cs`
- [ ] `Services/Ventas/VentasService.cs`
- [ ] `Services/Compras/ComprasService.cs`
- [ ] `Services/Recibos/RecibosService.cs`
- [ ] `Services/Miembros/MiembrosService.cs`

**Acción:**

```bash
# Buscar todos los lugares donde se accede a propiedades de navegación sin Include
grep -r "\.Recibos" --include="*.cs" src/Server/Services/
grep -r "\.Ventas" --include="*.cs" src/Server/Services/
grep -r "\.Compras" --include="*.cs" src/Server/Services/
```

---

#### Problema: Select N+1 (Proyecciones)

**Síntoma:** Se traen entidades completas cuando solo se necesitan algunos campos.

**Ejemplo de código problemático:**

```csharp
// ❌ Trae TODAS las columnas de Clientes y Recibos
var clientes = await _context.Clientes
    .Include(c => c.Recibos)
    .ToListAsync();

return clientes.Select(c => new ClienteDto
{
    Id = c.Id,
    Nombre = c.Nombre,
    TotalRecibos = c.Recibos.Count // Solo necesitamos el COUNT
});
```

**Solución: Proyección con Select()**

```csharp
// ✅ Solo trae las columnas necesarias
var clientes = await _context.Clientes
    .Select(c => new ClienteDto
    {
        Id = c.Id,
        Nombre = c.Nombre,
        TotalRecibos = c.Recibos.Count
    })
    .ToListAsync();
```

**Archivos a revisar:**

- [ ] `Controllers/ClientesController.cs` (método `GetAll`)
- [ ] `Controllers/VentasController.cs`
- [ ] `Controllers/ComprasController.cs`
- [ ] `Pages/Tesoreria/ListaRecibos.razor.cs` (si existe code-behind)

---

#### Problema: Falta de AsNoTracking()

**Síntoma:** EF Core trackea cambios en entidades que solo se consultan (read-only).

**Ejemplo de código problemático:**

```csharp
// ❌ EF Core trackea cambios innecesariamente
var recibos = await _context.Recibos
    .Where(r => r.FechaEmision >= inicio && r.FechaEmision <= fin)
    .ToListAsync();
```

**Solución: AsNoTracking()**

```csharp
// ✅ Sin tracking, más rápido para consultas read-only
var recibos = await _context.Recibos
    .AsNoTracking()
    .Where(r => r.FechaEmision >= inicio && r.FechaEmision <= fin)
    .ToListAsync();
```

**Archivos a revisar:**

- [ ] Todos los métodos `GetAll`, `Search`, `GetById` en servicios
- [ ] Páginas de reportes (Tesorería, Cartera, Contabilidad)

---

### 2. 🔄 Blazor Server - StateHasChanged()

#### Problema: Llamadas excesivas a StateHasChanged()

**Síntoma:** Re-renderizados innecesarios de componentes, lag en la UI.

**Ejemplo de código problemático:**

```razor
@code {
    private List<Cliente> clientes = new();

    protected override async Task OnInitializedAsync()
    {
        foreach (var id in new[] { 1, 2, 3, 4, 5 })
        {
            var cliente = await ClientesService.GetByIdAsync(id);
            clientes.Add(cliente);
            StateHasChanged(); // ❌ Re-renderiza 5 veces
        }
    }
}
```

**Solución: StateHasChanged() solo al final**

```razor
@code {
    private List<Cliente> clientes = new();

    protected override async Task OnInitializedAsync()
    {
        foreach (var id in new[] { 1, 2, 3, 4, 5 })
        {
            var cliente = await ClientesService.GetByIdAsync(id);
            clientes.Add(cliente);
        }
        StateHasChanged(); // ✅ Re-renderiza 1 sola vez
    }
}
```

**Archivos a revisar:**

- [ ] `Pages/GerenciaNegocios/Clientes.razor`
- [ ] `Pages/GerenciaNegocios/Productos.razor`
- [ ] `Pages/GerenciaNegocios/Ventas.razor`
- [ ] `Pages/Tesoreria/ListaRecibos.razor`

**Acción:**

```bash
# Buscar todos los StateHasChanged() en componentes
grep -r "StateHasChanged()" --include="*.razor" src/Server/Pages/
```

---

### 3. 📊 DataTableWrapper - Paginación

#### Problema: Paginación en memoria (client-side)

**Síntoma:** Se traen 10,000 registros de la BD, luego se paginan en memoria.

**Ejemplo de código problemático:**

```csharp
// ❌ Trae TODOS los clientes, luego pagina en memoria
var todosLosClientes = await _context.Clientes.ToListAsync();
return todosLosClientes.Skip(page * pageSize).Take(pageSize);
```

**Solución: Paginación en SQL (server-side)**

```csharp
// ✅ SQL Server solo retorna la página solicitada
var clientes = await _context.Clientes
    .AsNoTracking()
    .OrderBy(c => c.Nombre)
    .Skip(page * pageSize)
    .Take(pageSize)
    .ToListAsync();

var total = await _context.Clientes.CountAsync();

return new PagedResult<Cliente>
{
    Items = clientes,
    TotalCount = total,
    PageSize = pageSize,
    CurrentPage = page
};
```

**Archivos a revisar:**

- [ ] `Components/DataTableWrapper.razor` (verificar si usa Skip/Take en query)
- [ ] Servicios que retornan listas grandes (ClientesService, ProductosService, etc.)

---

### 4. 🖼️ MudBlazor - Componentes Pesados

#### Problema: MudDataGrid con virtualización deshabilitada

**Síntoma:** Renderizar 1,000+ filas causa lag.

**Ejemplo de código problemático:**

```razor
<!-- ❌ Renderiza todas las filas -->
<MudDataGrid Items="@clientes" />
```

**Solución: Habilitar virtualización**

```razor
<!-- ✅ Solo renderiza filas visibles en viewport -->
<MudDataGrid Items="@clientes" Virtualize="true" />
```

**O usar MudTable con paginación server-side:**

```razor
<MudTable Items="@clientes" ServerData="LoadServerData" @ref="table">
    <HeaderContent>
        <MudTh>Nombre</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.Nombre</MudTd>
    </RowTemplate>
    <PagerContent>
        <MudTablePager />
    </PagerContent>
</MudTable>

@code {
    private async Task<TableData<Cliente>> LoadServerData(TableState state)
    {
        var data = await ClientesService.GetPagedAsync(state.Page, state.PageSize);
        return new TableData<Cliente>
        {
            Items = data.Items,
            TotalItems = data.TotalCount
        };
    }
}
```

**Archivos a revisar:**

- [ ] Todas las páginas con `<MudDataGrid>` o `<MudTable>`
- [ ] `Components/DataTableWrapper.razor`

---

### 5. 🚀 Response Compression

#### Problema: Respuestas HTTP sin comprimir

**Síntoma:** Transferencia de datos lenta, especialmente en reportes grandes.

**Solución: Habilitar Response Compression Middleware**

**Agregar a `Program.cs`:**

```csharp
// Antes de builder.Build()
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream", "application/json" });
    opts.EnableForHttps = true; // Habilitar para HTTPS
});

// Después de app = builder.Build()
app.UseResponseCompression(); // ← ANTES de UseStaticFiles()
app.UseStaticFiles();
```

**Agregar NuGet:**

```bash
dotnet add package Microsoft.AspNetCore.ResponseCompression
```

---

### 6. 📦 Output Caching (ASP.NET Core 8.0)

#### Problema: Endpoints que retornan siempre los mismos datos sin cache

**Síntoma:** Consultas repetidas a BD para datos que cambian poco (ej: conceptos, categorías).

**Solución: Output Cache Middleware**

**Agregar a `Program.cs`:**

```csharp
// Antes de builder.Build()
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));
    
    // Política para conceptos (cambian muy poco)
    options.AddPolicy("Conceptos", builder => builder
        .Expire(TimeSpan.FromHours(1))
        .Tag("conceptos"));
});

// Después de app = builder.Build()
app.UseOutputCache(); // ← ANTES de MapControllers()
app.MapControllers();
```

**Aplicar en Controller:**

```csharp
[HttpGet]
[OutputCache(PolicyName = "Conceptos")]
public async Task<IActionResult> GetConceptos()
{
    var conceptos = await _conceptosService.GetAllAsync();
    return Ok(conceptos);
}
```

**Invalidar cache al modificar:**

```csharp
[HttpPost]
public async Task<IActionResult> CreateConcepto(ConceptoDto dto)
{
    await _conceptosService.CreateAsync(dto);
    
    // Invalidar cache
    var cache = HttpContext.RequestServices.GetRequiredService<IOutputCacheStore>();
    await cache.EvictByTagAsync("conceptos", default);
    
    return Ok();
}
```

**Archivos a aplicar:**

- [ ] `Controllers/ConceptosController.cs`
- [ ] `Controllers/CategoriasController.cs` (si existe)
- [ ] `Controllers/ProveedoresController.cs` (lista de proveedores cambia poco)

---

### 7. 🔐 Async/Await - Patrones Correctos

#### Problema: Uso incorrecto de async/await

**Ejemplo de código problemático:**

```csharp
// ❌ Bloquea el thread con .Result
public List<Cliente> GetClientes()
{
    return _context.Clientes.ToListAsync().Result;
}

// ❌ Async innecesario (no hay operación asíncrona)
public async Task<int> SumarDos(int a, int b)
{
    return a + b;
}
```

**Solución:**

```csharp
// ✅ Async correcto
public async Task<List<Cliente>> GetClientesAsync()
{
    return await _context.Clientes.ToListAsync();
}

// ✅ Sin async si no hay operación asíncrona
public int SumarDos(int a, int b)
{
    return a + b;
}
```

**Archivos a revisar:**

- [ ] Todos los servicios en `Services/`
- [ ] Todos los controllers en `Controllers/`

**Acción:**

```bash
# Buscar uso de .Result o .Wait() (anti-patrones)
grep -r "\.Result" --include="*.cs" src/Server/
grep -r "\.Wait()" --include="*.cs" src/Server/
```

---

### 8. 📄 PDF Generation - QuestPDF

#### Problema: Generación de PDFs bloquea el thread

**Síntoma:** Timeout en reportes grandes o muchos certificados simultáneos.

**Solución: Generar PDFs en background (opcional)**

**Crear servicio de background:**

```csharp
public class PdfBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly Channel<PdfGenerationRequest> _channel;

    public PdfBackgroundService(IServiceProvider services)
    {
        _services = services;
        _channel = Channel.CreateUnbounded<PdfGenerationRequest>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _services.CreateScope();
            var pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();
            await pdfService.GenerateAsync(request);
        }
    }

    public async Task EnqueueAsync(PdfGenerationRequest request)
    {
        await _channel.Writer.WriteAsync(request);
    }
}
```

**Nota:** Para la mayoría de casos, la generación síncrona de QuestPDF es suficientemente rápida. Solo aplicar si hay problemas de timeout.

---

### 9. 🗂️ Indexes en SQL Server

#### Problema: Consultas lentas por falta de índices

**Síntoma:** Queries con `WHERE`, `JOIN`, `ORDER BY` tardan segundos.

**Solución: Agregar índices en columnas frecuentemente consultadas**

**Migraciones a crear:**

```bash
dotnet ef migrations add AddIndexesToPerformance
```

**Código de migración:**

```csharp
public partial class AddIndexesToPerformance : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Índice en Recibos.FechaEmision (consultas de reportes por fecha)
        migrationBuilder.CreateIndex(
            name: "IX_Recibos_FechaEmision",
            table: "Recibos",
            column: "FechaEmision");

        // Índice en Recibos.Estado (filtros por estado)
        migrationBuilder.CreateIndex(
            name: "IX_Recibos_Estado",
            table: "Recibos",
            column: "Estado");

        // Índice compuesto en Ventas.FechaVenta + Estado
        migrationBuilder.CreateIndex(
            name: "IX_Ventas_FechaVenta_Estado",
            table: "Ventas",
            columns: new[] { "FechaVenta", "Estado" });

        // Índice en Clientes.NumeroIdentificacion (búsqueda por NIT)
        migrationBuilder.CreateIndex(
            name: "IX_Clientes_NumeroIdentificacion",
            table: "Clientes",
            column: "NumeroIdentificacion",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Recibos_FechaEmision", table: "Recibos");
        migrationBuilder.DropIndex(name: "IX_Recibos_Estado", table: "Recibos");
        migrationBuilder.DropIndex(name: "IX_Ventas_FechaVenta_Estado", table: "Ventas");
        migrationBuilder.DropIndex(name: "IX_Clientes_NumeroIdentificacion", table: "Clientes");
    }
}
```

**Archivos a revisar:**

- [ ] Revisar `Migrations/` para verificar índices existentes
- [ ] Ejecutar `EXPLAIN` en SQL Server para queries lentas

---

### 10. 🧹 Connection Pooling

#### Problema: Conexiones a BD se crean/destruyen constantemente

**Síntoma:** Alto tiempo de latencia en el primer request.

**Solución: Connection Pooling (habilitado por defecto en SQL Server)**

**Verificar en connection string:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LamaMedellin;Trusted_Connection=True;TrustServerCertificate=True;Min Pool Size=5;Max Pool Size=100;"
  }
}
```

**Configuración recomendada:**

- `Min Pool Size=5` → Mantiene 5 conexiones abiertas
- `Max Pool Size=100` → Máximo 100 conexiones simultáneas
- `Connection Lifetime=0` → Sin límite de tiempo (default)

---

## 📈 Benchmarking y Profiling

### Herramientas Recomendadas

1. **MiniProfiler** (ASP.NET Core)

   ```bash
   dotnet add package MiniProfiler.AspNetCore.Mvc
   dotnet add package MiniProfiler.EntityFrameworkCore
   ```

   ```csharp
   // Program.cs
   builder.Services.AddMiniProfiler(options =>
   {
       options.RouteBasePath = "/profiler";
   }).AddEntityFramework();

   app.UseMiniProfiler();
   ```

   **Acceso:** `http://localhost:5000/profiler/results`

2. **Application Insights** (Azure)

   Ver [DEPLOYMENT_GUIDE.md - Monitoreo](#monitoreo-con-azure-application-insights)

3. **SQL Server Profiler**

   Capturar queries lentas:
   - Abrir SQL Server Profiler
   - Template: **TSQL_Duration**
   - Filtro: `Duration >= 1000` (queries > 1 segundo)

4. **dotnet-trace** (local)

   ```bash
   dotnet tool install --global dotnet-trace
   dotnet-trace collect --process-id <PID>
   ```

---

## ✅ Checklist de Optimización

### Entity Framework Core
- [ ] Todas las queries tienen `AsNoTracking()` en métodos read-only
- [ ] Uso de `Include()` para evitar N+1 queries
- [ ] Proyecciones con `Select()` para traer solo columnas necesarias
- [ ] Paginación server-side con `Skip()` y `Take()`
- [ ] Índices en columnas con `WHERE`, `JOIN`, `ORDER BY` frecuentes

### Blazor Server
- [ ] `StateHasChanged()` solo cuando es necesario (no en bucles)
- [ ] Componentes pesados usan virtualización (`Virtualize="true"`)
- [ ] DataTables con paginación server-side

### ASP.NET Core Middleware
- [ ] Response Compression habilitado
- [ ] Output Cache para endpoints estáticos (conceptos, categorías)
- [ ] HSTS y HTTPS redirection en producción
- [ ] Health checks configurados (`/health`, `/health/ready`)

### Async/Await
- [ ] No hay uso de `.Result` o `.Wait()` (buscar con grep)
- [ ] Métodos async retornan `Task<T>`, no `Task.FromResult()`
- [ ] Servicios con sufijo `Async` en nombres de métodos

### SQL Server
- [ ] Connection pooling configurado (`Min Pool Size=5`, `Max Pool Size=100`)
- [ ] Índices en: `FechaEmision`, `Estado`, `NumeroIdentificacion`
- [ ] Queries optimizadas (sin SELECT *, sin DISTINCT innecesarios)

---

## 📊 Métricas de Performance Objetivo

| Métrica | Objetivo | Actual |
|---------|----------|--------|
| Tiempo de carga inicial (home) | < 2s | ⬜ Medir |
| Listar 100 clientes | < 500ms | ⬜ Medir |
| Crear recibo | < 300ms | ⬜ Medir |
| Generar PDF de certificado | < 1s | ⬜ Medir |
| Reporte tesorería (1 mes) | < 2s | ⬜ Medir |
| Consumo de memoria (Blazor Circuit) | < 50 MB/usuario | ⬜ Medir |

---

## 🎯 Próximos Pasos

1. **Ejecutar benchmarks** con MiniProfiler
2. **Identificar queries lentas** con SQL Server Profiler
3. **Aplicar optimizaciones** según checklist
4. **Re-medir performance** y comparar con objetivos
5. **Documentar mejoras** en este archivo

---

**Versión**: 1.0  
**Última actualización**: ${new Date().toLocaleDateString('es-CO')}  
**Responsable**: Daniel Villamizar
