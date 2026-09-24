namespace SalesDashboard.Api.Infrastructure.Seeding;

/// <summary>
/// «Характер» менеджера для генерации данных. Неравномерность в сиде задаётся явно,
/// а не надеждой на Random: сразу видно, кто сильный, кто слабый и у кого провал.
/// </summary>
/// <param name="SalesPerDay">Среднее число продаж в рабочий день.</param>
/// <param name="MaxDiscount">Максимальная скидка от прайса (0.15 = до 15%) — влияет на маржу.</param>
/// <param name="BigTicketChance">Вероятность, что продажа будет из дорогих категорий.</param>
/// <param name="CancelRate">Доля отменённых продаж.</param>
/// <param name="RefundRate">Доля возвратов.</param>
/// <param name="SlumpMonthsAgo">Месяц (сколько месяцев назад), в котором у менеджера провал продаж.</param>
public sealed record ManagerProfile(
    double SalesPerDay,
    double MaxDiscount,
    double BigTicketChance,
    double CancelRate,
    double RefundRate,
    int? SlumpMonthsAgo = null)
{
    public static readonly ManagerProfile Star = new(0.95, 0.05, 0.45, 0.04, 0.02);
    public static readonly ManagerProfile Solid = new(0.55, 0.08, 0.30, 0.06, 0.04);
    public static readonly ManagerProfile Discounter = new(0.75, 0.20, 0.30, 0.05, 0.03);
    public static readonly ManagerProfile Newbie = new(0.25, 0.10, 0.15, 0.12, 0.08);

    public ManagerProfile WithSlump(int monthsAgo) => this with { SlumpMonthsAgo = monthsAgo };

    public double ActivityFactor(DateOnly day, DateOnly today)
    {
        var monthsAgo = (today.Year - day.Year) * 12 + today.Month - day.Month;
        return SlumpMonthsAgo == monthsAgo ? 0.25 : 1.0;
    }
}
