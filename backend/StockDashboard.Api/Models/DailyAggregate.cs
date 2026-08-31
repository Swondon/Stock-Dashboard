namespace StockDashboard.Api.Models;

/// <summary>
/// One trading day's worth of intraday data, aggregated to match the JSON shape needed.
/// </summary>
/// <param name="Day">Calendar date (yyyy-MM-dd) in the exchange's local timezone.</param>
/// <param name="LowAverage">Mean of that day's bar lows, rounded to 4 decimal places.</param>
/// <param name="HighAverage">Mean of that day's bar highs, rounded to 4 decimal places.</param>
/// <param name="Volume">Sum of that day's bar volumes.</param>
public sealed record DailyAggregate(
    string Day,
    double LowAverage,
    double HighAverage,
    long Volume);
