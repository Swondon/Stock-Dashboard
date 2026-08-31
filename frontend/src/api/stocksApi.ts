import type { ApiErrorResponse, DailySummary, SymbolSuggestion } from "../types/stock";

// Falls back to the local backend's default port
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5080";

/** Thrown by fetchDailySummary*/
export class StockApiError extends Error {
  status?: number;

  constructor(message: string, status?: number) {
    super(message);
    this.name = "StockApiError";
    this.status = status;
  }
}

/**
 * Fetches the last month of daily-aggregated intraday data for `symbol`. Throws
 * `StockApiError` on any non-2xx response
 */
export async function fetchDailySummary(symbol: string, signal?: AbortSignal): Promise<DailySummary[]> {
  const response = await fetch(`${API_BASE_URL}/api/stocks/${encodeURIComponent(symbol)}/daily-summary`, { signal });

  if (!response.ok) {
    let message = `Request failed with status ${response.status}.`;
    try {
      const problem = (await response.json()) as ApiErrorResponse;
      message = problem.detail ?? problem.title ?? message;
    } catch {
      // Response body wasn't JSON
    }
    throw new StockApiError(message, response.status);
  }

  return (await response.json()) as DailySummary[];
}

/**
 * Ticker/company-name autocomplete matches for `query`, used by SymbolForm's suggestions
 * dropdown. Never throws - a failed request or non-2xx response just resolves to an empty
 * list.
 */
export async function searchSymbols(query: string, signal?: AbortSignal): Promise<SymbolSuggestion[]> {
  const response = await fetch(`${API_BASE_URL}/api/stocks/search?q=${encodeURIComponent(query)}`, { signal });

  if (!response.ok) {
    return [];
  }

  return (await response.json()) as SymbolSuggestion[];
}
