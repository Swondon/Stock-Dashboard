using StockDashboard.Api.Models;

namespace StockDashboard.Api.Services;

/// <summary>
/// Wraps Yahoo Finance's unofficial unauthenticated HTTP endpoints
/// </summary>
internal interface IYahooFinanceClient
{
    /// <summary>
    /// Fetches raw 15-minute intraday bars
    /// </summary>
    /// <exception cref="Exceptions.StockSymbolNotFoundException">
    /// The symbol doesn't exist or Yahoo returned no usable data
    /// </exception>
    /// <exception cref="Exceptions.StockDataUnavailableException">
    /// Yahoo's endpoint failed
    /// </exception>
    Task<YahooChartData> GetIntradayChartAsync(string symbol, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    /// <summary>
    /// Ticker/company-name search-as-you-type matches
    /// </summary>
    Task<IReadOnlyList<SymbolSuggestion>> SearchSymbolsAsync(string query, CancellationToken cancellationToken);
}
