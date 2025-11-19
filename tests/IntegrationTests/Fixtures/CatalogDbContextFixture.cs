using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Fixtures;

/// <summary>
/// Test fixture for CatalogDbContext with Testcontainers
/// </summary>
public sealed class CatalogDbContextFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("catalog_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        ConnectionString = _dbContainer.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    public CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
