using Microsoft.AspNetCore.Diagnostics;

namespace SalesDashboard.Api.Common;

/// <summary>
/// Единая точка превращения исключений в ответы формата ProblemDetails (RFC 7807).
/// Фронтенд всегда получает одинаковую структуру ошибки и может показать понятный текст.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            BadRequestException => (StatusCodes.Status400BadRequest, "Некорректный запрос"),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Некорректные параметры запроса"),
            OperationCanceledException when httpContext.RequestAborted.IsCancellationRequested =>
                (ClientClosedRequest, "Запрос отменён клиентом"),
            _ => (StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера"),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Необработанное исключение при запросе {Path}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = title,
                // Детали внутренних ошибок наружу не отдаём.
                Detail = status == StatusCodes.Status500InternalServerError ? null : exception.Message,
            },
        });
    }
}
