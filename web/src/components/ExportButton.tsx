"use client";

import { useState } from "react";

const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

function getApiKey(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("apiKey") || process.env.NEXT_PUBLIC_API_KEY || null;
}

interface ExportButtonProps {
  from?: string;
  to?: string;
}

export default function ExportButton({ from, to }: ExportButtonProps) {
  const [open, setOpen] = useState(false);

  const handleExport = (format: string) => {
    const apiKey = getApiKey();
    const params = new URLSearchParams();
    if (from) params.set("from", from);
    if (to) params.set("to", to);
    params.set("format", format);

    const url = `${API_BASE}/api/attendance/export?${params}`;

    // Use fetch to include API key header, then trigger download
    fetch(url, {
      headers: apiKey ? { "X-Api-Key": apiKey } : {},
    })
      .then((res) => res.blob())
      .then((blob) => {
        const a = document.createElement("a");
        a.href = URL.createObjectURL(blob);
        a.download = `attendance-${from || "report"}-${to || "today"}.${format}`;
        a.click();
        URL.revokeObjectURL(a.href);
      })
      .catch((err) => console.error("Export failed:", err));

    setOpen(false);
  };

  return (
    <div className="relative">
      <button
        onClick={() => setOpen(!open)}
        className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
      >
        Export
      </button>
      {open && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setOpen(false)} />
          <div className="absolute right-0 top-full mt-1 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-50 min-w-[120px]">
            <button
              onClick={() => handleExport("csv")}
              className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
            >
              CSV
            </button>
            <button
              onClick={() => handleExport("pdf")}
              className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
            >
              PDF
            </button>
          </div>
        </>
      )}
    </div>
  );
}
