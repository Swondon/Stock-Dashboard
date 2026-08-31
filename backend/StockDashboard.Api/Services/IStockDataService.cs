using StockDashboard.Api.Models;

namespace StockDashboard.Api.Services;

/// <summary>
/// Orchestrates a daily-summary request
/// </summary>
public interface IStockDataService
{
    /// <summary>
    /// Validates <paramref name="symbol"/>, then returns its last-month daily summary
    /// </summary>
    Task<IReadOnlyList<DailyAggregate>> GetDailySummaryAsync(string symbol, CancellationToken cancellationToken);

    /// <summary>
    /// Ticker/company-name autocomplete matches for <paramref name="query"/>
    /// </summary>
    Task<IReadOnlyList<SymbolSuggestion>> SearchSymbolsAsync(string query, CancellationToken cancellationToken);
}
