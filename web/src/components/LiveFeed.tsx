"use client";

import { useEffect, useState, useRef } from "react";
import { getPhotoUrl } from "@/lib/api";
import StatusBadge from "./StatusBadge";

interface LiveEvent {
  employeeName: string;
  photoUrl: string | null;
  status: string;
  checkInAt: string;
}

const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

function getApiKey(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("apiKey") || process.env.NEXT_PUBLIC_API_KEY || null;
}

export default function LiveFeed() {
  const [events, setEvents] = useState<LiveEvent[]>([]);
  const [connected, setConnected] = useState(false);
  const eventSourceRef = useRef<EventSource | null>(null);

  useEffect(() => {
    const apiKey = getApiKey();
    const url = `${API_BASE}/api/attendance/live${apiKey ? `?apiKey=${apiKey}` : ""}`;

    const es = new EventSource(url);
    eventSourceRef.current = es;

    es.onopen = () => setConnected(true);
    es.onerror = () => setConnected(false);

    es.onmessage = (e) => {
      try {
        const data = JSON.parse(e.data) as LiveEvent;
        setEvents((prev) => [data, ...prev].slice(0, 10));
      } catch {
        // ignore parse errors
      }
    };

    return () => {
      es.close();
      eventSourceRef.current = null;
    };
  }, []);

  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5">
      <div className="flex items-center gap-2 mb-4">
        <div className={`w-2 h-2 rounded-full ${connected ? "bg-green-500 animate-pulse" : "bg-red-500"}`} />
        <h2 className="text-sm font-semibold text-gray-700">Live Feed</h2>
      </div>

      {events.length === 0 ? (
        <p className="text-sm text-gray-400">Waiting for check-ins...</p>
      ) : (
        <div className="space-y-2">
          {events.map((evt, i) => (
            <div
              key={`${evt.checkInAt}-${i}`}
              className="flex items-center gap-3 p-2 rounded-lg bg-gray-50 animate-in"
              style={{ animationDelay: `${i * 50}ms` }}
            >
              {evt.photoUrl ? (
                <img
                  src={getPhotoUrl(evt.photoUrl) || ""}
                  alt=""
                  className="w-7 h-7 rounded-full object-cover"
                />
              ) : (
                <div className="w-7 h-7 rounded-full bg-gray-200 flex items-center justify-center text-xs font-medium text-gray-500">
                  {evt.employeeName.charAt(0)}
                </div>
              )}
              <div className="flex-1 min-w-0">
                <div className="text-sm font-medium text-gray-900 truncate">{evt.employeeName}</div>
                <div className="text-xs text-gray-400">
                  {new Date(evt.checkInAt).toLocaleTimeString()}
                </div>
              </div>
              <StatusBadge status={evt.status} />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
