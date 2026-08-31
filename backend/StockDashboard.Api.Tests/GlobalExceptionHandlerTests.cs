using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using StockDashboard.Api.ErrorHandling;
using StockDashboard.Api.Exceptions;
using Xunit;

namespace StockDashboard.Api.Tests;

/// <summary>
/// Verifies each domain exception
/// </summary>
public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task MapsInvalidStockSymbolException_To400()
    {
        var (statusCode, body) = await HandleAsync(new InvalidStockSymbolException("bad"));

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.Equal("Invalid stock symbol", body?.Title);
    }

    [Fact]
    public async Task MapsStockSymbolNotFoundException_To404()
    {
        var (statusCode, body) = await HandleAsync(new StockSymbolNotFoundException("ZZZ", "not found"));

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
        Assert.Equal("Stock symbol not found", body?.Title);
    }

    [Fact]
    public async Task MapsStockDataUnavailableException_To502()
    {
        var (statusCode, body) = await HandleAsync(new StockDataUnavailableException("TSLA", "upstream down"));

        Assert.Equal(StatusCodes.Status502BadGateway, statusCode);
        Assert.Equal("Upstream market data provider unavailable", body?.Title);
    }

    [Fact]
    public async Task MapsUnrecognizedException_To500()
    {
        // Any exception that isnt handled
        var (statusCode, body) = await HandleAsync(new InvalidOperationException("boom"));

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.Equal("An unexpected error occurred", body?.Title);
    }

    private static async Task<(int StatusCode, ProblemDetails? Body)> HandleAsync(Exception exception)
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var context = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() }
        };

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        Assert.True(handled);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);

        return (context.Response.StatusCode, body);
    }
}
