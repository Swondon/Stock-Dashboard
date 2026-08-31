import type { DailySummary } from "../types/stock";

interface DailySummaryTableProps {
  data: DailySummary[];
}

/** Tabular view of the same data DailySummaryChart plots */
export function DailySummaryTable({ data }: DailySummaryTableProps) {
  return (
    // horizontally on narrow screens
    <div className="summary-table-wrap">
      <table className="summary-table">
        <thead>
          <tr>
            <th>Day</th>
            <th>Low avg</th>
            <th>High avg</th>
            <th>Volume</th>
          </tr>
        </thead>
        <tbody>
          {data.map((row) => (
            <tr key={row.day}>
              <td>{row.day}</td>
              <td>{row.lowAverage.toFixed(4)}</td>
              <td>{row.highAverage.toFixed(4)}</td>
              <td>{row.volume.toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
