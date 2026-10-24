namespace InvoiceKeeper.Application.Common;

/// <summary>Ошибка бизнес-логики, которая преобразуется в ProblemDetails с заданным HTTP-статусом.</summary>
public class ApiException : Exception
{
    public ApiException(int statusCode, string message, string? title = null)
        : base(message)
    {
        StatusCode = statusCode;
        Title = title ?? DefaultTitle(statusCode);
    }

    public int StatusCode { get; }
    public string Title { get; }

    private static string DefaultTitle(int statusCode) => statusCode switch
    {
        400 => "Некорректный запрос",
        401 => "Требуется аутентификация",
        403 => "Доступ запрещён",
        404 => "Ресурс не найден",
        409 => "Конфликт состояния",
        _ => "Ошибка обработки запроса"
    };
}

public sealed class BadRequestException(string message)
    : ApiException(StatusCodes.Status400BadRequest, message);

public sealed class UnauthorizedException(string message)
    : ApiException(StatusCodes.Status401Unauthorized, message);

public sealed class ForbiddenException(string message)
    : ApiException(StatusCodes.Status403Forbidden, message);

public sealed class NotFoundException(string message)
    : ApiException(StatusCodes.Status404NotFound, message);

public sealed class ConflictException(string message)
    : ApiException(StatusCodes.Status409Conflict, message);

