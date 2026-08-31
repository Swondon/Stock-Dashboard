const RANGE_OPTIONS = [
  { label: "7D", days: 7 },
  { label: "14D", days: 14 },
  { label: "30D", days: 30 },
];

interface RangeSelectorProps {
  value: number;
  onChange: (days: number) => void;
}

/** Segmented 7D/14D/30D toggle. */
export function RangeSelector({ value, onChange }: RangeSelectorProps) {
  return (
    <div className="range-selector" role="group" aria-label="Date range">
      {RANGE_OPTIONS.map((option) => (
        <button
          key={option.days}
          type="button"
          className={option.days === value ? "active" : undefined}
          onClick={() => onChange(option.days)}
        >
          {option.label}
        </button>
      ))}
    </div>
  );
}
