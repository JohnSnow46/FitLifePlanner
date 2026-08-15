using FitLifePlanner.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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
            case DbUpdateException dbUpdateException when IsForeignKeyViolation(dbUpdateException):
                logger.LogWarning(exception, "DbUpdateException (FK violation). TraceId: {TraceId}", httpContext.TraceIdentifier);
                return (StatusCodes.Status409Conflict, "Resource in use", "The resource cannot be deleted or modified because it is referenced by other records.");
            default:
                logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", httpContext.TraceIdentifier);
                var detail = environment.IsDevelopment() ? exception.Message : null;
                return (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", detail);
        }
    }

    // Only a SQLite foreign key constraint violation means "referenced by other records".
    // Other DbUpdateException causes (unique constraint, NOT NULL, concurrency, truncation)
    // are unrelated write failures and must fall through to the default 500 handling instead
    // of being masked as a 409.
    // SqliteErrorCode 19 is SQLITE_CONSTRAINT; the FK-specific extended code differs by
    // scenario (787 SQLITE_CONSTRAINT_FOREIGNKEY on insert/update of the child row, 1811
    // SQLITE_CONSTRAINT_TRIGGER when the violation is caught deleting the referenced parent
    // row instead), so match on the message SQLite always uses for this constraint family.
    private static bool IsForeignKeyViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteErrorCode: 19 } sqliteException &&
        sqliteException.Message.Contains("FOREIGN KEY", StringComparison.Ordinal);
}
