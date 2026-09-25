using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SalesDashboard.IntegrationTests;

/// <summary>
/// Один контейнер PostgreSQL на все тесты коллекции (запуск ~5–10 с).
/// Схема создаётся теми же миграциями, что и в приложении, — заодно проверяем и их.
/// Перед каждым тестом таблицы очищаются (ResetAsync), тесты коллекции идут последовательно.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public AppDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options);

    public async Task ResetAsync()
    {
        await using var db = CreateDbContext();
        await db.Database.ExecuteSqlRawAsync(
            "TRUNCATE sale_items, sales, products, categories, customers, managers RESTART IDENTITY CASCADE");
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "PostgreSQL";
}
