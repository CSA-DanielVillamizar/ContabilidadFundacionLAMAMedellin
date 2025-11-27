using System.Threading.Tasks;
using Xunit;

namespace ContabilidadLAMAMedellin.Tests.E2E.Common
{
    /// <summary>
    /// Colección compartida para pruebas E2E que reutiliza el mismo
    /// <see cref="TestWebApplicationFactory"/> a lo largo de la suite.
    /// </summary>
    [CollectionDefinition("E2ECollection")]
    public class E2ECollection : ICollectionFixture<E2EFixture>
    {
        // Intencionalmente vacío: xUnit usa esta clase para asociar el fixture a la colección.
    }

    /// <summary>
    /// Fixture que inicializa el factory y ejecuta el seed de prueba una sola vez.
    /// </summary>
    public class E2EFixture : IAsyncLifetime
    {
        public TestWebApplicationFactory Factory { get; }

        public E2EFixture()
        {
            Factory = new TestWebApplicationFactory();
        }

        public async Task InitializeAsync()
        {
            // Inicializa el seed una sola vez para toda la colección
            await Factory.EnsureSeedAsync();
        }

        public Task DisposeAsync()
        {
            Factory?.Dispose();
            return Task.CompletedTask;
        }
    }
}
