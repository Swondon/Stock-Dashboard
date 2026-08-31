namespace StockDashboard.Api.Models;

// Deserialization targets for Yahoo Finance's symbol-search endpoint

internal sealed class YahooSearchResponse
{
    public List<YahooSearchQuote>? Quotes { get; init; }
}

internal sealed class YahooSearchQuote
{
    public string? Symbol { get; init; }

    public string? Shortname { get; init; }

    /// <summary>Full company name</summary>
    public string? Longname { get; init; }

    /// <summary>Human-readable exchange name</summary>
    public string? ExchDisp { get; init; }

    /// <summary>"EQUITY", "ETF", "MUTUALFUND", etc used to filter out nonstock results</summary>
    public string? QuoteType { get; init; }
}
