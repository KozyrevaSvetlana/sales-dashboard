namespace SalesDashboard.Api.Domain.Periods;

/// <summary>
/// Период в бизнес-датах, полуоткрытый интервал [From, ToExclusive).
///
/// Почему полуоткрытый: продажа в 23:59:59.999 последнего дня попадает в период,
/// а продажа в 00:00:00 следующего дня — нет. Смежные периоды не пересекаются
/// и не теряют продажи на границе.
/// </summary>
public sealed record DateRange
{
    public DateRange(DateOnly from, DateOnly toExclusive)
    {
        if (toExclusive <= from)
        {
            throw new ArgumentException("Период должен содержать хотя бы один день.", nameof(toExclusive));
        }

        From = from;
        ToExclusive = toExclusive;
    }

    public DateOnly From { get; }

    public DateOnly ToExclusive { get; }

    /// <summary>Последний день периода включительно — для отображения пользователю.</summary>
    public DateOnly ToInclusive => ToExclusive.AddDays(-1);

    public int Days => ToExclusive.DayNumber - From.DayNumber;

    /// <summary>Создаёт период из дат «с — по» включительно, как их выбирает пользователь.</summary>
    public static DateRange Inclusive(DateOnly from, DateOnly to) => new(from, to.AddDays(1));

    /// <summary>Такой же по длине период, непосредственно предшествующий текущему.</summary>
    public DateRange PrecedingSameLength() => new(From.AddDays(-Days), From);

    /// <summary>Переводит границы бизнес-дней (полночь в часовом поясе компании) в UTC для запроса к БД.</summary>
    public UtcRange ToUtc(TimeZoneInfo businessTimeZone) =>
        new(StartOfDayUtc(From, businessTimeZone), StartOfDayUtc(ToExclusive, businessTimeZone));

    private static DateTime StartOfDayUtc(DateOnly date, TimeZoneInfo timeZone) =>
        TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), timeZone);
}

/// <summary>Границы периода в UTC, тоже полуоткрытые: [FromUtc, ToUtcExclusive).</summary>
public readonly record struct UtcRange(DateTime FromUtc, DateTime ToUtcExclusive);
