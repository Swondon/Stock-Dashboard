// Quick-pick chips shown before any search has been made
const EXAMPLE_SYMBOLS = ["TSLA", "AAPL", "MSFT", "NVDA", "GOOGL"];

const FEATURES = [
  {
    label: "Daily aggregates",
    detail: "15-minute bars from the last month, grouped into per-day low/high averages and total volume.",
  },
  {
    label: "Chart + table",
    detail: "See the trend at a glance, then check the exact figures behind it.",
  },
  {
    label: "Adjustable range",
    detail: "Zoom the chart to the last 7, 14, or 30 days.",
  },
  {
    label: "Live market data",
    detail: "Pulled straight from Yahoo Finance's intraday feed — nothing mocked.",
  },
];

interface EmptyStateProps {
  onSelectSymbol: (symbol: string) => void;
}

/** Landing content shown before the user has searched for anything */
export function EmptyState({ onSelectSymbol }: EmptyStateProps) {
  return (
    <section className="empty-state">
      <p className="empty-state-lead">
        Enter a stock symbol above and hit search to pull the last month of intraday data.
      </p>

      <div className="empty-state-examples">
        <span className="empty-state-examples-label">Try</span>
        {EXAMPLE_SYMBOLS.map((symbol) => (
          <button key={symbol} type="button" onClick={() => onSelectSymbol(symbol)}>
            {symbol}
          </button>
        ))}
      </div>

      <ul className="empty-state-features">
        {FEATURES.map((feature) => (
          <li key={feature.label}>
            <span className="empty-state-feature-label">{feature.label}</span>
            <span className="empty-state-feature-detail">{feature.detail}</span>
          </li>
        ))}
      </ul>
    </section>
  );
}
