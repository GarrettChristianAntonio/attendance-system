import type { Metadata } from "next";
import EmployeeList from "@/components/EmployeeList";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Employees - Attendance System",
};

export default function EmployeesPage() {
  return (
    <div className="py-6 sm:py-8 px-4">
      <div className="max-w-4xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Employees</h1>
            <p className="text-sm text-gray-500 mt-1">Manage registered employees</p>
          </div>
          <Link
            href="/employees/new"
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2.5 rounded-lg font-medium text-sm transition-colors"
          >
            + New Employee
          </Link>
        </div>
        <div className="bg-white rounded-xl shadow-sm border border-gray-100">
          <EmployeeList />
        </div>
      </div>
    </div>
  );
}
