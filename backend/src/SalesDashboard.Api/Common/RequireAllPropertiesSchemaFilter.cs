using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SalesDashboard.Api.Common;

/// <summary>
/// Помечает все свойства DTO как обязательные в OpenAPI-схеме.
///
/// API всегда отдаёт все поля (null — тоже значение), поэтому для TypeScript-типов,
/// сгенерированных из схемы, правильнее `margin: number | null`, чем `margin?: number | null`.
/// Иначе фронтенду пришлось бы везде проверять `undefined`, которого на самом деле не бывает.
/// </summary>
internal sealed class RequireAllPropertiesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties is { Count: > 0 })
        {
            schema.Required = new SortedSet<string>(schema.Properties.Keys, StringComparer.Ordinal);
        }
    }
}
