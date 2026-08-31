using StockDashboard.Api.Models;

namespace StockDashboard.Api.Services;

/// <summary>
/// Groups raw 15-minute intraday bars into one row per trading day
/// </summary>
internal sealed class DailyAggregationService : IDailyAggregationService
{
    public IReadOnlyList<DailyAggregate> Aggregate(YahooChartData chartData)
    {

        var buckets = new SortedDictionary<DateOnly, DayBucket>();

        var timestamps = chartData.Timestamps;
        var lows = chartData.Quote.Low;
        var highs = chartData.Quote.High;
        var volumes = chartData.Quote.Volume;

        for (var i = 0; i < timestamps.Length; i++)
        {
            // Matching local time
            var localInstant = DateTimeOffset.FromUnixTimeSeconds(timestamps[i] + chartData.GmtOffsetSeconds).UtcDateTime;
            var day = DateOnly.FromDateTime(localInstant);

            if (!buckets.TryGetValue(day, out var bucket))
            {
                bucket = new DayBucket();
                buckets[day] = bucket;
            }

            // Skip yahoo nulls
            var low = lows is not null && i < lows.Length ? lows[i] : null;
            var high = highs is not null && i < highs.Length ? highs[i] : null;
            var volume = volumes is not null && i < volumes.Length ? volumes[i] : null;

            if (low.HasValue)
            {
                bucket.Lows.Add(low.Value);
            }

            if (high.HasValue)
            {
                bucket.Highs.Add(high.Value);
            }

            if (volume.HasValue)
            {
                bucket.VolumeSum += volume.Value;
            }
        }

        var result = new List<DailyAggregate>(buckets.Count);
        foreach (var (day, bucket) in buckets)
        {
            // Skip 0 days
            if (bucket.Lows.Count == 0 && bucket.Highs.Count == 0)
            {
                continue;
            }

            result.Add(new DailyAggregate(
                day.ToString("yyyy-MM-dd"),
                bucket.Lows.Count > 0 ? Math.Round(bucket.Lows.Average(), 4) : 0,
                bucket.Highs.Count > 0 ? Math.Round(bucket.Highs.Average(), 4) : 0,
                bucket.VolumeSum));
        }

        return result;
    }

    /// <summary>Running totals</summary>
    private sealed class DayBucket
    {
        public List<double> Lows { get; } = new();

        public List<double> Highs { get; } = new();

        public long VolumeSum { get; set; }
    }
}
