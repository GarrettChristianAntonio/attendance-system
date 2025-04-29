"use client";

import { useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import { getPhotoUrl } from "@/lib/api";

const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

interface StatusData {
  totalEmployees: number;
  checkedIn: number;
  recentCheckIns: Array<{
    name: string;
    photoPath: string | null;
    checkInAt: string;
    status: string;
  }>;
}

export default function EmbedStatusPage() {
  const [data, setData] = useState<StatusData | null>(null);
  const searchParams = useSearchParams();
  const token = searchParams.get("token");

  useEffect(() => {
    const fetchStatus = async () => {
      try {
        const headers: Record<string, string> = {};
        if (token) {
          headers["Authorization"] = `Bearer ${token}`;
        }

        const res = await fetch(`${API_BASE}/api/widget/status`, { headers });
        if (!res.ok) throw new Error(`API error: ${res.status}`);
        const d = await res.json();
        setData(d);
      } catch (err) {
        console.error("Failed to load status:", err);
      }
    };
    fetchStatus();
    const interval = setInterval(fetchStatus, 30000);
    return () => clearInterval(interval);
  }, [token]);

  if (!data) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50">
        <p className="text-gray-500">Loading...</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-lg mx-auto">
        <div className="flex gap-4 mb-6">
          <div className="flex-1 bg-blue-50 rounded-xl p-4 text-center border border-blue-100">
            <div className="text-2xl font-bold text-blue-700">{data.checkedIn}</div>
            <div className="text-xs text-blue-500">Checked In</div>
          </div>
          <div className="flex-1 bg-gray-50 rounded-xl p-4 text-center border border-gray-200">
            <div className="text-2xl font-bold text-gray-700">{data.totalEmployees}</div>
            <div className="text-xs text-gray-500">Total</div>
          </div>
        </div>

        <h2 className="text-sm font-semibold text-gray-600 mb-3">Recent Check-ins</h2>
        <div className="space-y-2">
          {data.recentCheckIns.map((r, i) => (
            <div key={i} className="flex items-center gap-3 bg-white rounded-lg p-3 border border-gray-100">
              {r.photoPath ? (
                <img
                  src={getPhotoUrl(r.photoPath) || ""}
                  alt=""
                  className="w-8 h-8 rounded-full object-cover"
                />
              ) : (
                <div className="w-8 h-8 rounded-full bg-gray-200 flex items-center justify-center text-xs font-medium text-gray-500">
                  {r.name.charAt(0)}
                </div>
              )}
              <div className="flex-1">
                <div className="text-sm font-medium text-gray-900">{r.name}</div>
                <div className="text-xs text-gray-400">
                  {new Date(r.checkInAt).toLocaleTimeString()}
                </div>
              </div>
              <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${
                r.status === "OnTime" ? "bg-green-100 text-green-700" : "bg-yellow-100 text-yellow-700"
              }`}>
                {r.status}
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
