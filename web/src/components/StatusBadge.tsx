"use client";

const statusConfig: Record<string, { bg: string; text: string; label: string }> = {
  OnTime: { bg: "bg-green-100", text: "text-green-700", label: "On Time" },
  Late: { bg: "bg-yellow-100", text: "text-yellow-700", label: "Late" },
  Absent: { bg: "bg-red-100", text: "text-red-700", label: "Absent" },
  EarlyDeparture: { bg: "bg-orange-100", text: "text-orange-700", label: "Early Departure" },
};

export default function StatusBadge({ status }: { status: string }) {
  const config = statusConfig[status] || statusConfig.OnTime;

  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${config.bg} ${config.text}`}>
      {config.label}
    </span>
  );
}
