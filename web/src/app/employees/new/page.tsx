import type { Metadata } from "next";
import EmployeeForm from "@/components/EmployeeForm";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Register Employee - Attendance System",
};

export default function NewEmployeePage() {
  return (
    <div className="min-h-screen bg-gray-50 py-6 sm:py-8 px-4">
      <div className="max-w-2xl mx-auto">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6 sm:mb-8">
          <h1 className="text-2xl sm:text-3xl font-bold">Register Employee</h1>
          <Link
            href="/employees"
            className="text-blue-600 hover:text-blue-700 font-medium text-sm sm:text-base"
          >
            &larr; Back to list
          </Link>
        </div>
        <div className="bg-white rounded-xl shadow-sm p-4 sm:p-6">
          <EmployeeForm />
        </div>
      </div>
    </div>
  );
}
