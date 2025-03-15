"use client";

import { getPhotoUrl } from "@/lib/api";
import ShiftBadge from "./ShiftBadge";

interface ShiftSlot {
  scheduleId: string;
  shiftId: string;
  shiftName: string;
  shiftColor: string;
  startTime: string;
  endTime: string;
}

interface ScheduleEntry {
  employeeId: string;
  employeeName: string;
  photoUrl: string | null;
  days: Record<number, ShiftSlot[]>;
}

interface WeeklyCalendarProps {
  weekStart: string;
  entries: ScheduleEntry[];
  onDeleteSchedule?: (scheduleId: string) => void;
}

const DAY_LABELS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

export default function WeeklyCalendar({ weekStart, entries, onDeleteSchedule }: WeeklyCalendarProps) {
  const startDate = new Date(weekStart + "T00:00:00");

  const formatDate = (dayIndex: number) => {
    const d = new Date(startDate);
    d.setDate(d.getDate() + dayIndex);
    return `${d.getMonth() + 1}/${d.getDate()}`;
  };

  if (entries.length === 0) {
    return (
      <div className="text-center py-12 text-gray-500">
        <p className="text-lg">No schedules found for this week</p>
        <p className="text-sm mt-1">Assign shifts to employees to see them here</p>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full border-collapse">
        <thead>
          <tr>
            <th className="text-left p-3 bg-gray-50 border border-gray-200 min-w-[160px] text-sm font-medium text-gray-600">
              Employee
            </th>
            {DAY_LABELS.map((label, i) => (
              <th key={i} className="text-center p-3 bg-gray-50 border border-gray-200 min-w-[120px]">
                <div className="text-sm font-medium text-gray-600">{label}</div>
                <div className="text-xs text-gray-400">{formatDate(i)}</div>
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {entries.map((entry) => (
            <tr key={entry.employeeId}>
              <td className="p-3 border border-gray-200">
                <div className="flex items-center gap-2">
                  {entry.photoUrl && (
                    <img
                      src={getPhotoUrl(entry.photoUrl) || ""}
                      alt=""
                      className="w-7 h-7 rounded-full object-cover"
                    />
                  )}
                  <span className="text-sm font-medium text-gray-900 truncate">
                    {entry.employeeName}
                  </span>
                </div>
              </td>
              {DAY_LABELS.map((_, dayIndex) => {
                const slots = entry.days[dayIndex] || [];
                return (
                  <td key={dayIndex} className="p-2 border border-gray-200 align-top">
                    <div className="flex flex-col gap-1">
                      {slots.map((slot) => (
                        <div key={slot.scheduleId} className="group relative">
                          <ShiftBadge
                            name={slot.shiftName}
                            color={slot.shiftColor}
                            startTime={slot.startTime}
                            endTime={slot.endTime}
                          />
                          {onDeleteSchedule && (
                            <button
                              onClick={() => onDeleteSchedule(slot.scheduleId)}
                              className="absolute -top-1 -right-1 w-4 h-4 bg-red-500 text-white rounded-full text-[10px] leading-none opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center"
                            >
                              x
                            </button>
                          )}
                        </div>
                      ))}
                    </div>
                  </td>
                );
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
