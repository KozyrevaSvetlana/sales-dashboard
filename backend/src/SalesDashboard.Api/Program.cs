using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Common;
using SalesDashboard.Api.Domain.Periods;
using SalesDashboard.Api.Features.Kpi;
using SalesDashboard.Api.Features.Ranking;
using SalesDashboard.Api.Infrastructure;
using SalesDashboard.Api.Infrastructure.Persistence;
using SalesDashboard.Api.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// --- Инфраструктура ---
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(config.GetConnectionString("Default"), npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 5))
    .UseSnakeCaseNamingConvention());

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(sp => new PeriodResolver(
    sp.GetRequiredService<TimeProvider>(),
    TimeZoneInfo.FindSystemTimeZoneById(config["Dashboard:TimeZone"] ?? "Europe/Moscow")));

builder.Services.AddScoped<DataSeeder>();

// --- Фичи ---
builder.Services.AddScoped<KpiService>();
builder.Services.AddScoped<RankingService>();
// TODO: TrendsService, CategoriesService, RecentSalesService

// --- HTTP ---
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();

await DatabaseInitializer.InitializeAsync(app.Services);

var api = app.MapGroup("/api");
api.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithTags("System");
api.MapKpiEndpoints();
api.MapRankingEndpoints();

app.Run();

/// <summary>Нужен для интеграционных тестов через WebApplicationFactory.</summary>
public partial class Program
{
}
