using InvoiceKeeper.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceKeeper.Api.Middleware;

public sealed class ApiExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionHandlingMiddleware> _logger;

    public ApiExceptionHandlingMiddleware(RequestDelegate next, ILogger<ApiExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            _logger.LogInformation(
                "API error {StatusCode} on {Method} {Path}: {Message}",
                exception.StatusCode,
                context.Request.Method,
                context.Request.Path,
                exception.Message);

            context.Response.Clear();
            context.Response.StatusCode = exception.StatusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = exception.StatusCode,
                Title = exception.Title,
                Detail = exception.Message,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}

