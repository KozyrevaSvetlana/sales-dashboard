using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesDashboard.Api.Domain.Entities;

namespace SalesDashboard.Api.Infrastructure.Persistence.Configurations;

internal sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(16);

        builder.HasOne(s => s.Manager)
            .WithMany(m => m.Sales)
            .HasForeignKey(s => s.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Items)
            .WithOne(i => i.Sale)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы под реальные запросы дашборда (обоснование — в README):
        // лента последних продаж и тренды фильтруют/сортируют по дате;
        builder.HasIndex(s => s.SoldAtUtc);
        // KPI и категории — «оплаченные за период»;
        builder.HasIndex(s => new { s.Status, s.SoldAtUtc });
        // рейтинг — группировка по менеджеру внутри периода (заодно покрывает FK manager_id).
        builder.HasIndex(s => new { s.ManagerId, s.SoldAtUtc });
    }
}

internal sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable(t => t.HasCheckConstraint("ck_sale_items_quantity_positive", "quantity > 0"));

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
