using StockDashboard.Api.Models;
using StockDashboard.Api.Services;
using Xunit;

namespace StockDashboard.Api.Tests;

/// <summary>
/// <see cref="DailyAggregationService"/> test with input arrays
/// </summary>
public class DailyAggregationServiceTests
{
    private readonly DailyAggregationService _sut = new();

    [Fact]
    public void Aggregate_GroupsBarsByLocalTradingDay()
    {
        var timestamps = new long[] { 1704204000, 1704204900, 1704290400 };

        var quote = new YahooQuote
        {
            Low = new double?[] { 100.0, 102.0, 200.0 },
            High = new double?[] { 110.0, 112.0, 210.0 },
            Volume = new long?[] { 1000, 2000, 5000 }
        };

        var chartData = new YahooChartData(timestamps, quote, -18000);

        var result = _sut.Aggregate(chartData);

        Assert.Equal(2, result.Count);

        Assert.Equal("2024-01-02", result[0].Day);
        Assert.Equal(101.0, result[0].LowAverage);
        Assert.Equal(111.0, result[0].HighAverage);
        Assert.Equal(3000, result[0].Volume);

        Assert.Equal("2024-01-03", result[1].Day);
        Assert.Equal(200.0, result[1].LowAverage);
        Assert.Equal(210.0, result[1].HighAverage);
        Assert.Equal(5000, result[1].Volume);
    }

    [Fact]
    public void Aggregate_SkipsNullSamplesWithoutSkewingAverage()
    {
        var timestamps = new long[] { 1704204000, 1704204900 };
        var quote = new YahooQuote
        {
            Low = new double?[] { 100.0, null },
            High = new double?[] { 110.0, null },
            Volume = new long?[] { 1000, null }
        };

        var chartData = new YahooChartData(timestamps, quote, -18000);

        var result = _sut.Aggregate(chartData);

        Assert.Single(result);
        Assert.Equal(100.0, result[0].LowAverage);
        Assert.Equal(110.0, result[0].HighAverage);
        Assert.Equal(1000, result[0].Volume);
    }

    [Fact]
    public void Aggregate_ReturnsEmptyList_WhenNoTimestamps()
    {
        var chartData = new YahooChartData(Array.Empty<long>(), new YahooQuote(), 0);

        var result = _sut.Aggregate(chartData);

        Assert.Empty(result);
    }
}
