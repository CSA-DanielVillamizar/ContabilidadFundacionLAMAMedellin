using System.Net;
using Xunit;
using ContabilidadLAMAMedellin.Tests.Integration.Common;

namespace ContabilidadLAMAMedellin.Tests.Integration;

/// <summary>
/// Tests de integración para páginas Blazor.
/// Verifica que las páginas principales se rendericen correctamente.
/// </summary>
[Collection("IntegrationCollection")]
public class BlazorPagesIntegrationTests
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public BlazorPagesIntegrationTests(IntegrationFixture fixture)
    {
        _factory = fixture.Factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HomePage_Returns200()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.Redirect,
            "La página principal debería retornar 200 OK o redirigir a login"
        );
    }

    [Fact]
    public async Task LoginPage_Returns200()
    {
        // Act
        var response = await _client.GetAsync("/Identity/Account/Login");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("login", html.ToLower());
    }

    /// <summary>
    /// NOTA: Blazor Server NO protege rutas con HTTP status codes en peticiones GET directas.
    /// La protección real ocurre cuando se establece el circuito SignalR.
    /// Este test verifica que las páginas se rendericen (200 OK) sin error de servidor.
    /// La prueba de autorización real está en los tests E2E con Playwright.
    /// </summary>
    [Theory]
    [InlineData("/tesoreria/recibos")]
    [InlineData("/tesoreria/donaciones")]
    [InlineData("/gerencia-negocios/clientes")]
    [InlineData("/gerencia-negocios/productos")]
    [InlineData("/config/usuarios")]
    public async Task ProtectedPages_RenderWithoutServerError(string url)
    {
        // Act
        var response = await _client.GetAsync(url);

        // Assert - En Blazor Server, las páginas siempre retornan 200 OK en HTTP GET
        // La autorización real se valida en el circuito SignalR (tests E2E)
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"La página {url} debería renderizarse sin error de servidor (HTTP 5xx)"
        );
    }

    [Fact]
    public async Task StaticFiles_AreAccessible()
    {
        // Act - CSS de MudBlazor
        var cssResponse = await _client.GetAsync("/_content/MudBlazor/MudBlazor.min.css");

        // Assert
        Assert.True(
            cssResponse.StatusCode == HttpStatusCode.OK ||
            cssResponse.StatusCode == HttpStatusCode.NotModified,
            "Los archivos estáticos deberían estar accesibles"
        );
    }
}
