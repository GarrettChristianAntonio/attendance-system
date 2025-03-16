"use client";

import { useEffect, useState } from "react";
import { apiFetch, getPhotoUrl } from "@/lib/api";
import StatusBadge from "@/components/StatusBadge";

interface EmployeeSummary {
  employeeId: string;
  employeeName: string;
  photoUrl: string | null;
  totalDays: number;
  onTimeCount: number;
  lateCount: number;
  earlyDepartureCount: number;
  absentCount: number;
  totalHours: number;
  avgArrivalTime: string | null;
}

export default function AttendanceSummaryPage() {
  const [summary, setSummary] = useState<EmployeeSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [from, setFrom] = useState(() => {
    const d = new Date();
    d.setDate(d.getDate() - 30);
    return d.toISOString().slice(0, 10);
  });
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10));

  const fetchSummary = async () => {
    setLoading(true);
    try {
      const data = await apiFetch<EmployeeSummary[]>(`/api/attendance/summary?from=${from}&to=${to}`);
      setSummary(data);
    } catch (err) {
      console.error("Failed to load summary:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSummary();
  }, [from, to]);

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 py-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Attendance Summary</h1>
          <p className="text-sm text-gray-500 mt-1">Per-employee attendance statistics</p>
        </div>
        <a
          href="/attendance"
          className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
        >
          Back to Log
        </a>
      </div>

      <div className="flex items-center gap-3 mb-6">
        <div>
          <label className="block text-xs text-gray-500 mb-1">From</label>
          <input
            type="date"
            value={from}
            onChange={(e) => setFrom(e.target.value)}
            className="rounded-lg border border-gray-300 px-3 py-1.5 text-sm"
          />
        </div>
        <div>
          <label className="block text-xs text-gray-500 mb-1">To</label>
          <input
            type="date"
            value={to}
            onChange={(e) => setTo(e.target.value)}
            className="rounded-lg border border-gray-300 px-3 py-1.5 text-sm"
          />
        </div>
      </div>

      {loading ? (
        <div className="text-center py-12 text-gray-500">Loading summary...</div>
      ) : summary.length === 0 ? (
        <div className="text-center py-12 text-gray-500">No data for selected period</div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {summary.map((emp) => (
            <div key={emp.employeeId} className="bg-white rounded-xl border border-gray-200 p-4">
              <div className="flex items-center gap-3 mb-3">
                {emp.photoUrl ? (
                  <img
                    src={getPhotoUrl(emp.photoUrl) || ""}
                    alt=""
                    className="w-10 h-10 rounded-full object-cover"
                  />
                ) : (
                  <div className="w-10 h-10 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 text-sm font-medium">
                    {emp.employeeName.charAt(0)}
                  </div>
                )}
                <div>
                  <div className="font-medium text-gray-900">{emp.employeeName}</div>
                  {emp.avgArrivalTime && (
                    <div className="text-xs text-gray-500">Avg arrival: {emp.avgArrivalTime}</div>
                  )}
                </div>
              </div>
              <div className="grid grid-cols-2 gap-2 text-sm">
                <div className="bg-gray-50 rounded-lg p-2">
                  <div className="text-gray-500 text-xs">Days</div>
                  <div className="font-semibold text-gray-900">{emp.totalDays}</div>
                </div>
                <div className="bg-gray-50 rounded-lg p-2">
                  <div className="text-gray-500 text-xs">Hours</div>
                  <div className="font-semibold text-gray-900">{emp.totalHours}h</div>
                </div>
                <div className="bg-green-50 rounded-lg p-2">
                  <div className="text-green-600 text-xs">On Time</div>
                  <div className="font-semibold text-green-700">{emp.onTimeCount}</div>
                </div>
                <div className="bg-yellow-50 rounded-lg p-2">
                  <div className="text-yellow-600 text-xs">Late</div>
                  <div className="font-semibold text-yellow-700">{emp.lateCount}</div>
                </div>
                <div className="bg-orange-50 rounded-lg p-2">
                  <div className="text-orange-600 text-xs">Early Dep.</div>
                  <div className="font-semibold text-orange-700">{emp.earlyDepartureCount}</div>
                </div>
                <div className="bg-red-50 rounded-lg p-2">
                  <div className="text-red-600 text-xs">Absent</div>
                  <div className="font-semibold text-red-700">{emp.absentCount}</div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
