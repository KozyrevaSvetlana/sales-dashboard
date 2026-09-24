using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SalesDashboard.Api.Infrastructure.Persistence.Migrations;

// ВНИМАНИЕ: скелет собирался без .NET SDK, поэтому первая миграция написана вручную
// по модели из Configurations/ и без ModelSnapshot. Перед работой пересоздайте её штатно:
//   rm -r src/SalesDashboard.Api/Infrastructure/Persistence/Migrations
//   dotnet ef migrations add InitialCreate -p src/SalesDashboard.Api -o Infrastructure/Persistence/Migrations
// (см. README → «Первый запуск»).
[DbContext(typeof(AppDbContext))]
[Migration("20260924000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "categories",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            },
            constraints: table => table.PrimaryKey("pk_categories", x => x.id));

        migrationBuilder.CreateTable(
            name: "customers",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                segment = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
            },
            constraints: table => table.PrimaryKey("pk_customers", x => x.id));

        migrationBuilder.CreateTable(
            name: "managers",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                team = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
            },
            constraints: table => table.PrimaryKey("pk_managers", x => x.id));

        migrationBuilder.CreateTable(
            name: "products",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                category_id = table.Column<int>(type: "integer", nullable: false),
                list_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                unit_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_products", x => x.id);
                table.ForeignKey(
                    name: "fk_products_categories_category_id",
                    column: x => x.category_id,
                    principalTable: "categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "sales",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                manager_id = table.Column<int>(type: "integer", nullable: false),
                customer_id = table.Column<int>(type: "integer", nullable: false),
                sold_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_sales", x => x.id);
                table.ForeignKey(
                    name: "fk_sales_customers_customer_id",
                    column: x => x.customer_id,
                    principalTable: "customers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_sales_managers_manager_id",
                    column: x => x.manager_id,
                    principalTable: "managers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "sale_items",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                sale_id = table.Column<int>(type: "integer", nullable: false),
                product_id = table.Column<int>(type: "integer", nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                unit_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_sale_items", x => x.id);
                table.CheckConstraint("ck_sale_items_quantity_positive", "quantity > 0");
                table.ForeignKey(
                    name: "fk_sale_items_products_product_id",
                    column: x => x.product_id,
                    principalTable: "products",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_sale_items_sales_sale_id",
                    column: x => x.sale_id,
                    principalTable: "sales",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "ix_categories_name", table: "categories", column: "name", unique: true);
        migrationBuilder.CreateIndex(name: "ix_products_category_id", table: "products", column: "category_id");
        migrationBuilder.CreateIndex(name: "ix_products_sku", table: "products", column: "sku", unique: true);
        migrationBuilder.CreateIndex(name: "ix_sales_customer_id", table: "sales", column: "customer_id");
        migrationBuilder.CreateIndex(name: "ix_sales_manager_id_sold_at_utc", table: "sales", columns: ["manager_id", "sold_at_utc"]);
        migrationBuilder.CreateIndex(name: "ix_sales_sold_at_utc", table: "sales", column: "sold_at_utc");
        migrationBuilder.CreateIndex(name: "ix_sales_status_sold_at_utc", table: "sales", columns: ["status", "sold_at_utc"]);
        migrationBuilder.CreateIndex(name: "ix_sale_items_product_id", table: "sale_items", column: "product_id");
        migrationBuilder.CreateIndex(name: "ix_sale_items_sale_id", table: "sale_items", column: "sale_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "sale_items");
        migrationBuilder.DropTable(name: "products");
        migrationBuilder.DropTable(name: "sales");
        migrationBuilder.DropTable(name: "categories");
        migrationBuilder.DropTable(name: "customers");
        migrationBuilder.DropTable(name: "managers");
    }
}
