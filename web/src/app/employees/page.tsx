import type { Metadata } from "next";
import EmployeeList from "@/components/EmployeeList";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Employees - Attendance System",
};

export default function EmployeesPage() {
  return (
    <div className="min-h-screen bg-gray-50 py-6 sm:py-8 px-4">
      <div className="max-w-4xl mx-auto">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6 sm:mb-8">
          <h1 className="text-2xl sm:text-3xl font-bold">Employees</h1>
          <div className="flex gap-3 sm:gap-4">
            <Link
              href="/"
              className="text-gray-600 hover:text-gray-800 font-medium text-sm sm:text-base"
            >
              &larr; Camera
            </Link>
            <Link
              href="/employees/new"
              className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium text-sm sm:text-base"
            >
              + New Employee
            </Link>
          </div>
        </div>
        <div className="bg-white rounded-xl shadow-sm p-4 sm:p-6">
          <EmployeeList />
        </div>
      </div>
    </div>
  );
}
