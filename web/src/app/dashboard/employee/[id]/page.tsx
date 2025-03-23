"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { apiFetch, getPhotoUrl } from "@/lib/api";
import BarChart from "@/components/BarChart";
import StatCard from "@/components/StatCard";

interface EmployeeAnalytics {
  employeeId: string;
  employeeName: string;
  photoUrl: string | null;
  totalDays: number;
  onTimeCount: number;
  lateCount: number;
  absentCount: number;
  earlyDepartureCount: number;
  totalHours: number;
  avgArrivalTime: string | null;
  dailyHistory: Array<{
    date: string;
    dayLabel: string;
    checkIns: number;
    onTime: number;
    late: number;
  }>;
}

export default function EmployeeDetailPage() {
  const params = useParams();
  const id = params.id as string;
  const [data, setData] = useState<EmployeeAnalytics | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetch = async () => {
      try {
        const d = await apiFetch<EmployeeAnalytics>(`/api/analytics/employee/${id}`);
        setData(d);
      } catch (err) {
        console.error("Failed to load employee analytics:", err);
      } finally {
        setLoading(false);
      }
    };
    fetch();
  }, [id]);

  if (loading) {
    return (
      <div className="max-w-4xl mx-auto px-4 sm:px-6 py-6">
        <div className="text-center py-12 text-gray-500">Loading...</div>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="max-w-4xl mx-auto px-4 sm:px-6 py-6">
        <div className="text-center py-12 text-gray-500">Employee not found</div>
      </div>
    );
  }

  const onTimeRate = data.totalDays > 0
    ? Math.round((data.onTimeCount / (data.onTimeCount + data.lateCount)) * 100)
    : 0;

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 py-6">
      <a href="/dashboard" className="text-sm text-blue-600 hover:text-blue-700 mb-4 inline-block">
        &larr; Back to Dashboard
      </a>

      <div className="flex items-center gap-4 mb-6">
        {data.photoUrl ? (
          <img
            src={getPhotoUrl(data.photoUrl) || ""}
            alt=""
            className="w-16 h-16 rounded-full object-cover"
          />
        ) : (
          <div className="w-16 h-16 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 text-2xl font-medium">
            {data.employeeName.charAt(0)}
          </div>
        )}
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{data.employeeName}</h1>
          {data.avgArrivalTime && (
            <p className="text-sm text-gray-500">Avg. arrival: {data.avgArrivalTime}</p>
          )}
        </div>
      </div>

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-6">
        <StatCard
          label="Days"
          value={data.totalDays}
          sub="attended"
          color="bg-blue-50 border-blue-100 text-blue-900"
        />
        <StatCard
          label="On-Time Rate"
          value={`${onTimeRate}%`}
          sub={`${data.onTimeCount} on time`}
          color="bg-green-50 border-green-100 text-green-900"
        />
        <StatCard
          label="Hours"
          value={data.totalHours}
          sub="total worked"
          color="bg-purple-50 border-purple-100 text-purple-900"
        />
        <StatCard
          label="Absences"
          value={data.absentCount}
          sub={`${data.lateCount} late`}
          color="bg-red-50 border-red-100 text-red-900"
        />
      </div>

      {data.dailyHistory.length > 0 && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-sm font-semibold text-gray-700 mb-4">Daily History</h2>
          <BarChart
            data={data.dailyHistory.map((d) => ({
              label: d.dayLabel,
              value: d.onTime + d.late,
              color: d.late > 0 ? "#F59E0B" : "#10B981",
            }))}
            height={180}
          />
        </div>
      )}
    </div>
  );
}
