using Microsoft.AspNetCore.Mvc;
using StockDashboard.Api.Models;
using StockDashboard.Api.Services;

namespace StockDashboard.Api.Controllers;

/// <summary>
/// HTTP surface for stock market data. Validation, caching, and other logic in <see cref="IStockDataService"/>. controller only translates HTTP requests into service calls and service results into HTTP responses.
/// </summary>
[ApiController]
[Route("api/stocks")]
public sealed class StocksController : ControllerBase
{
    private readonly IStockDataService _stockDataService;

    public StocksController(IStockDataService stockDataService)
    {
        _stockDataService = stockDataService;
    }

    /// <summary>
    /// Returns the last month of intraday data for <paramref name="symbol"/>.
    /// </summary>
    /// <remarks>
    /// Errors (invalid symbol format, symbol not found, upstream provider failure) thrown as exceptions from service layer and converted into the right HTTP status code by <see cref="StockDashboard.Api.ErrorHandling.GlobalExceptionHandler"/>
    /// </remarks>
    [HttpGet("{symbol}/daily-summary")]
    [ProducesResponseType(typeof(IReadOnlyList<DailyAggregate>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DailyAggregate>>> GetDailySummary(string symbol, CancellationToken cancellationToken)
    {
        var summary = await _stockDataService.GetDailySummaryAsync(symbol, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Ticker/company-name autocomplete, used by the frontend's search-as-you-type dropdown. Always returns 200 with a list that could be empty. No match is not treated as an error
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<SymbolSuggestion>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SymbolSuggestion>>> Search([FromQuery] string? q, CancellationToken cancellationToken)
    {
        var suggestions = await _stockDataService.SearchSymbolsAsync(q ?? string.Empty, cancellationToken);
        return Ok(suggestions);
    }
}
