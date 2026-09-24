using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Domain.Entities;

namespace SalesDashboard.Api.Infrastructure.Persistence;

/// <summary>
/// DbContext уже реализует Unit of Work и репозиторий, поэтому отдельного Generic Repository нет:
/// он бы только спрятал IQueryable и помешал строить агрегаты на стороне БД.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Деньги — только decimal/numeric, никогда double.
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }
}
