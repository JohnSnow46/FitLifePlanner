using FitLifePlanner.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FitLifePlanner.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = MapException(exception, httpContext);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private (int StatusCode, string Title, string? Detail) MapException(Exception exception, HttpContext httpContext)
    {
        switch (exception)
        {
            case ValidationException:
                return (StatusCodes.Status400BadRequest, "Validation failed", exception.Message);
            case NotFoundException:
                return (StatusCodes.Status404NotFound, "Resource not found", exception.Message);
            default:
                logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", httpContext.TraceIdentifier);
                var detail = environment.IsDevelopment() ? exception.Message : null;
                return (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", detail);
        }
    }
}
