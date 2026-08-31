namespace StockDashboard.Api.Options;

/// <summary>
/// Class for easily dealing with Yahoo API settings
/// </summary>
public sealed class YahooFinanceOptions
{
    /// <summary>Base URL</summary>
    public string BaseUrl { get; init; } = "https://query1.finance.yahoo.com/";

    /// <summary>HTTP timeout</summary>
    public int TimeoutSeconds { get; init; } = 10;

    /// <summary>How long a symbol's daily-summary result stays in cache</summary>
    public int CacheDurationSeconds { get; init; } = 60;
}
