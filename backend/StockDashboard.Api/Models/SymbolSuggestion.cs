namespace StockDashboard.Api.Models;

/// <summary>
/// One autocomplete match returned by <c>GET /api/stocks/search</c>.
/// </summary>
public sealed record SymbolSuggestion(string Symbol, string Name, string Exchange);
