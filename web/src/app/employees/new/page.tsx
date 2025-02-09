import EmployeeForm from "@/components/EmployeeForm";
import Link from "next/link";

export default function NewEmployeePage() {
  return (
    <div className="min-h-screen bg-gray-50 py-8 px-4">
      <div className="max-w-2xl mx-auto">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold">Register Employee</h1>
          <Link
            href="/employees"
            className="text-blue-600 hover:text-blue-700 font-medium"
          >
            &larr; Back to list
          </Link>
        </div>
        <div className="bg-white rounded-xl shadow-sm p-6">
          <EmployeeForm />
        </div>
      </div>
    </div>
  );
}
