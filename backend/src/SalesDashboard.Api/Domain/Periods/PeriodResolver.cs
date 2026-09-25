using SalesDashboard.Api.Common;

namespace SalesDashboard.Api.Domain.Periods;

/// <summary>
/// Превращает пресет периода в конкретные даты и подбирает сопоставимый предыдущий период.
///
/// «Сегодня» берётся из TimeProvider, а не из DateTime.Now — поэтому логику можно
/// проверить тестами на любую дату, включая границы месяцев.
///
/// Правила сравнения:
///  • Сегодня / 7 / 30 дней / произвольный — предыдущий период такой же длины, вплотную перед текущим;
///  • Текущий месяц (с 1-го числа по сегодня) — те же дни прошлого месяца (1–24 сентября ↔ 1–24 августа),
///    но не дальше конца прошлого месяца (31 марта ↔ весь февраль);
///  • Прошлый месяц — позапрошлый календарный месяц целиком.
/// </summary>
public sealed class PeriodResolver(TimeProvider clock, TimeZoneInfo businessTimeZone)
{
    public const int MaxCustomRangeDays = 366;

    public TimeZoneInfo BusinessTimeZone => businessTimeZone;

    /// <summary>
    /// IANA-имя часового пояса (Europe/Moscow) — его понимает PostgreSQL в AT TIME ZONE.
    /// На Windows TimeZoneInfo может хранить Windows-имя (Russian Standard Time), поэтому конвертируем.
    /// </summary>
    public string BusinessTimeZoneIanaId =>
        businessTimeZone.HasIanaId
            ? businessTimeZone.Id
            : TimeZoneInfo.TryConvertWindowsIdToIanaId(businessTimeZone.Id, out var ianaId)
                ? ianaId
                : throw new InvalidOperationException($"Не удалось определить IANA-имя часового пояса {businessTimeZone.Id}.");

    public DateOnly Today() =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), businessTimeZone).DateTime);

    public UtcRange ToUtc(DateRange range) => range.ToUtc(businessTimeZone);

    public ReportingPeriod Resolve(PeriodPreset preset, DateOnly? from = null, DateOnly? to = null)
    {
        var today = Today();

        return preset switch
        {
            PeriodPreset.Today => RollingDays(today, days: 1),
            PeriodPreset.Last7Days => RollingDays(today, days: 7),
            PeriodPreset.Last30Days => RollingDays(today, days: 30),
            PeriodPreset.CurrentMonth => MonthToDate(today),
            PeriodPreset.PreviousMonth => FullMonth(StartOfMonth(today).AddMonths(-1)),
            PeriodPreset.Custom => Custom(from, to),
            _ => throw new BadRequestException($"Неизвестный период: {preset}."),
        };
    }

    private static ReportingPeriod RollingDays(DateOnly today, int days) =>
        WithPrecedingPeriod(new DateRange(today.AddDays(1 - days), today.AddDays(1)));

    private static ReportingPeriod MonthToDate(DateOnly today)
    {
        var monthStart = StartOfMonth(today);
        var current = new DateRange(monthStart, today.AddDays(1));

        var previousMonthStart = monthStart.AddMonths(-1);
        var previousEnd = Min(previousMonthStart.AddDays(current.Days), monthStart);

        return new ReportingPeriod(current, new DateRange(previousMonthStart, previousEnd));
    }

    private static ReportingPeriod FullMonth(DateOnly monthStart) =>
        new(
            new DateRange(monthStart, monthStart.AddMonths(1)),
            new DateRange(monthStart.AddMonths(-1), monthStart));

    private static ReportingPeriod Custom(DateOnly? from, DateOnly? to)
    {
        if (from is null || to is null)
        {
            throw new BadRequestException("Для произвольного периода нужны параметры from и to.");
        }

        if (to < from)
        {
            throw new BadRequestException("Дата окончания не может быть раньше даты начала.");
        }

        var range = DateRange.Inclusive(from.Value, to.Value);
        if (range.Days > MaxCustomRangeDays)
        {
            throw new BadRequestException($"Период не может быть длиннее {MaxCustomRangeDays} дней.");
        }

        return WithPrecedingPeriod(range);
    }

    private static ReportingPeriod WithPrecedingPeriod(DateRange current) =>
        new(current, current.PrecedingSameLength());

    private static DateOnly StartOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;
}
