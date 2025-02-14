import EmployeeList from "@/components/EmployeeList";
import Link from "next/link";

export default function EmployeesPage() {
  return (
    <div className="min-h-screen bg-gray-50 py-8 px-4">
      <div className="max-w-4xl mx-auto">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold">Employees</h1>
          <div className="flex gap-4">
            <Link
              href="/"
              className="text-gray-600 hover:text-gray-800 font-medium"
            >
              &larr; Camera
            </Link>
            <Link
              href="/employees/new"
              className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium"
            >
              + New Employee
            </Link>
          </div>
        </div>
        <div className="bg-white rounded-xl shadow-sm p-6">
          <EmployeeList />
        </div>
      </div>
    </div>
  );
}
