namespace StockDashboard.Api.Models;

// Deserialization targets for Yahoo Finance's chart endpoint

/// <summary>Top-level envelope Yahoo wraps every chart response in.</summary>
internal sealed class YahooChartResponse
{
    public YahooChart? Chart { get; init; }
}

internal sealed class YahooChart
{
    /// <summary>Array with max one entry per requested symbol</summary>
    public List<YahooChartResult>? Result { get; init; }

    /// <summary>Null when symbol is invalid</summary>
    public YahooChartError? Error { get; init; }
}

internal sealed class YahooChartError
{
    public string? Code { get; init; }

    public string? Description { get; init; }
}

internal sealed class YahooChartResult
{
    public YahooChartMeta? Meta { get; init; }

    /// <summary>Unix seconds </summary>
    public long[]? Timestamp { get; init; }

    public YahooIndicators? Indicators { get; init; }
}

internal sealed class YahooChartMeta
{
    /// <summary>Exchange's UTC offset in seconds</summary>
    public int GmtOffset { get; init; }
}

internal sealed class YahooIndicators
{
    /// <summary>list for working with Yahoo's API</summary>
    public List<YahooQuote>? Quote { get; init; }
}

/// <summary>Parallel arrays any entry can be null for a bar with missing data</summary>
internal sealed class YahooQuote
{
    public double?[]? Low { get; init; }

    public double?[]? High { get; init; }

    public long?[]? Volume { get; init; }
}
