using SalesDashboard.Api.Features.Common;

namespace SalesDashboard.Api.Features.Categories;

public static class CategoriesEndpoints
{
    public static RouteGroupBuilder MapCategoriesEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/dashboard/categories",
                ([AsParameters] PeriodQuery query, CategoriesService service, CancellationToken ct, int top = CategoriesService.DefaultTop) =>
                    service.GetAsync(query, top, ct))
            .WithName("GetCategories")
            .WithTags("Dashboard")
            .WithSummary("Выручка и прибыль по категориям, доля в выручке и топ товаров");

        return api;
    }
}
