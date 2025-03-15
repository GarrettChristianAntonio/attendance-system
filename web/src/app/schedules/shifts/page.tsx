"use client";

import { useEffect, useState } from "react";
import { apiFetch, apiPost, apiPut, apiDelete } from "@/lib/api";
import ShiftForm from "@/components/ShiftForm";

interface Shift {
  id: string;
  name: string;
  startTime: string;
  endTime: string;
  gracePeriodMinutes: number;
  color: string;
  isActive: boolean;
  createdAt: string;
}

export default function ShiftsPage() {
  const [shifts, setShifts] = useState<Shift[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingShift, setEditingShift] = useState<Shift | null>(null);

  const fetchShifts = async () => {
    try {
      const data = await apiFetch<Shift[]>("/api/shifts");
      setShifts(data);
    } catch (err) {
      console.error("Failed to load shifts:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchShifts();
  }, []);

  const handleCreate = async (data: { name: string; startTime: string; endTime: string; gracePeriodMinutes: number; color: string }) => {
    await apiPost("/api/shifts", data);
    setShowForm(false);
    fetchShifts();
  };

  const handleUpdate = async (data: { name: string; startTime: string; endTime: string; gracePeriodMinutes: number; color: string }) => {
    if (!editingShift) return;
    await apiPut(`/api/shifts/${editingShift.id}`, data);
    setEditingShift(null);
    fetchShifts();
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to deactivate this shift?")) return;
    await apiDelete(`/api/shifts/${id}`);
    fetchShifts();
  };

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 py-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Shifts</h1>
          <p className="text-sm text-gray-500 mt-1">Configure work shifts and grace periods</p>
        </div>
        <div className="flex items-center gap-3">
          <a
            href="/schedules"
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          >
            Back to Schedules
          </a>
          <button
            onClick={() => setShowForm(true)}
            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors"
          >
            + New Shift
          </button>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-12 text-gray-500">Loading shifts...</div>
      ) : shifts.length === 0 ? (
        <div className="text-center py-12 text-gray-500">
          <p className="text-lg">No shifts configured</p>
          <p className="text-sm mt-1">Create your first shift to get started</p>
        </div>
      ) : (
        <div className="grid gap-3">
          {shifts.map((shift) => (
            <div
              key={shift.id}
              className="bg-white rounded-xl border border-gray-200 p-4 flex items-center justify-between"
            >
              <div className="flex items-center gap-3">
                <div
                  className="w-4 h-4 rounded-full flex-shrink-0"
                  style={{ backgroundColor: shift.color }}
                />
                <div>
                  <div className="font-medium text-gray-900">{shift.name}</div>
                  <div className="text-sm text-gray-500">
                    {shift.startTime} - {shift.endTime} | Grace: {shift.gracePeriodMinutes} min
                  </div>
                </div>
              </div>
              <div className="flex items-center gap-2">
                <button
                  onClick={() => setEditingShift(shift)}
                  className="px-3 py-1.5 text-sm text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-colors"
                >
                  Edit
                </button>
                <button
                  onClick={() => handleDelete(shift.id)}
                  className="px-3 py-1.5 text-sm text-red-600 hover:text-red-700 hover:bg-red-50 rounded-lg transition-colors"
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {showForm && (
        <ShiftForm
          onSubmit={handleCreate}
          onCancel={() => setShowForm(false)}
        />
      )}

      {editingShift && (
        <ShiftForm
          initialData={{
            name: editingShift.name,
            startTime: editingShift.startTime,
            endTime: editingShift.endTime,
            gracePeriodMinutes: editingShift.gracePeriodMinutes,
            color: editingShift.color,
          }}
          onSubmit={handleUpdate}
          onCancel={() => setEditingShift(null)}
          isEditing
        />
      )}
    </div>
  );
}
