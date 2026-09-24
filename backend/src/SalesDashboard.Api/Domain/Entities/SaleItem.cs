namespace SalesDashboard.Api.Domain.Entities;

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    /// <summary>Цена продажи за единицу (снимок на момент продажи, с учётом скидки).</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Себестоимость единицы (снимок на момент продажи).</summary>
    public decimal UnitCost { get; set; }
}
