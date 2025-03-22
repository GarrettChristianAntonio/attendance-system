"use client";

interface TrendLineProps {
  data: number[];
  width?: number;
  height?: number;
  color?: string;
}

export default function TrendLine({ data, width = 200, height = 60, color = "#3B82F6" }: TrendLineProps) {
  if (data.length < 2) return null;

  const maxVal = Math.max(...data, 1);
  const minVal = Math.min(...data, 0);
  const range = maxVal - minVal || 1;
  const padding = 4;

  const points = data.map((v, i) => {
    const x = padding + (i / (data.length - 1)) * (width - 2 * padding);
    const y = height - padding - ((v - minVal) / range) * (height - 2 * padding);
    return `${x},${y}`;
  });

  const areaPoints = [
    `${padding},${height - padding}`,
    ...points,
    `${width - padding},${height - padding}`,
  ];

  return (
    <svg width={width} height={height} className="overflow-visible">
      <defs>
        <linearGradient id={`gradient-${color}`} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor={color} stopOpacity="0.2" />
          <stop offset="100%" stopColor={color} stopOpacity="0.02" />
        </linearGradient>
      </defs>
      <polygon
        points={areaPoints.join(" ")}
        fill={`url(#gradient-${color})`}
      />
      <polyline
        points={points.join(" ")}
        fill="none"
        stroke={color}
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      {points.map((p, i) => {
        const [x, y] = p.split(",");
        return (
          <circle
            key={i}
            cx={x}
            cy={y}
            r="2.5"
            fill="white"
            stroke={color}
            strokeWidth="1.5"
          />
        );
      })}
    </svg>
  );
}
