using Microsoft.Extensions.Caching.Memory;
using StockDashboard.Api.Exceptions;
using StockDashboard.Api.Models;
using StockDashboard.Api.Options;
using StockDashboard.Api.Services;
using Xunit;

namespace StockDashboard.Api.Tests;

/// <summary>
/// Covers StockDataService's error paths
/// </summary>
public class StockDataServiceTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("TOOLONGTICKER")]
    [InlineData("BAD$YMBOL")]
    public async Task GetDailySummaryAsync_RejectsInvalidSymbols_WithoutCallingUpstream(string symbol)
    {
        var client = new FakeYahooFinanceClient();
        var service = CreateService(client);

        await Assert.ThrowsAsync<InvalidStockSymbolException>(
            () => service.GetDailySummaryAsync(symbol, CancellationToken.None));

        Assert.False(client.ChartRequested);
    }

    [Fact]
    public async Task SearchSymbolsAsync_ReturnsEmpty_ForBlankQuery_WithoutCallingUpstream()
    {
        var client = new FakeYahooFinanceClient();
        var service = CreateService(client);

        var result = await service.SearchSymbolsAsync("   ", CancellationToken.None);

        Assert.Empty(result);
        Assert.False(client.SearchRequested);
    }

    private static StockDataService CreateService(IYahooFinanceClient client)
    {
        return new StockDataService(
            client,
            new FakeDailyAggregationService(),
            new NoOpMemoryCache(),
            TimeProvider.System,
            Microsoft.Extensions.Options.Options.Create(new YahooFinanceOptions()));
    }

    private sealed class FakeYahooFinanceClient : IYahooFinanceClient
    {
        public bool ChartRequested { get; private set; }

        public bool SearchRequested { get; private set; }

        public Task<YahooChartData> GetIntradayChartAsync(string symbol, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
        {
            ChartRequested = true;
            return Task.FromResult(new YahooChartData(Array.Empty<long>(), new YahooQuote(), 0));
        }

        public Task<IReadOnlyList<SymbolSuggestion>> SearchSymbolsAsync(string query, CancellationToken cancellationToken)
        {
            SearchRequested = true;
            return Task.FromResult<IReadOnlyList<SymbolSuggestion>>(Array.Empty<SymbolSuggestion>());
        }
    }

    private sealed class FakeDailyAggregationService : IDailyAggregationService
    {
        public IReadOnlyList<DailyAggregate> Aggregate(YahooChartData chartData) => Array.Empty<DailyAggregate>();
    }

    private sealed class NoOpMemoryCache : IMemoryCache
    {
        public bool TryGetValue(object key, out object? value)
        {
            value = null;
            return false;
        }

        public ICacheEntry CreateEntry(object key) => throw new NotSupportedException();

        public void Remove(object key)
        {
        }

        public void Dispose()
        {
        }
    }
}
