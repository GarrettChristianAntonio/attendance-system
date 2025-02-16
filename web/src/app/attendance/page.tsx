"use client";

import { useEffect, useState } from "react";
import { apiFetch, getPhotoUrl } from "@/lib/api";
import Link from "next/link";

interface AttendanceRecord {
  id: string;
  employeeId: string;
  employeeName: string;
  photoUrl: string | null;
  checkInAt: string;
  confidence: number;
}

function SkeletonRow() {
  return (
    <tr className="border-b border-gray-100">
      <td className="py-3 px-4">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-full bg-gray-200 animate-pulse" />
          <div className="h-4 w-24 bg-gray-200 rounded animate-pulse" />
        </div>
      </td>
      <td className="py-3 px-4">
        <div className="h-4 w-20 bg-gray-200 rounded animate-pulse" />
      </td>
      <td className="py-3 px-4">
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

  return (
    <div className="min-h-screen bg-gray-50 py-6 sm:py-8 px-4">
      <div className="max-w-4xl mx-auto">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6 sm:mb-8">
          <h1 className="text-2xl sm:text-3xl font-bold">Attendance Log</h1>
          <div className="flex gap-3 sm:gap-4 items-center">
            <Link
              href="/"
              className="text-gray-600 hover:text-gray-800 font-medium text-sm sm:text-base"
            >
              &larr; Camera
            </Link>
            <input
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              className="rounded-lg border border-gray-300 px-3 py-2 text-sm"
            />
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm">
          {!loading && records.length > 0 && (
            <div className="px-4 sm:px-6 pt-4 sm:pt-5 pb-2">
              <p className="text-sm text-gray-500">
                {records.length} check-in{records.length !== 1 ? "s" : ""} recorded
              </p>
            </div>
          )}

          <div className="p-4 sm:p-6 pt-2">
            {loading ? (
              <div className="overflow-x-auto">
                <table className="w-full border-collapse">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left py-3 px-4 font-medium text-gray-600 text-sm">Employee</th>
                      <th className="text-left py-3 px-4 font-medium text-gray-600 text-sm">Check-in Time</th>
                      <th className="text-left py-3 px-4 font-medium text-gray-600 text-sm">Confidence</th>
                    </tr>
                  </thead>
                  <tbody>
                    <SkeletonRow />
                    <SkeletonRow />
                    <SkeletonRow />
                  </tbody>
                </table>
              </div>
            ) : records.length === 0 ? (
              <p className="text-gray-500 text-center py-8">
                No attendance records for this date.
              </p>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full border-collapse">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left py-3 px-4 font-medium text-gray-600 text-sm">Employee</th>
                      <th className="text-left py-3 px-4 font-medium text-gray-600 text-sm">Check-in Time</th>
                      <th className="text-left py-3 px-4 font-medium text-gray-600 text-sm">Confidence</th>
                    </tr>
                  </thead>
                  <tbody>
                    {records.map((record) => {
                      const photo = getPhotoUrl(record.photoUrl);
                      const confidence = Math.max(0, (1 - record.confidence) * 100);
                      return (
                        <tr
                          key={record.id}
                          className="border-b border-gray-100 hover:bg-gray-50 transition-colors"
                        >
                          <td className="py-3 px-4">
                            <div className="flex items-center gap-3">
                              {photo ? (
                                <img
                                  src={photo}
                                  alt={record.employeeName}
                                  className="w-8 h-8 rounded-full object-cover"
                                />
                              ) : (
                                <div className="w-8 h-8 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 text-xs font-medium">
                                  {record.employeeName.charAt(0)}
                                </div>
                              )}
                              <span className="font-medium text-sm sm:text-base">
                                {record.employeeName}
                              </span>
                            </div>
                          </td>
                          <td className="py-3 px-4 text-gray-600 text-sm">
                            {new Date(record.checkInAt).toLocaleTimeString()}
                          </td>
                          <td className="py-3 px-4">
                            <span
                              className={`inline-block px-2 py-1 rounded text-xs font-medium ${
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
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
