"use client";

import dynamic from "next/dynamic";
import Link from "next/link";
import { usePathname } from "next/navigation";

const CameraFeed = dynamic(() => import("@/components/CameraFeed"), {
  ssr: false,
  loading: () => (
    <div className="flex items-center justify-center h-full bg-gray-900 text-white">
      <div className="text-center">
        <div className="animate-spin w-10 h-10 border-4 border-blue-400 border-t-transparent rounded-full mx-auto mb-3" />
        <p>Loading camera...</p>
      </div>
    </div>
  ),
});

export default function Home() {
  const pathname = usePathname();

  return (
    <div className="h-screen flex flex-col bg-black">
      <nav className="bg-gray-900 text-white px-4 sm:px-6 py-3 flex items-center justify-between">
        <Link href="/" className="text-lg sm:text-xl font-bold hover:text-blue-400 transition-colors">
          Attendance System
        </Link>
        <div className="flex gap-2 sm:gap-4 text-sm sm:text-base">
          <Link
            href="/employees"
            className={`px-3 py-1 rounded-lg transition-colors ${
              pathname === "/employees"
                ? "bg-blue-600 text-white"
                : "hover:text-blue-400"
            }`}
          >
            Employees
          </Link>
          <Link
            href="/attendance"
            className={`px-3 py-1 rounded-lg transition-colors ${
              pathname === "/attendance"
                ? "bg-blue-600 text-white"
                : "hover:text-blue-400"
            }`}
          >
            Attendance
          </Link>
        </div>
      </nav>
      <div className="flex-1 relative overflow-hidden">
        <CameraFeed />
      </div>
    </div>
  );
}
