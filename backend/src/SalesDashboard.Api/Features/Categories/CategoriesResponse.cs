using SalesDashboard.Api.Features.Common;

namespace SalesDashboard.Api.Features.Categories;

/// <param name="Share">Доля категории в выручке периода (0.25 = 25%). null, если выручки нет.</param>
/// <param name="Units">Продано единиц товара.</param>
public sealed record CategorySalesDto(
    int CategoryId,
    string Name,
    decimal Revenue,
    decimal GrossProfit,
    decimal? Margin,
    decimal? Share,
    int Units);

public sealed record ProductSalesDto(
    int ProductId,
    string Name,
    string CategoryName,
    int Units,
    decimal Revenue,
    decimal GrossProfit,
    decimal? Margin);

public sealed record CategoriesResponse(
    PeriodDto Period,
    IReadOnlyList<CategorySalesDto> Categories,
    IReadOnlyList<ProductSalesDto> TopProducts);
