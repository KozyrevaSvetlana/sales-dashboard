namespace SalesDashboard.Api.Common;

/// <summary>
/// Ошибка входных данных, понятная пользователю. GlobalExceptionHandler превращает её в 400 ProblemDetails.
/// </summary>
public sealed class BadRequestException(string message) : Exception(message);
