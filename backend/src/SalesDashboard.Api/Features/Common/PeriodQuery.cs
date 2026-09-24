using SalesDashboard.Api.Domain.Periods;

namespace SalesDashboard.Api.Features.Common;

/// <summary>Общие query-параметры периода: ?preset=Custom&amp;from=2026-09-01&amp;to=2026-09-24 (даты включительно).</summary>
public sealed record PeriodQuery(PeriodPreset Preset = PeriodPreset.Last30Days, DateOnly? From = null, DateOnly? To = null);

/// <summary>Период в ответе API: даты включительно, как их видит пользователь.</summary>
public sealed record PeriodDto(DateOnly From, DateOnly To, DateOnly PreviousFrom, DateOnly PreviousTo)
{
    public static PeriodDto Create(ReportingPeriod period) => new(
        period.Current.From,
        period.Current.ToInclusive,
        period.Previous.From,
        period.Previous.ToInclusive);
}
