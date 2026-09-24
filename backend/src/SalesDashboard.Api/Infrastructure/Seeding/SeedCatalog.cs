using SalesDashboard.Api.Domain.Entities;

namespace SalesDashboard.Api.Infrastructure.Seeding;

/// <summary>Справочные данные для сида: каталог, менеджеры, клиенты.</summary>
internal static class SeedCatalog
{
    /// <summary>Категории, которые считаются «крупным чеком».</summary>
    public static readonly HashSet<string> BigTicketCategories = ["Дроны", "Профессиональные решения"];

    /// <param name="CostRatio">Доля себестоимости от прайсовой цены — задаёт типичную маржу категории.</param>
    private sealed record CatalogEntry(string Category, decimal CostRatio, (string Name, decimal Price)[] Products);

    private static readonly CatalogEntry[] Catalog =
    [
        new("Дроны", 0.78m,
        [
            ("DJI Neo", 24_990m), ("DJI Mini 4 Pro", 89_990m), ("DJI Air 3S", 139_990m),
            ("DJI Avata 2", 99_990m), ("DJI Mavic 3 Pro", 259_990m),
        ]),
        new("Профессиональные решения", 0.70m,
        [
            ("DJI Matrice 30T", 1_590_000m), ("DJI Mavic 3 Enterprise", 489_990m), ("DJI Agras T50", 2_150_000m),
        ]),
        new("Экшн-камеры", 0.74m,
        [
            ("DJI Osmo Action 5 Pro", 42_990m), ("DJI Osmo 360", 54_990m), ("DJI Osmo Pocket 3", 54_990m),
        ]),
        new("Стабилизаторы", 0.70m,
        [
            ("DJI RS 4 Pro", 79_990m), ("DJI RS 4 Mini", 39_990m), ("DJI Osmo Mobile 7P", 13_990m),
        ]),
        new("Микрофоны", 0.62m,
        [
            ("DJI Mic 2", 29_990m), ("DJI Mic Mini", 11_990m),
        ]),
        new("Питание", 0.68m,
        [
            ("Аккумулятор Mini 4 Pro", 7_990m), ("Зарядный хаб", 5_990m), ("Зарядная станция Power 1000", 69_990m),
        ]),
        new("Аксессуары", 0.48m,
        [
            ("Набор ND-фильтров", 6_990m), ("Fly More Kit", 24_990m), ("Карта памяти 256 ГБ", 3_490m), ("Защитный кейс", 4_990m),
        ]),
        new("Сервис", 0.30m,
        [
            ("DJI Care Refresh 1 год", 9_990m), ("Настройка и обучение", 7_500m),
        ]),
    ];

    public static List<Category> Categories()
    {
        var categories = new List<Category>();
        var skuCounter = 1;

        foreach (var (categoryName, costRatio, products) in Catalog)
        {
            var category = new Category { Name = categoryName };
            foreach (var (name, price) in products)
            {
                category.Products.Add(new Product
                {
                    Name = name,
                    Sku = $"SKU-{skuCounter++:D4}",
                    Category = category,
                    ListPrice = price,
                    UnitCost = Math.Round(price * costRatio, 2),
                });
            }

            categories.Add(category);
        }

        return categories;
    }

    public static List<(Manager Manager, ManagerProfile Profile)> Managers() =>
    [
        (M("Анна Смирнова", "Корпоративные продажи", "Ведущий менеджер"), ManagerProfile.Star),
        (M("Дмитрий Козлов", "Корпоративные продажи", "Ведущий менеджер"), ManagerProfile.Star.WithSlump(4)),
        (M("Елена Волкова", "Корпоративные продажи", "Менеджер"), ManagerProfile.Solid),
        (M("Игорь Новиков", "Корпоративные продажи", "Менеджер"), ManagerProfile.Discounter),
        (M("Мария Лебедева", "Корпоративные продажи", "Менеджер"), ManagerProfile.Solid),
        (M("Сергей Морозов", "Розница", "Старший продавец"), ManagerProfile.Star),
        (M("Ольга Павлова", "Розница", "Продавец-консультант"), ManagerProfile.Solid),
        (M("Артём Соколов", "Розница", "Продавец-консультант"), ManagerProfile.Discounter),
        (M("Наталья Егорова", "Розница", "Продавец-консультант"), ManagerProfile.Solid.WithSlump(2)),
        (M("Павел Орлов", "Розница", "Продавец-консультант"), ManagerProfile.Newbie),
        (M("Юлия Андреева", "Розница", "Продавец-консультант"), ManagerProfile.Solid),
        (M("Алексей Макаров", "Розница", "Стажёр"), ManagerProfile.Newbie),
        (M("Татьяна Никитина", "Онлайн", "Менеджер интернет-магазина"), ManagerProfile.Discounter),
        (M("Кирилл Захаров", "Онлайн", "Менеджер интернет-магазина"), ManagerProfile.Solid),
        (M("Виктория Зайцева", "Онлайн", "Менеджер интернет-магазина"), ManagerProfile.Star.WithSlump(7)),
        (M("Роман Фёдоров", "Онлайн", "Менеджер интернет-магазина"), ManagerProfile.Solid),
        (M("Екатерина Белова", "Онлайн", "Менеджер интернет-магазина"), ManagerProfile.Newbie),
        (M("Максим Григорьев", "Онлайн", "Стажёр"), ManagerProfile.Newbie),
        (M("Светлана Комарова", "Корпоративные продажи", "Менеджер"), ManagerProfile.Solid),
        (M("Андрей Киселёв", "Розница", "Продавец-консультант", isActive: false), ManagerProfile.Solid.WithSlump(0)),
    ];

    private static readonly string[] FirstNames =
        ["Александр", "Михаил", "Иван", "Николай", "Владимир", "Ирина", "Светлана", "Ксения", "Полина", "Дарья", "Георгий", "Людмила"];

    private static readonly string[] LastNames =
        ["Иванов", "Петров", "Сидоров", "Кузнецов", "Попов", "Васильев", "Смирнов", "Михайлов", "Фролов", "Тихонов", "Гусев", "Титов"];

    private static readonly string[] Companies =
        ["Аэросъёмка", "ГеоПлан", "АгроТех", "МедиаПро", "СтройКонтроль", "ЭнергоСеть", "Северные Линии", "Кадр", "Вектор", "Точка Обзора"];

    public static List<Customer> Customers(Random random, int count)
    {
        var customers = new List<Customer>(count);
        for (var i = 0; i < count; i++)
        {
            var roll = random.NextDouble();
            var segment = roll < 0.6 ? CustomerSegment.Retail : roll < 0.9 ? CustomerSegment.Smb : CustomerSegment.Enterprise;
            var company = segment == CustomerSegment.Retail
                ? "Частное лицо"
                : $"ООО «{Companies[random.Next(Companies.Length)]}{(i % 3 == 0 ? "" : $" {i}")}»";

            customers.Add(new Customer
            {
                Name = $"{FirstNames[random.Next(FirstNames.Length)]} {LastNames[random.Next(LastNames.Length)]}",
                Company = company,
                Segment = segment,
            });
        }

        return customers;
    }

    private static Manager M(string fullName, string team, string position, bool isActive = true) =>
        new() { FullName = fullName, Team = team, Position = position, IsActive = isActive };
}
