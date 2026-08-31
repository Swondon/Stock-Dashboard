namespace StockDashboard.Api.Models;

/// <summary>
/// The trimmed-down data <see cref="Services.IYahooFinanceClient"/> hands to
/// <see cref="Services.IDailyAggregationService"/>
/// </summary>
internal sealed record YahooChartData(long[] Timestamps, YahooQuote Quote, int GmtOffsetSeconds);
