namespace SalesDashboard.Api.Domain.Entities;

public class Sale
{
    public int Id { get; set; }
    public int ManagerId { get; set; }
    public Manager Manager { get; set; } = null!;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    /// <summary>Момент продажи в UTC. В бизнес-даты переводится через PeriodResolver.</summary>
    public DateTime SoldAtUtc { get; set; }

    public SaleStatus Status { get; set; }

    public List<SaleItem> Items { get; set; } = [];
}
