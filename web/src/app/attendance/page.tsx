"use client";

import { useEffect, useState, useMemo } from "react";
import { apiFetch, getPhotoUrl } from "@/lib/api";

interface AttendanceRecord {
  id: string;
  employeeId: string;
  employeeName: string;
  photoUrl: string | null;
  checkInAt: string;
  confidence: number;
}

function StatCard({
  label,
  value,
  sub,
  color,
}: {
  label: string;
  value: string | number;
  sub?: string;
  color: string;
}) {
  return (
    <div className={`rounded-xl border p-4 ${color}`}>
      <p className="text-sm font-medium opacity-70">{label}</p>
      <p className="text-2xl font-bold mt-1">{value}</p>
      {sub && <p className="text-xs opacity-60 mt-0.5">{sub}</p>}
    </div>
  );
}

function SkeletonRow() {
  return (
    <tr className="border-b border-gray-50">
      <td className="py-4 px-4">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-full bg-gray-200 animate-pulse" />
          <div className="h-4 w-24 bg-gray-200 rounded animate-pulse" />
        </div>
      </td>
      <td className="py-4 px-4">
        <div className="h-4 w-20 bg-gray-200 rounded animate-pulse" />
      </td>
      <td className="py-4 px-4">
        <div className="h-6 w-12 bg-gray-200 rounded animate-pulse" />
      </td>
    </tr>
  );
}

export default function AttendancePage() {
  const [records, setRecords] = useState<AttendanceRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [date, setDate] = useState(() => {
    const today = new Date();
    return today.toISOString().split("T")[0];
  });

  useEffect(() => {
    const fetchRecords = async () => {
      setLoading(true);
      try {
        const data = await apiFetch<AttendanceRecord[]>(
          `/api/attendance?date=${date}`
        );
        setRecords(data);
      } catch (err) {
        console.error("Failed to fetch attendance:", err);
      } finally {
        setLoading(false);
      }
    };

    fetchRecords();
  }, [date]);

  const stats = useMemo(() => {
    if (records.length === 0) return null;
    const uniqueEmployees = new Set(records.map((r) => r.employeeId)).size;
    const avgConfidence =
      records.reduce((sum, r) => sum + Math.max(0, (1 - r.confidence) * 100), 0) /
      records.length;
    const firstCheckIn = records.length > 0
      ? new Date(records[records.length - 1].checkInAt).toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        })
      : "-";
    const lastCheckIn = records.length > 0
      ? new Date(records[0].checkInAt).toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        })
      : "-";
    return { uniqueEmployees, avgConfidence, firstCheckIn, lastCheckIn };
  }, [records]);

  return (
    <div className="py-6 sm:py-8 px-4">
      <div className="max-w-4xl mx-auto">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Attendance Log</h1>
            <p className="text-sm text-gray-500 mt-1">
              {!loading && records.length > 0
                ? `${records.length} check-in${records.length !== 1 ? "s" : ""} recorded`
                : "View daily check-in records"}
            </p>
          </div>
          <input
            type="date"
            value={date}
            onChange={(e) => setDate(e.target.value)}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm bg-white"
          />
        </div>

        {!loading && stats && (
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-6">
            <StatCard
              label="Employees"
              value={stats.uniqueEmployees}
              sub="unique today"
              color="bg-blue-50 border-blue-100 text-blue-900"
            />
            <StatCard
              label="Check-ins"
              value={records.length}
              sub="total records"
              color="bg-green-50 border-green-100 text-green-900"
            />
            <StatCard
              label="Avg. Confidence"
              value={`${stats.avgConfidence.toFixed(0)}%`}
              sub="recognition"
              color="bg-purple-50 border-purple-100 text-purple-900"
            />
            <StatCard
              label="First / Last"
              value={stats.firstCheckIn}
              sub={`Last: ${stats.lastCheckIn}`}
              color="bg-amber-50 border-amber-100 text-amber-900"
            />
          </div>
        )}

        <div className="bg-white rounded-xl shadow-sm border border-gray-100">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200">
                  <th className="text-left py-3 px-4 font-semibold text-gray-700 text-sm">Employee</th>
                  <th className="text-left py-3 px-4 font-semibold text-gray-700 text-sm">Check-in Time</th>
                  <th className="text-left py-3 px-4 font-semibold text-gray-700 text-sm">Confidence</th>
                </tr>
              </thead>
              <tbody>
                {loading ? (
                  <>
                    <SkeletonRow />
                    <SkeletonRow />
                    <SkeletonRow />
                  </>
                ) : records.length === 0 ? (
                  <tr>
                    <td colSpan={3} className="py-12 text-center">
                      <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-3">
                        <svg className="w-6 h-6 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                      </div>
                      <p className="text-gray-500">No check-ins for this date</p>
                    </td>
                  </tr>
                ) : (
                  records.map((record) => {
                    const photo = getPhotoUrl(record.photoUrl);
                    const confidence = Math.max(0, (1 - record.confidence) * 100);
                    return (
                      <tr
                        key={record.id}
                        className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors"
                      >
                        <td className="py-4 px-4">
                          <div className="flex items-center gap-3">
                            {photo ? (
                              <img
                                src={photo}
                                alt={record.employeeName}
                                className="w-8 h-8 rounded-full object-cover"
                              />
                            ) : (
                              <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 text-xs font-semibold">
                                {record.employeeName.charAt(0).toUpperCase()}
                              </div>
                            )}
                            <span className="font-medium text-gray-900">
                              {record.employeeName}
                            </span>
                          </div>
                        </td>
                        <td className="py-4 px-4 text-gray-600 text-sm">
                          {new Date(record.checkInAt).toLocaleTimeString()}
                        </td>
                        <td className="py-4 px-4">
                          <span
                            className={`inline-block px-2.5 py-1 rounded-full text-xs font-medium ${
                              confidence > 80
                                ? "bg-green-100 text-green-700"
                                : confidence > 60
                                ? "bg-yellow-100 text-yellow-700"
                                : "bg-red-100 text-red-700"
                            }`}
                          >
                            {confidence.toFixed(0)}%
                          </span>
                        </td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
