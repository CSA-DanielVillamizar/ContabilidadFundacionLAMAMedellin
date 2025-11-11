# Componentes Reutilizables MudBlazor

Esta carpeta contiene componentes reutilizables basados en MudBlazor para mantener consistencia en toda la aplicación.

## Componentes Disponibles

### 1. **KpiCard** - Tarjeta de Indicador Clave
Muestra métricas importantes con título, valor y badge de cambio.

**Uso:**
```razor
<KpiCard Title="Total Ventas"
         Value="@totalVentas.ToString("C0")"
         Badge="+15%"
         TitleColor="Color.Success"
         BadgeColor="Color.Success" />
```

**Parámetros:**
- `Title` (string): Título del KPI
- `Value` (string): Valor principal a mostrar
- `Badge` (string?): Texto del badge (opcional)
- `TitleColor` (Color): Color del título (Default: Primary)
- `BadgeColor` (Color): Color del badge (Default: Info)
- `Elevation` (int): Elevación del papel (Default: 2)
- `Class` (string): Clases CSS adicionales

---

### 2. **PageHeader** - Encabezado de Página
Encabezado estandarizado con icono, título y área de acciones.

**Uso:**
```razor
<PageHeader Title="Productos" 
            Icon="@Icons.Material.Filled.Inventory">
    <Actions>
        <MudButton Color="Color.Primary" StartIcon="@Icons.Material.Filled.Add">
            Nuevo Producto
        </MudButton>
    </Actions>
</PageHeader>
```

**Parámetros:**
- `Title` (string): Título de la página
- `Subtitle` (string?): Subtítulo opcional
- `Icon` (string?): Icono Material Design
- `IconColor` (Color): Color del icono (Default: Primary)
- `IconSize` (Size): Tamaño del icono (Default: Large)
- `TitleTypo` (Typo): Tipografía del título (Default: h4)
- `Actions` (RenderFragment?): Contenido de acciones (botones, filtros)
- `MarginBottom` (int): Margen inferior (Default: 4)
- `Class` (string): Clases CSS adicionales

---

### 3. **StatCard** - Tarjeta Estadística
Tarjeta simple para mostrar estadísticas o enlaces a secciones.

**Uso:**
```razor
<StatCard Title="Ventas"
          Icon="@Icons.Material.Filled.ShoppingCart"
          Description="Gestión de ventas y cotizaciones"
          ButtonText="Ver Ventas"
          ButtonHref="ventas" />
```

**Parámetros:**
- `Title` (string): Título de la tarjeta
- `Description` (string?): Descripción breve
- `Icon` (string?): Icono Material Design
- `IconColor` (Color): Color del icono (Default: Primary)
- `ButtonText` (string?): Texto del botón de acción
- `ButtonHref` (string?): URL de navegación
- `ButtonOnClick` (EventCallback): Evento click del botón
- `ButtonVariant` (Variant): Variante del botón (Default: Filled)
- `ButtonColor` (Color): Color del botón (Default: Primary)
- `CustomContent` (RenderFragment?): Contenido personalizado adicional
- `Elevation` (int): Elevación del papel (Default: 2)
- `Class` (string): Clases CSS adicionales

---

### 4. **DataTableWrapper** - Tabla de Datos
Tabla completa con búsqueda, paginación y acciones personalizables.

**Uso:**
```razor
<DataTableWrapper TItem="Producto"
                  Title="Productos"
                  Items="@productos"
                  SearchFunction="@((item, search) => item.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase))">
    <HeaderActions>
        <MudButton Color="Color.Primary" StartIcon="@Icons.Material.Filled.Add">
            Nuevo
        </MudButton>
    </HeaderActions>
    <TableHeader>
        <MudTh>Código</MudTh>
        <MudTh>Nombre</MudTh>
        <MudTh>Precio</MudTh>
        <MudTh>Stock</MudTh>
    </TableHeader>
    <TableRow Context="producto">
        <MudTd>@producto.Codigo</MudTd>
        <MudTd>@producto.Nombre</MudTd>
        <MudTd>@producto.Precio.ToString("C0")</MudTd>
        <MudTd>@producto.Stock</MudTd>
    </TableRow>
</DataTableWrapper>
```

**Parámetros:**
- `Title` (string): Título de la tabla
- `Items` (IEnumerable<TItem>): Items a mostrar
- `SearchFunction` (Func<TItem, string, bool>?): Función de búsqueda
- `TableHeader` (RenderFragment): Template del encabezado (MudTh)
- `TableRow` (RenderFragment<TItem>): Template de fila (MudTd)
- `HeaderActions` (RenderFragment?): Acciones del encabezado
- `ShowHeader` (bool): Mostrar encabezado (Default: true)
- `ShowSearch` (bool): Mostrar búsqueda (Default: true)
- `ShowPagination` (bool): Mostrar paginación (Default: true)
- `Hover` (bool): Efecto hover en filas (Default: true)
- `Dense` (bool): Tabla compacta (Default: true)
- `Loading` (bool): Indicador de carga (Default: false)
- `PageSize` (int): Tamaño de página (Default: 10)
- `Elevation` (int): Elevación del papel (Default: 2)
- `Class` (string): Clases CSS adicionales

---

## Mejores Prácticas

1. **Usa componentes consistentemente**: Aplica estos componentes en todas las páginas nuevas
2. **Personaliza cuando sea necesario**: Los parámetros permiten flexibilidad sin duplicar código
3. **Mantén la semántica de colores**: 
   - Primary: Acciones principales
   - Success: Ingresos, positivos, confirmaciones
   - Warning: Advertencias, egresos
   - Error: Errores, negativos, eliminaciones
   - Info: Información neutral

## Ejemplo Completo

```razor
@page "/productos"

<PageTitle>Productos</PageTitle>

<MudPaper Elevation="1" Class="pa-6">
    <PageHeader Title="Productos" 
                Icon="@Icons.Material.Filled.Inventory">
        <Actions>
            <MudButton Color="Color.Primary" 
                      StartIcon="@Icons.Material.Filled.Add"
                      OnClick="@AbrirFormulario">
                Nuevo Producto
            </MudButton>
        </Actions>
    </PageHeader>

    <MudGrid Class="mb-6">
        <MudItem xs="12" sm="6" md="3">
            <KpiCard Title="Total Productos"
                     Value="@productos.Count.ToString()"
                     TitleColor="Color.Primary" />
        </MudItem>
        <MudItem xs="12" sm="6" md="3">
            <KpiCard Title="Valor Inventario"
                     Value="@valorTotal.ToString("C0")"
                     TitleColor="Color.Success" />
        </MudItem>
    </MudGrid>

    <DataTableWrapper TItem="Producto"
                      Title="Lista de Productos"
                      Items="@productos"
                      SearchFunction="@SearchProducto">
        <TableHeader>
            <MudTh>Código</MudTh>
            <MudTh>Nombre</MudTh>
            <MudTh>Precio</MudTh>
        </TableHeader>
        <TableRow Context="producto">
            <MudTd>@producto.Codigo</MudTd>
            <MudTd>@producto.Nombre</MudTd>
            <MudTd>@producto.Precio.ToString("C0")</MudTd>
        </TableRow>
    </DataTableWrapper>
</MudPaper>

@code {
    private List<Producto> productos = new();
    private decimal valorTotal => productos.Sum(p => p.Precio * p.Stock);

    private bool SearchProducto(Producto producto, string searchText)
    {
        return producto.Nombre.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || producto.Codigo.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }
}
```
