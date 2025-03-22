"use client";

interface BarChartProps {
  data: Array<{ label: string; value: number; color?: string }>;
  height?: number;
  showValues?: boolean;
}

export default function BarChart({ data, height = 160, showValues = true }: BarChartProps) {
  const maxValue = Math.max(...data.map((d) => d.value), 1);

  return (
    <div className="flex items-end gap-2" style={{ height }}>
      {data.map((item, i) => {
        const barHeight = (item.value / maxValue) * (height - 30);
        return (
          <div key={i} className="flex flex-col items-center flex-1 gap-1">
            {showValues && (
              <span className="text-[10px] text-gray-500 font-medium">
                {item.value}
              </span>
            )}
            <div
              className="w-full rounded-t-md transition-all duration-300 min-h-[2px]"
              style={{
                height: barHeight,
                backgroundColor: item.color || "#3B82F6",
              }}
            />
            <span className="text-[10px] text-gray-400">{item.label}</span>
          </div>
        );
      })}
    </div>
  );
}
