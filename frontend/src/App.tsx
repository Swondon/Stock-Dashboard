import { useState } from "react";
import "./App.css";
import { StockApiError, fetchDailySummary } from "./api/stocksApi";
import { DailySummaryChart } from "./components/DailySummaryChart";
import { DailySummaryTable } from "./components/DailySummaryTable";
import { EmptyState } from "./components/EmptyState";
import { ErrorBanner } from "./components/ErrorBanner";
import { RangeSelector } from "./components/RangeSelector";
import { SymbolForm } from "./components/SymbolForm";
import type { DailySummary } from "./types/stock";

type ViewState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; message: string }
  | { status: "success"; data: DailySummary[]; symbol: string };

/**
 * Filtering for range selector
 */
function filterByRangeDays(data: DailySummary[], days: number): DailySummary[] {
  if (data.length === 0) {
    return data;
  }
  const latest = new Date(`${data[data.length - 1].day}T00:00:00Z`);
  const cutoff = new Date(latest);
  cutoff.setUTCDate(cutoff.getUTCDate() - (days - 1));
  return data.filter((row) => new Date(`${row.day}T00:00:00Z`) >= cutoff);
}

function App() {
  const [state, setState] = useState<ViewState>({ status: "idle" });
  const [rangeDays, setRangeDays] = useState(30);
  const [symbolInput, setSymbolInput] = useState("TSLA");

  async function handleSearch(symbol: string) {
    setState({ status: "loading" });
    try {
      const data = await fetchDailySummary(symbol);
      if (data.length === 0) {
        setState({ status: "error", message: `No intraday data available for "${symbol}".` });
        return;
      }
      setState({ status: "success", data, symbol });
    } catch (error) {
      const message = error instanceof StockApiError
        ? error.message
        : "Could not reach the backend API. Is it running?";
      setState({ status: "error", message });
    }
  }

  function handleSelectExample(symbol: string) {
    setSymbolInput(symbol);
    void handleSearch(symbol);
  }

  return (
    <div className="app">
      <header className="page-header">
        <h1>Stock Dashboard</h1>
      </header>

      <SymbolForm
        value={symbolInput}
        onChange={setSymbolInput}
        onSubmit={handleSearch}
        isLoading={state.status === "loading"}
      />

      {state.status === "idle" && <EmptyState onSelectSymbol={handleSelectExample} />}

      {state.status === "error" && <ErrorBanner message={state.message} />}

      {state.status === "success" && (
        <section className="results">
          <div className="results-header">
            <h2>{state.symbol}</h2>
            <RangeSelector value={rangeDays} onChange={setRangeDays} />
          </div>
          <DailySummaryChart data={filterByRangeDays(state.data, rangeDays)} />
          <DailySummaryTable data={filterByRangeDays(state.data, rangeDays)} />
        </section>
      )}
    </div>
  );
}

export default App;
