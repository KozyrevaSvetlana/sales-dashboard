using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SalesDashboard.Api.Common;
using SalesDashboard.Api.Domain.Periods;
using SalesDashboard.Api.Features.Categories;
using SalesDashboard.Api.Features.Kpi;
using SalesDashboard.Api.Features.Ranking;
using SalesDashboard.Api.Features.RecentSales;
using SalesDashboard.Api.Features.Trends;
using SalesDashboard.Api.Infrastructure;
using SalesDashboard.Api.Infrastructure.Persistence;
using SalesDashboard.Api.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// --- Инфраструктура ---
string connection = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Не задана строка подключения ConnectionStrings:Default в appsettings.json.");

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(connection, npgsql =>
    {
        npgsql.EnableRetryOnFailure(maxRetryCount: 5);
        npgsql.UseAdminDatabase("template1");
    })
    .UseSnakeCaseNamingConvention());

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(sp => new PeriodResolver(
    sp.GetRequiredService<TimeProvider>(),
    TimeZoneInfo.FindSystemTimeZoneById(config["Dashboard:TimeZone"] ?? "Europe/Moscow")));

builder.Services.AddScoped<DataSeeder>();

// --- Фичи ---
builder.Services.AddScoped<KpiService>();
builder.Services.AddScoped<RankingService>();
builder.Services.AddScoped<TrendsService>();
builder.Services.AddScoped<CategoriesService>();
builder.Services.AddScoped<RecentSalesService>();

// --- HTTP ---
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Swashbuckle строит схемы по MVC JsonOptions — дублируем настройку, чтобы enum'ы
// и в OpenAPI были строками ("GrossProfit"), как в реальных ответах API.
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Точная схема для генерации TypeScript-типов (npm run gen:api во frontend/):
    options.SupportNonNullableReferenceTypes();              // string ≠ string | null
    options.UseAllOfToExtendReferenceSchemas();              // nullable-ссылки: TopManagerDto | null
    options.SchemaFilter<RequireAllPropertiesSchemaFilter>(); // все поля ответа всегда присутствуют
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();
// Scalar — интерактивная документация API: http://localhost:<порт>/scalar/v1
// Описание API берёт из того же swagger.json, что генерирует Swashbuckle.
app.MapScalarApiReference(options => options
    .WithTitle("Sales Dashboard API")
    .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json"));

await DatabaseInitializer.InitializeAsync(app.Services, app.Lifetime.ApplicationStopping);

var api = app.MapGroup("/api");
api.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithTags("System");
api.MapKpiEndpoints();
api.MapRankingEndpoints();
api.MapTrendsEndpoints();
api.MapCategoriesEndpoints();
api.MapRecentSalesEndpoints();

app.Run();

/// <summary>Нужен для интеграционных тестов через WebApplicationFactory.</summary>
public partial class Program
{
}
