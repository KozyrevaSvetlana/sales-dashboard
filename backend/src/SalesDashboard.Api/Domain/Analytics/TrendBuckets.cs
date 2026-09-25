using SalesDashboard.Api.Domain.Periods;

namespace SalesDashboard.Api.Domain.Analytics;

public enum TrendGranularity
{
    Day,
    Week,
    Month,
}

/// <summary>Точка графика: начало интервала (день, понедельник недели или 1-е число месяца) и суммы за него.</summary>
public sealed record TrendPoint(DateOnly Bucket, decimal Revenue, decimal GrossProfit, int SalesCount);

/// <summary>
/// Разбиение периода на интервалы графика. Чистые функции без БД — поэтому покрыты юнит-тестами.
///
/// Правила:
///  • неделя начинается в понедельник (как date_trunc('week') в PostgreSQL);
///  • интервалы без продаж заполняются нулями, чтобы на графике не было «дыр» и ложных соединений точек;
///  • первый интервал может начинаться раньше периода (неделя/месяц, в которые попадает первый день) —
///    в него входят только продажи внутри периода.
/// </summary>
public static class TrendBuckets
{
    public const int MaxDailyDays = 45;
    public const int MaxWeeklyDays = 190;

    /// <summary>Детализация по умолчанию: чтобы на графике было 7–45 точек, а не 365 или 2.</summary>
    public static TrendGranularity AutoGranularity(DateRange range) => range.Days switch
    {
        <= MaxDailyDays => TrendGranularity.Day,
        <= MaxWeeklyDays => TrendGranularity.Week,
        _ => TrendGranularity.Month,
    };

    public static DateOnly BucketStart(DateOnly date, TrendGranularity granularity) => granularity switch
    {
        TrendGranularity.Day => date,
        TrendGranularity.Week => date.AddDays(-(((int)date.DayOfWeek + 6) % 7)), // понедельник
        TrendGranularity.Month => new DateOnly(date.Year, date.Month, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(granularity), granularity, null),
    };

    public static IReadOnlyList<DateOnly> Buckets(DateRange range, TrendGranularity granularity)
    {
        var buckets = new List<DateOnly>();
        for (var bucket = BucketStart(range.From, granularity); bucket < range.ToExclusive; bucket = Next(bucket, granularity))
        {
            buckets.Add(bucket);
        }

        return buckets;
    }

    /// <summary>Все интервалы периода по порядку; для интервалов без данных — нули.</summary>
    public static IReadOnlyList<TrendPoint> Fill(
        DateRange range,
        TrendGranularity granularity,
        IReadOnlyDictionary<DateOnly, SalesTotals> totalsByBucket) =>
        Buckets(range, granularity)
            .Select(bucket => totalsByBucket.TryGetValue(bucket, out var totals)
                ? new TrendPoint(bucket, totals.Revenue, totals.Revenue - totals.Cost, totals.SalesCount)
                : new TrendPoint(bucket, 0m, 0m, 0))
            .ToList();

    private static DateOnly Next(DateOnly bucket, TrendGranularity granularity) => granularity switch
    {
        TrendGranularity.Day => bucket.AddDays(1),
        TrendGranularity.Week => bucket.AddDays(7),
        TrendGranularity.Month => bucket.AddMonths(1),
        _ => throw new ArgumentOutOfRangeException(nameof(granularity), granularity, null),
    };
}
