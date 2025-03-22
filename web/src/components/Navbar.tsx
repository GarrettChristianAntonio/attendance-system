"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const links = [
  { href: "/", label: "Camera" },
  { href: "/employees", label: "Employees" },
  { href: "/attendance", label: "Attendance" },
  { href: "/schedules", label: "Schedules" },
  { href: "/dashboard", label: "Dashboard" },
];

export default function Navbar() {
  const pathname = usePathname();

  return (
    <nav className="bg-white border-b border-gray-200 px-4 sm:px-6 py-3 sticky top-0 z-50">
      <div className="max-w-5xl mx-auto flex items-center justify-between">
        <Link href="/" className="text-lg font-bold text-gray-900 hover:text-blue-600 transition-colors">
          Attendance System
        </Link>
        <div className="flex gap-1">
          {links.map((link) => {
            const isActive =
              link.href === "/"
                ? pathname === "/"
                : pathname.startsWith(link.href);
            return (
              <Link
                key={link.href}
                href={link.href}
                className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                  isActive
                    ? "bg-blue-50 text-blue-700"
                    : "text-gray-600 hover:text-gray-900 hover:bg-gray-100"
                }`}
              >
                {link.label}
              </Link>
            );
          })}
        </div>
      </div>
    </nav>
  );
}
