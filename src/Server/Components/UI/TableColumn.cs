using Microsoft.AspNetCore.Components;

namespace Server.Components.UI;

/// <summary>
/// Representa una columna en un componente UITable.
/// </summary>
/// <typeparam name="T">Tipo de dato de los items de la tabla</typeparam>
public class TableColumn<T>
{
    /// <summary>
    /// Texto del encabezado de la columna
    /// </summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>
    /// Función para renderizar el contenido de la celda. 
    /// Puede retornar string, RenderFragment, o cualquier objeto (se convertirá a string)
    /// </summary>
    public Func<T, object> Cell { get; set; } = _ => string.Empty;
}
