using System.Net.Http.Json;
using System.Text.Json;
using StockDashboard.Api.Exceptions;
using StockDashboard.Api.Models;

namespace StockDashboard.Api.Services;

/// <summary>
/// Wraps Yahoo Finance's unofficial unauthenticated HTTP endpoints
/// </summary>
internal sealed class YahooFinanceClient : IYahooFinanceClient
{
    // Fix keys
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<YahooFinanceClient> _logger;

    public YahooFinanceClient(HttpClient httpClient, ILogger<YahooFinanceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<YahooChartData> GetIntradayChartAsync(string symbol, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        // interval=15m matches
        var requestUri = $"v8/finance/chart/{Uri.EscapeDataString(symbol)}" +
                          $"?interval=15m&period1={from.ToUnixTimeSeconds()}&period2={to.ToUnixTimeSeconds()}";

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<YahooChartResponse>(SerializerOptions, cancellationToken);

        // An invalid/delisted symbol comes back from Yahoo
        var error = payload?.Chart?.Error;
        if (error is not null)
        {
            throw new StockSymbolNotFoundException(symbol, error.Description ?? error.Code ?? "Unknown upstream error.");
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Yahoo Finance returned {StatusCode} for symbol {Symbol}", response.StatusCode, symbol);
            throw new StockDataUnavailableException(symbol, $"Upstream provider returned HTTP {(int)response.StatusCode}.");
        }

        // Yahoo's response wraps a single result in an array
        var result = payload?.Chart?.Result?.FirstOrDefault();
        var quote = result?.Indicators?.Quote?.FirstOrDefault();

        if (result?.Timestamp is null || quote is null)
        {
            throw new StockSymbolNotFoundException(symbol, "No intraday data returned for symbol.");
        }

        return new YahooChartData(result.Timestamp, quote, result.Meta?.GmtOffset ?? 0);
    }

    public async Task<IReadOnlyList<SymbolSuggestion>> SearchSymbolsAsync(string query, CancellationToken cancellationToken)
    {
        var requestUri = $"v1/finance/search?q={Uri.EscapeDataString(query)}&quotesCount=8&newsCount=0";

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // Log bad autocomplete but dont throw
            _logger.LogWarning("Yahoo Finance search returned {StatusCode} for query {Query}", response.StatusCode, query);
            return Array.Empty<SymbolSuggestion>();
        }

        var payload = await response.Content.ReadFromJsonAsync<YahooSearchResponse>(SerializerOptions, cancellationToken);
        var quotes = payload?.Quotes ?? new List<YahooSearchQuote>();

        return quotes
            // get rid of everything else but stocks
            .Where(quote => !string.IsNullOrWhiteSpace(quote.Symbol)
                             && string.Equals(quote.QuoteType, "EQUITY", StringComparison.OrdinalIgnoreCase))
            .Select(quote => new SymbolSuggestion(
                quote.Symbol!,
                quote.Longname ?? quote.Shortname ?? quote.Symbol!,
                quote.ExchDisp ?? string.Empty))
            .ToList();
    }
}
