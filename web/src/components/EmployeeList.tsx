"use client";

import { useEffect, useState } from "react";
import { apiFetch, getPhotoUrl } from "@/lib/api";
import Link from "next/link";

interface Employee {
  id: string;
  name: string;
  email: string | null;
  photoUrl: string | null;
  isActive: boolean;
  createdAt: string;
}

export default function EmployeeList() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchEmployees = async () => {
    try {
      const data = await apiFetch<Employee[]>("/api/employees");
      setEmployees(data);
    } catch (err) {
      console.error("Failed to fetch employees:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEmployees();
  }, []);

  const handleDeactivate = async (id: string) => {
    try {
      await apiFetch(`/api/employees/${id}`, { method: "DELETE" });
      setEmployees((prev) => prev.filter((e) => e.id !== id));
    } catch (err) {
      console.error("Failed to deactivate:", err);
    }
  };

  if (loading) {
    return <p className="text-gray-500 text-center py-8">Loading employees...</p>;
  }

  if (employees.length === 0) {
    return (
      <div className="text-center py-8">
        <p className="text-gray-500 mb-4">No employees registered yet.</p>
        <Link
          href="/employees/new"
          className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-lg"
        >
          Register First Employee
        </Link>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full border-collapse">
        <thead>
          <tr className="border-b border-gray-200">
            <th className="text-left py-3 px-4 font-medium text-gray-600">Photo</th>
            <th className="text-left py-3 px-4 font-medium text-gray-600">Name</th>
            <th className="text-left py-3 px-4 font-medium text-gray-600">Email</th>
            <th className="text-left py-3 px-4 font-medium text-gray-600">Registered</th>
            <th className="text-left py-3 px-4 font-medium text-gray-600">Actions</th>
          </tr>
        </thead>
        <tbody>
          {employees.map((emp) => {
            const photo = getPhotoUrl(emp.photoUrl);
            return (
              <tr key={emp.id} className="border-b border-gray-100 hover:bg-gray-50">
                <td className="py-3 px-4">
                  {photo ? (
                    <img
                      src={photo}
                      alt={emp.name}
                      className="w-10 h-10 rounded-full object-cover"
                    />
                  ) : (
                    <div className="w-10 h-10 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 text-sm">
                      {emp.name.charAt(0)}
                    </div>
                  )}
                </td>
                <td className="py-3 px-4 font-medium">{emp.name}</td>
                <td className="py-3 px-4 text-gray-600">{emp.email || "-"}</td>
                <td className="py-3 px-4 text-gray-600">
                  {new Date(emp.createdAt).toLocaleDateString()}
                </td>
                <td className="py-3 px-4">
                  <button
                    onClick={() => handleDeactivate(emp.id)}
                    className="text-red-600 hover:text-red-700 text-sm font-medium"
                  >
                    Deactivate
                  </button>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
