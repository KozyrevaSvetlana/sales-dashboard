using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Infrastructure.Persistence;
using SalesDashboard.Api.Infrastructure.Seeding;

namespace SalesDashboard.Api.Infrastructure;

/// <summary>
/// Применяет миграции и заполняет пустую БД при старте приложения —
/// чтобы `docker compose up --build` поднимал рабочий дашборд без ручных шагов.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseInitializer));

        logger.LogInformation("Применяю миграции…");
        await db.Database.MigrateAsync(ct);

        if (await db.Sales.AnyAsync(ct))
        {
            logger.LogInformation("Данные уже есть, сид пропущен.");
            return;
        }

        await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync(ct);
    }
}
