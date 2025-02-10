"use client";

import dynamic from "next/dynamic";
import Link from "next/link";

const CameraFeed = dynamic(() => import("@/components/CameraFeed"), {
  ssr: false,
  loading: () => (
    <div className="flex items-center justify-center h-full bg-gray-900 text-white">
      <p>Loading camera...</p>
    </div>
  ),
});

export default function Home() {
  return (
    <div className="h-screen flex flex-col bg-black">
      <nav className="bg-gray-900 text-white px-6 py-3 flex items-center justify-between">
        <h1 className="text-xl font-bold">Attendance System</h1>
        <div className="flex gap-4">
          <Link href="/employees" className="hover:text-blue-400">
            Employees
          </Link>
          <Link href="/attendance" className="hover:text-blue-400">
            Attendance
          </Link>
        </div>
      </nav>
      <div className="flex-1 relative">
        <CameraFeed />
      </div>
    </div>
  );
}
