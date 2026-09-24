namespace SalesDashboard.Api.Domain.Entities;

public class Manager
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Team { get; set; }
    public required string Position { get; set; }
    public bool IsActive { get; set; } = true;

    public List<Sale> Sales { get; set; } = [];
}
