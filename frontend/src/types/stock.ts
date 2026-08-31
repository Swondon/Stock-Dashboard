/** One trading day's aggregated data from `GET /api/stocks/{symbol}/daily-summary`. */
export interface DailySummary {
  day: string;
  lowAverage: number;
  highAverage: number;
  volume: number;
}

/** Shape of the backend's RFC 7807 ProblemDetails error body. */
export interface ApiErrorResponse {
  title?: string;
  detail?: string;
  status?: number;
}

/** One autocomplete match from `GET /api/stocks/search`. */
export interface SymbolSuggestion {
  symbol: string;
  name: string;
  exchange: string;
}
