using System.Net;
using System.Net.Http.Json;
using Xunit;
using ContabilidadLAMAMedellin.Tests.Integration.Common;

namespace ContabilidadLAMAMedellin.Tests.Integration;

/// <summary>
/// Tests de integración para endpoints de la API REST.
/// Usan HttpClient contra el TestServer en memoria (no requieren Playwright).
/// </summary>
[Collection("IntegrationCollection")]
public class ApiIntegrationTests
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(IntegrationFixture fixture)
    {
        _factory = fixture.Factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Api_HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Api_GetTRM_ReturnsJson()
    {
        // Act
        var response = await _client.GetAsync("/api/trm/latest");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }
        else
        {
            // Puede ser 404 si no hay TRM configurada
            Assert.True(
                response.StatusCode == HttpStatusCode.OK || 
                response.StatusCode == HttpStatusCode.NotFound,
                "Debería retornar OK con datos o NotFound si no hay TRM"
            );
        }
    }
}
