"use client";

interface ShiftBadgeProps {
  name: string;
  color: string;
  startTime?: string;
  endTime?: string;
}

export default function ShiftBadge({ name, color, startTime, endTime }: ShiftBadgeProps) {
  return (
    <span
      className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-xs font-medium bg-gray-50 border border-gray-200"
    >
      <span
        className="w-2.5 h-2.5 rounded-full flex-shrink-0"
        style={{ backgroundColor: color }}
      />
      <span className="text-gray-700">{name}</span>
      {startTime && endTime && (
        <span className="text-gray-400 text-[10px]">{startTime}-{endTime}</span>
      )}
    </span>
  );
}
