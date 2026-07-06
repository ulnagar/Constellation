namespace Constellation.Presentation.Server.Helpers.ExceptionHandlers;

using Core.Abstractions.Services;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using Serilog.Events;

/// <summary>
/// Catches unhandled exceptions, maps them to appropriate HTTP status codes,
/// logs them with user context, and redirects to the Constellation error page.
/// Registered via AddExceptionHandler and invoked by UseExceptionHandler middleware.
/// </summary>
internal sealed class ConstellationExceptionHandler : IExceptionHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ConstellationExceptionHandler(
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _currentUserService = currentUserService;
        _logger = logger.ForContext<ConstellationExceptionHandler>();
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, logLevel) = ClassifyException(exception);

        _logger
            .ForContext(nameof(_currentUserService.UserName), _currentUserService.UserName)
            .ForContext(nameof(_currentUserService.EmailAddress), _currentUserService.EmailAddress)
            .ForContext(nameof(HttpContext.TraceIdentifier), httpContext.TraceIdentifier)
            .ForContext(nameof(StatusCodes), statusCode)
            .Write(logLevel, exception, "Unhandled exception: {ExceptionMessage}", exception.Message);

        // Redirect to the error page with the mapped status code and the original URL.
        // The URL is URL-encoded so it survives as a single query string value.
        var failingUrl = Uri.EscapeDataString(
            httpContext.Request.Path + httpContext.Request.QueryString);

        // Redirect to the error page with the mapped status code.
        // Using a redirect here (rather than re-execution) keeps the error page
        // handler simple — OnGet receives statusCode directly from the query string —
        // and avoids any concern about response state left over from the failed request.
        httpContext.Response.Redirect($"/Error?statusCode={statusCode}&failingUrl={failingUrl}");

        // Return true to signal that this exception is fully handled.
        // The redirect response will be sent; UseExceptionHandler will not re-execute.
        return true;
    }

    /// <summary>
    /// Maps an exception type to an HTTP status code and an appropriate Serilog log level.
    /// Add new cases here as domain-specific exception types are introduced.
    /// </summary>
    private static (int StatusCode, LogEventLevel LogLevel) ClassifyException(Exception exception) =>
        exception switch
        {
            // ── Domain exceptions ──────────────────────────────────────────────────
            // Map your own exception types here, e.g.:
            //   NotFoundException     => 422 (record not found / invalid ID)
            //   ValidationException   => 422
            //   DomainException       => 422

            // ── Framework / BCL exceptions ─────────────────────────────────────────
            // UnauthorizedAccessException is thrown by code, not the auth middleware.
            // Auth middleware 403s are handled separately by UseStatusCodePagesWithReExecute.
            UnauthorizedAccessException =>
                (StatusCodes.Status403Forbidden, LogEventLevel.Warning),

            OperationCanceledException =>
                (StatusCodes.Status503ServiceUnavailable, LogEventLevel.Information),

            // ── Catch-all ─────────────────────────────────────────────────────────
            _ => (StatusCodes.Status500InternalServerError, LogEventLevel.Error)
        };
}