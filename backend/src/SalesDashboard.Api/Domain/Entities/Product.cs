namespace SalesDashboard.Api.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    /// <summary>Текущая прайсовая цена. В продажах хранится снимок цены на момент продажи.</summary>
    public decimal ListPrice { get; set; }

    /// <summary>Текущая закупочная себестоимость единицы.</summary>
    public decimal UnitCost { get; set; }
}
