using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StockDashboard.Api.Exceptions;
using StockDashboard.Api.Models;
using StockDashboard.Api.Options;

namespace StockDashboard.Api.Services;

/// <summary>
/// Orchestrates a daily-summary request
/// </summary>
internal sealed partial class StockDataService : IStockDataService
{
    private static readonly TimeSpan LookbackWindow = TimeSpan.FromDays(31);

    private readonly IYahooFinanceClient _client;
    private readonly IDailyAggregationService _aggregator;
    private readonly IMemoryCache _cache;
    private readonly TimeProvider _timeProvider;
    private readonly YahooFinanceOptions _options;

    public StockDataService(
        IYahooFinanceClient client,
        IDailyAggregationService aggregator,
        IMemoryCache cache,
        TimeProvider timeProvider,
        IOptions<YahooFinanceOptions> options)
    {
        _client = client;
        _aggregator = aggregator;
        _cache = cache;
        _timeProvider = timeProvider;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<DailyAggregate>> GetDailySummaryAsync(string symbol, CancellationToken cancellationToken)
    {
        // Reject anything that cant be a  ticker
        if (string.IsNullOrWhiteSpace(symbol) || !SymbolPattern().IsMatch(symbol))
        {
            throw new InvalidStockSymbolException(symbol);
        }

        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var cacheKey = $"daily-summary:{normalizedSymbol}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<DailyAggregate>? cached) && cached is not null)
        {
            return cached;
        }

        var now = _timeProvider.GetUtcNow();
        var chartData = await _client.GetIntradayChartAsync(normalizedSymbol, now - LookbackWindow, now, cancellationToken);
        var summary = _aggregator.Aggregate(chartData);

        _cache.Set(cacheKey, summary, TimeSpan.FromSeconds(_options.CacheDurationSeconds));

        return summary;
    }

    public async Task<IReadOnlyList<SymbolSuggestion>> SearchSymbolsAsync(string query, CancellationToken cancellationToken)
    {
        // Expty queries ok
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<SymbolSuggestion>();
        }

        return await _client.SearchSymbolsAsync(query.Trim(), cancellationToken);
    }

    [GeneratedRegex(@"^[A-Za-z0-9.\-]{1,10}$")]
    private static partial Regex SymbolPattern();
}
