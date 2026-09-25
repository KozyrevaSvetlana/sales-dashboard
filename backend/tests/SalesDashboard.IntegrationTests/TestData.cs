using SalesDashboard.Api.Domain.Entities;
using SalesDashboard.Api.Infrastructure.Persistence;

namespace SalesDashboard.IntegrationTests;

/// <summary>Короткий способ завести в тестовой БД ровно те данные, которые нужны тесту.</summary>
internal sealed class TestData(AppDbContext db)
{
    public static readonly TimeZoneInfo Moscow = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");

    private int _skuCounter;

    /// <summary>Момент по московскому времени → UTC, как он хранится в sales.sold_at_utc.</summary>
    public static DateTime Msk(int year, int month, int day, int hour = 12, int minute = 0) =>
        TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified), Moscow);

    public Category Category(string name)
    {
        var category = new Category { Name = name };
        db.Categories.Add(category);
        return category;
    }

    public Product Product(Category category, string name, decimal price, decimal cost)
    {
        var product = new Product
        {
            Name = name,
            Sku = $"T-{++_skuCounter:D4}",
            Category = category,
            ListPrice = price,
            UnitCost = cost,
        };
        db.Products.Add(product);
        return product;
    }

    public Manager Manager(string fullName, bool isActive = true)
    {
        var manager = new Manager { FullName = fullName, Team = "Тест", Position = "Менеджер", IsActive = isActive };
        db.Managers.Add(manager);
        return manager;
    }

    public Customer Customer(string name = "Клиент")
    {
        var customer = new Customer { Name = name, Company = "ООО «Тест»", Segment = CustomerSegment.Smb };
        db.Customers.Add(customer);
        return customer;
    }

    /// <summary>Продажа: позиции по цене продажи; себестоимость берётся из товара.</summary>
    public Sale Sale(
        Manager manager,
        Customer customer,
        DateTime soldAtUtc,
        SaleStatus status,
        params (Product Product, int Quantity, decimal UnitPrice)[] items)
    {
        var sale = new Sale { Manager = manager, Customer = customer, SoldAtUtc = soldAtUtc, Status = status };
        foreach (var (product, quantity, unitPrice) in items)
        {
            sale.Items.Add(new SaleItem
            {
                Product = product,
                Quantity = quantity,
                UnitPrice = unitPrice,
                UnitCost = product.UnitCost,
            });
        }

        db.Sales.Add(sale);
        return sale;
    }

    public Task SaveAsync() => db.SaveChangesAsync();
}
