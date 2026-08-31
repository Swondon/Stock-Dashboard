using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StockDashboard.Api.Exceptions;

namespace StockDashboard.Api.ErrorHandling;

/// <summary>
/// Every unhandled exception in the app gets turned into an HTTP response.
/// </summary>
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Map our own domain exceptions to a specific status code or 500
        var (statusCode, title) = exception switch
        {
            InvalidStockSymbolException => (StatusCodes.Status400BadRequest, "Invalid stock symbol"),
            StockSymbolNotFoundException => (StatusCodes.Status404NotFound, "Stock symbol not found"),
            StockDataUnavailableException => (StatusCodes.Status502BadGateway, "Upstream market data provider unavailable"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        // Unexpected failures logged
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception processing {Path}", httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning("Handled exception ({StatusCode}) processing {Path}: {Message}", statusCode, httpContext.Request.Path, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;
        // Standardize JSON shape
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        }, cancellationToken);

        return true;
    }
}
