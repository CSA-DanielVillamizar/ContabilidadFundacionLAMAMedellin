namespace Server.DTOs;

/// <summary>
/// Resultado paginado genérico para listados.
/// </summary>
/// <typeparam name="T">Tipo de los elementos.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Elementos de la página actual.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Número total de elementos sin paginar.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Número de página actual (1-indexed).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Tamaño de página.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de páginas calculado.
    /// </summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
