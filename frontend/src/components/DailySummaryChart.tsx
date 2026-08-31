import {
  Area,
  CartesianGrid,
  ComposedChart,
  Legend,
  Line,
  ReferenceDot,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import type { TooltipContentProps } from "recharts";
import { useIsMobile } from "../hooks/useIsMobile";
import type { DailySummary } from "../types/stock";

interface DailySummaryChartProps {
  data: DailySummary[];
}

const COLORS = {
  high: "#15803d",
  low: "#dc2626",
  grid: "rgba(255, 255, 255, 0.08)",
  axisLine: "rgba(255, 255, 255, 0.2)",
  tick: "#7a7a7a",
  textPrimary: "#ffffff",
  surface: "#0a0a0a",
};

/**
 * Custom tooltip content.
 */
function CustomTooltip({ active, payload, label }: TooltipContentProps) {
  if (!active || !payload || payload.length === 0) {
    return null;
  }

  const byKey = new Map<string, (typeof payload)[number]>();
  for (const entry of payload) {
    if (typeof entry.dataKey === "string") {
      byKey.set(entry.dataKey, entry);
    }
  }

  return (
    <div className="chart-tooltip">
      <div className="chart-tooltip-label">{label}</div>
      {Array.from(byKey.values()).map((entry) => (
        <div className="chart-tooltip-row" key={String(entry.dataKey)}>
          <span className="chart-tooltip-key" style={{ background: entry.color }} />
          <span className="chart-tooltip-name">{entry.name}</span>
          <span className="chart-tooltip-value">
            {typeof entry.value === "number" ? entry.value.toFixed(4) : entry.value}
          </span>
        </div>
      ))}
    </div>
  );
}

/** Line + gradient-area chart of daily low/high averages. */
export function DailySummaryChart({ data }: DailySummaryChartProps) {
  const last = data[data.length - 1];
  const isMobile = useIsMobile();

  return (
    <div className="summary-chart">
      <div className="chart-container">
        <ResponsiveContainer width="100%" height="100%">
          <ComposedChart
            data={data}
            margin={{ top: 8, right: isMobile ? 8 : 24, bottom: 8, left: 0 }}
          >
            <defs>
              <linearGradient id="highAreaFill" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor={COLORS.high} stopOpacity={0.18} />
                <stop offset="100%" stopColor={COLORS.high} stopOpacity={0} />
              </linearGradient>
            </defs>

            <CartesianGrid stroke={COLORS.grid} strokeDasharray="0" vertical={false} />
            <XAxis
              dataKey="day"
              tick={{ fill: COLORS.tick, fontSize: isMobile ? 10 : 11, fontFamily: "JetBrains Mono, monospace" }}
              tickLine={false}
              axisLine={{ stroke: COLORS.axisLine }}
              minTickGap={isMobile ? 12 : 24}
              tickFormatter={(day: string) => day.slice(5)}
            />
            <YAxis
              tick={{ fill: COLORS.tick, fontSize: isMobile ? 10 : 11, fontFamily: "JetBrains Mono, monospace" }}
              tickLine={false}
              axisLine={false}
              width={isMobile ? 38 : 56}
              domain={["auto", "auto"]}
              tickFormatter={(value: number) => `$${value.toFixed(0)}`}
            />
            <Tooltip content={CustomTooltip} cursor={{ stroke: COLORS.axisLine, strokeWidth: 1 }} />
            <Legend
              iconType="plainline"
              wrapperStyle={{ paddingTop: 12, fontSize: isMobile ? 11 : 13, fontFamily: "Inter, sans-serif" }}
            />

            <Area
              type="monotone"
              dataKey="highAverage"
              name="High avg"
              stroke="none"
              fill="url(#highAreaFill)"
              isAnimationActive={false}
              legendType="none"
            />
            <Line
              type="monotone"
              dataKey="highAverage"
              name="High avg"
              stroke={COLORS.high}
              strokeWidth={2}
              dot={false}
              activeDot={{ r: 5, strokeWidth: 2, stroke: COLORS.surface }}
            />
            <Line
              type="monotone"
              dataKey="lowAverage"
              name="Low avg"
              stroke={COLORS.low}
              strokeWidth={2}
              dot={false}
              activeDot={{ r: 5, strokeWidth: 2, stroke: COLORS.surface }}
            />

            {last && (
              <>
                <ReferenceDot
                  x={last.day}
                  y={last.highAverage}
                  r={5}
                  fill={COLORS.high}
                  stroke={COLORS.surface}
                  strokeWidth={2}
                />
                <ReferenceDot
                  x={last.day}
                  y={last.lowAverage}
                  r={5}
                  fill={COLORS.low}
                  stroke={COLORS.surface}
                  strokeWidth={2}
                />
              </>
            )}
          </ComposedChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
