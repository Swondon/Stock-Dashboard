using StockDashboard.Api.Models;

namespace StockDashboard.Api.Services;

/// <summary>
/// Groups raw 15-minute intraday bars into one row per trading day
/// </summary>
internal interface IDailyAggregationService
{
    IReadOnlyList<DailyAggregate> Aggregate(YahooChartData chartData);
}
