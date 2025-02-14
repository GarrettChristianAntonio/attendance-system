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
    <div className="min-h-screen bg-gray-50 py-8 px-4">
      <div className="max-w-4xl mx-auto">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold">Attendance Log</h1>
          <div className="flex gap-4 items-center">
            <Link
              href="/"
              className="text-gray-600 hover:text-gray-800 font-medium"
            >
              &larr; Camera
            </Link>
            <input
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              className="rounded-lg border border-gray-300 px-3 py-2"
            />
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-6">
          {loading ? (
            <p className="text-gray-500 text-center py-8">Loading...</p>
          ) : records.length === 0 ? (
            <p className="text-gray-500 text-center py-8">
              No attendance records for this date.
            </p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full border-collapse">
                <thead>
                  <tr className="border-b border-gray-200">
                    <th className="text-left py-3 px-4 font-medium text-gray-600">
                      Employee
                    </th>
                    <th className="text-left py-3 px-4 font-medium text-gray-600">
                      Check-in Time
                    </th>
                    <th className="text-left py-3 px-4 font-medium text-gray-600">
                      Confidence
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {records.map((record) => {
                    const photo = getPhotoUrl(record.photoUrl);
                    const confidence = Math.max(
                      0,
                      (1 - record.confidence) * 100
                    );
                    return (
                      <tr
                        key={record.id}
                        className="border-b border-gray-100 hover:bg-gray-50"
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
                              <div className="w-8 h-8 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 text-xs">
                                {record.employeeName.charAt(0)}
                              </div>
                            )}
                            <span className="font-medium">
                              {record.employeeName}
                            </span>
                          </div>
                        </td>
                        <td className="py-3 px-4 text-gray-600">
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
  );
}
