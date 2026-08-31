namespace StockDashboard.Api.Exceptions;

// Handling exceptions

/// <summary>Common base fpr GlobalExceptionHandler</summary>
internal abstract class StockDashboardException : Exception
{
    protected StockDashboardException(string message) : base(message)
    {
    }
}

/// <summary>The requested symbol failed format validation -- 400</summary>
internal sealed class InvalidStockSymbolException : StockDashboardException
{
    public InvalidStockSymbolException(string? symbol)
        : base($"'{symbol}' is not a valid stock symbol.")
    {
    }
}

/// <summary>No data from Yahoo -- 404</summary>
internal sealed class StockSymbolNotFoundException : StockDashboardException
{
    public StockSymbolNotFoundException(string symbol, string reason)
        : base($"No market data found for symbol '{symbol}': {reason}")
    {
    }
}

/// <summary>Yahoo failure -- 502</summary>
internal sealed class StockDataUnavailableException : StockDashboardException
{
    public StockDataUnavailableException(string symbol, string reason)
        : base($"Market data provider unavailable for symbol '{symbol}': {reason}")
    {
    }
}
