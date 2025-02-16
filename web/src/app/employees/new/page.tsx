import type { Metadata } from "next";
import EmployeeForm from "@/components/EmployeeForm";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Register Employee - Attendance System",
};

export default function NewEmployeePage() {
  return (
    <div className="py-6 sm:py-8 px-4">
      <div className="max-w-2xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Register Employee</h1>
            <p className="text-sm text-gray-500 mt-1">Capture a photo for facial recognition</p>
          </div>
          <Link
            href="/employees"
            className="text-gray-600 hover:text-gray-800 font-medium text-sm transition-colors"
          >
            &larr; Back
          </Link>
        </div>
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <EmployeeForm />
        </div>
      </div>
    </div>
  );
}
