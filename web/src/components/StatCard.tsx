"use client";

interface StatCardProps {
  label: string;
  value: string | number;
  sub?: string;
  color: string;
  trend?: number[];
  trendColor?: string;
}

export default function StatCard({ label, value, sub, color }: StatCardProps) {
  return (
    <div className={`rounded-xl border p-4 ${color}`}>
      <p className="text-sm font-medium opacity-70">{label}</p>
      <p className="text-2xl font-bold mt-1">{value}</p>
      {sub && <p className="text-xs opacity-60 mt-0.5">{sub}</p>}
    </div>
  );
}
