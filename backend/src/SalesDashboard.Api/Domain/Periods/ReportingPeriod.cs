namespace SalesDashboard.Api.Domain.Periods;

/// <summary>Выбранный период и сопоставимый с ним предыдущий — для расчёта динамики.</summary>
public sealed record ReportingPeriod(DateRange Current, DateRange Previous);
