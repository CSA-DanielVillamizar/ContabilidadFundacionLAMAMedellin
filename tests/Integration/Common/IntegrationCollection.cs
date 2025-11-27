using Xunit;

namespace ContabilidadLAMAMedellin.Tests.Integration.Common;

/// <summary>
/// Definición de la colección de tests de integración.
/// Todos los tests que pertenezcan a esta colección compartirán el mismo IntegrationFixture.
/// </summary>
[CollectionDefinition("IntegrationCollection")]
public class IntegrationCollection : ICollectionFixture<IntegrationFixture>
{
}

/// <summary>
/// Fixture compartido para todos los tests de integración.
/// Se inicializa una vez por colección.
/// </summary>
public class IntegrationFixture : IAsyncLifetime
{
    public IntegrationTestWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Factory = new IntegrationTestWebApplicationFactory();
        await Factory.EnsureSeedAsync();
    }

    public Task DisposeAsync()
    {
        Factory?.Dispose();
        return Task.CompletedTask;
    }
}
