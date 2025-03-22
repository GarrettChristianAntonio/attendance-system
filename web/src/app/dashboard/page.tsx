"use client";

import { useEffect, useState } from "react";
import { apiFetch, getPhotoUrl } from "@/lib/api";
import BarChart from "@/components/BarChart";
import StatCard from "@/components/StatCard";
import Link from "next/link";

interface DailyStats {
  date: string;
  dayLabel: string;
  checkIns: number;
  onTime: number;
  late: number;
  absent: number;
}

interface TopEmployee {
  employeeId: string;
  employeeName: string;
  photoUrl: string | null;
  onTimeCount: number;
  totalDays: number;
  onTimeRate: number;
}

interface Dashboard {
  totalEmployees: number;
  todayCheckIns: number;
  onTimeToday: number;
  lateToday: number;
  onTimeRate: number;
  weeklyStats: DailyStats[];
  topEmployees: TopEmployee[];
}

export default function DashboardPage() {
  const [data, setData] = useState<Dashboard | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetch = async () => {
      try {
        const d = await apiFetch<Dashboard>("/api/analytics/dashboard");
        setData(d);
      } catch (err) {
        console.error("Failed to load dashboard:", err);
      } finally {
        setLoading(false);
      }
    };
    fetch();
  }, []);

  if (loading) {
    return (
      <div className="max-w-6xl mx-auto px-4 sm:px-6 py-6">
        <div className="text-center py-12 text-gray-500">Loading dashboard...</div>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="max-w-6xl mx-auto px-4 sm:px-6 py-6">
        <div className="text-center py-12 text-gray-500">Failed to load dashboard</div>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 py-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-sm text-gray-500 mt-1">Attendance overview and analytics</p>
      </div>

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-6">
        <StatCard
          label="Employees"
          value={data.totalEmployees}
          sub="active"
          color="bg-blue-50 border-blue-100 text-blue-900"
        />
        <StatCard
          label="Today"
          value={data.todayCheckIns}
          sub="check-ins"
          color="bg-green-50 border-green-100 text-green-900"
        />
        <StatCard
          label="On Time"
          value={`${data.onTimeRate}%`}
          sub={`${data.onTimeToday} on time, ${data.lateToday} late`}
          color="bg-emerald-50 border-emerald-100 text-emerald-900"
        />
        <StatCard
          label="Late Today"
          value={data.lateToday}
          sub="arrivals"
          color="bg-amber-50 border-amber-100 text-amber-900"
        />
      </div>

      <div className="grid sm:grid-cols-2 gap-6 mb-6">
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-sm font-semibold text-gray-700 mb-4">Weekly Check-ins</h2>
          <BarChart
            data={data.weeklyStats.map((s) => ({
              label: s.dayLabel,
              value: s.checkIns,
              color: "#3B82F6",
            }))}
            height={180}
          />
        </div>
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-sm font-semibold text-gray-700 mb-4">On-Time vs Late</h2>
          <BarChart
            data={data.weeklyStats.flatMap((s) => [
              { label: "", value: s.onTime, color: "#10B981" },
              { label: s.dayLabel, value: s.late, color: "#F59E0B" },
            ])}
            height={180}
          />
        </div>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <h2 className="text-sm font-semibold text-gray-700 mb-4">Top Employees (Last 30 Days)</h2>
        {data.topEmployees.length === 0 ? (
          <p className="text-sm text-gray-500">No data available yet</p>
        ) : (
          <div className="space-y-3">
            {data.topEmployees.map((emp, i) => (
              <Link
                key={emp.employeeId}
                href={`/dashboard/employee/${emp.employeeId}`}
                className="flex items-center gap-3 p-3 rounded-lg hover:bg-gray-50 transition-colors"
              >
                <span className="text-sm font-medium text-gray-400 w-5">{i + 1}</span>
                {emp.photoUrl ? (
                  <img
                    src={getPhotoUrl(emp.photoUrl) || ""}
                    alt=""
                    className="w-8 h-8 rounded-full object-cover"
                  />
                ) : (
                  <div className="w-8 h-8 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 text-xs font-medium">
                    {emp.employeeName.charAt(0)}
                  </div>
                )}
                <div className="flex-1">
                  <div className="text-sm font-medium text-gray-900">{emp.employeeName}</div>
                  <div className="text-xs text-gray-500">{emp.totalDays} days</div>
                </div>
                <div className="text-right">
                  <div className="text-sm font-semibold text-green-600">{emp.onTimeRate}%</div>
                  <div className="text-xs text-gray-400">on-time</div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
