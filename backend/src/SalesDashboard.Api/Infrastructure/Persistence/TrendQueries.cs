using Dapper;
using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Domain.Entities;
using SalesDashboard.Api.Domain.Periods;

namespace SalesDashboard.Api.Infrastructure.Persistence;

/// <summary>
/// Агрегаты для графика динамики — единственный запрос на «сыром» SQL (через Dapper).
///
/// Почему не LINQ: нужна группировка по дню/неделе/месяцу в часовом поясе компании
/// (date_trunc + AT TIME ZONE). В SQL это одна понятная строка, а в LINQ — зависимость
/// от тонкостей трансляции провайдера. Задание явно допускает raw SQL/Dapper для аналитики.
///
/// Правило статусов здесь продублировано в SQL (status = 'Paid'). Чтобы оно не разошлось
/// с SaleRules, интеграционный тест сверяет сумму точек графика с KPI за тот же период.
/// </summary>
public static class TrendQueries
{
    private const string Sql = """
        SELECT
            date_trunc(@granularity, s.sold_at_utc AT TIME ZONE @timeZone)::date AS bucket,
            COALESCE(SUM(i.quantity * i.unit_price), 0)                          AS revenue,
            COALESCE(SUM(i.quantity * i.unit_cost), 0)                           AS cost,
            COUNT(DISTINCT s.id)                                                 AS salescount
        FROM sales s
        JOIN sale_items i ON i.sale_id = s.id
        WHERE s.status = @paidStatus
          AND s.sold_at_utc >= @fromUtc
          AND s.sold_at_utc <  @toUtc
        GROUP BY 1
        ORDER BY 1
        """;

    public static async Task<Dictionary<DateOnly, SalesTotals>> GetTotalsByBucketAsync(
        this AppDbContext db,
        UtcRange range,
        TrendGranularity granularity,
        string timeZoneIanaId,
        CancellationToken ct)
    {
        var connection = db.Database.GetDbConnection();
        var command = new CommandDefinition(
            Sql,
            new
            {
                granularity = granularity.ToString().ToLowerInvariant(), // day | week | month
                timeZone = timeZoneIanaId,
                paidStatus = nameof(SaleStatus.Paid),
                fromUtc = range.FromUtc,
                toUtc = range.ToUtcExclusive,
            },
            cancellationToken: ct);

        var rows = await connection.QueryAsync<TrendRow>(command);

        return rows.ToDictionary(
            r => DateOnly.FromDateTime(r.Bucket),
            r => new SalesTotals(r.Revenue, r.Cost, (int)r.SalesCount));
    }

    internal sealed class TrendRow
    {
        public DateTime Bucket { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public long SalesCount { get; set; }
    }
}
