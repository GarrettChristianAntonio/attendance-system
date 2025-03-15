"use client";

import { useEffect, useState } from "react";
import { apiFetch, apiPost, apiDelete } from "@/lib/api";
import WeeklyCalendar from "@/components/WeeklyCalendar";

interface Shift {
  id: string;
  name: string;
  startTime: string;
  endTime: string;
  color: string;
}

interface Employee {
  id: string;
  name: string;
}

interface WeeklySchedule {
  weekStart: string;
  entries: Array<{
    employeeId: string;
    employeeName: string;
    photoUrl: string | null;
    days: Record<number, Array<{
      scheduleId: string;
      shiftId: string;
      shiftName: string;
      shiftColor: string;
      startTime: string;
      endTime: string;
    }>>;
  }>;
}

export default function SchedulesPage() {
  const [schedule, setSchedule] = useState<WeeklySchedule | null>(null);
  const [shifts, setShifts] = useState<Shift[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [weekOffset, setWeekOffset] = useState(0);
  const [showAssign, setShowAssign] = useState(false);
  const [assignForm, setAssignForm] = useState({
    employeeId: "",
    shiftId: "",
    dayOfWeek: 1,
    effectiveFrom: new Date().toISOString().slice(0, 10),
  });

  const getWeekDate = () => {
    const d = new Date();
    d.setDate(d.getDate() + weekOffset * 7);
    return d.toISOString().slice(0, 10);
  };

  const fetchData = async () => {
    setLoading(true);
    try {
      const [sched, sh, emp] = await Promise.all([
        apiFetch<WeeklySchedule>(`/api/schedules?week=${getWeekDate()}`),
        apiFetch<Shift[]>("/api/shifts"),
        apiFetch<Employee[]>("/api/employees"),
      ]);
      setSchedule(sched);
      setShifts(sh);
      setEmployees(emp);
    } catch (err) {
      console.error("Failed to load schedules:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [weekOffset]);

  const handleAssign = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await apiPost("/api/schedules", assignForm);
      setShowAssign(false);
      fetchData();
    } catch (err) {
      console.error("Failed to assign schedule:", err);
    }
  };

  const handleDelete = async (scheduleId: string) => {
    if (!confirm("Remove this schedule assignment?")) return;
    try {
      await apiDelete(`/api/schedules/${scheduleId}`);
      fetchData();
    } catch (err) {
      console.error("Failed to delete schedule:", err);
    }
  };

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 py-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Schedules</h1>
          <p className="text-sm text-gray-500 mt-1">Manage employee shift assignments</p>
        </div>
        <div className="flex items-center gap-3">
          <a
            href="/schedules/shifts"
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          >
            Manage Shifts
          </a>
          <button
            onClick={() => setShowAssign(true)}
            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors"
          >
            + Assign Shift
          </button>
        </div>
      </div>

      <div className="flex items-center justify-center gap-4 mb-6">
        <button
          onClick={() => setWeekOffset(weekOffset - 1)}
          className="p-2 hover:bg-gray-100 rounded-lg transition-colors text-gray-600"
        >
          &larr; Prev
        </button>
        <span className="text-sm font-medium text-gray-700">
          Week of {schedule?.weekStart || getWeekDate()}
        </span>
        <button
          onClick={() => setWeekOffset(weekOffset + 1)}
          className="p-2 hover:bg-gray-100 rounded-lg transition-colors text-gray-600"
        >
          Next &rarr;
        </button>
        {weekOffset !== 0 && (
          <button
            onClick={() => setWeekOffset(0)}
            className="text-sm text-blue-600 hover:text-blue-700"
          >
            Today
          </button>
        )}
      </div>

      {loading ? (
        <div className="text-center py-12 text-gray-500">Loading schedules...</div>
      ) : schedule ? (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <WeeklyCalendar
            weekStart={schedule.weekStart}
            entries={schedule.entries}
            onDeleteSchedule={handleDelete}
          />
        </div>
      ) : null}

      {showAssign && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl max-w-md w-full p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Assign Shift</h2>
            <form onSubmit={handleAssign} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Employee</label>
                <select
                  value={assignForm.employeeId}
                  onChange={(e) => setAssignForm({ ...assignForm, employeeId: e.target.value })}
                  required
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm"
                >
                  <option value="">Select employee...</option>
                  {employees.map((emp) => (
                    <option key={emp.id} value={emp.id}>{emp.name}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Shift</label>
                <select
                  value={assignForm.shiftId}
                  onChange={(e) => setAssignForm({ ...assignForm, shiftId: e.target.value })}
                  required
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm"
                >
                  <option value="">Select shift...</option>
                  {shifts.map((s) => (
                    <option key={s.id} value={s.id}>{s.name} ({s.startTime}-{s.endTime})</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Day of Week</label>
                <select
                  value={assignForm.dayOfWeek}
                  onChange={(e) => setAssignForm({ ...assignForm, dayOfWeek: Number(e.target.value) })}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm"
                >
                  {["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"].map((d, i) => (
                    <option key={i} value={i}>{d}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Effective From</label>
                <input
                  type="date"
                  value={assignForm.effectiveFrom}
                  onChange={(e) => setAssignForm({ ...assignForm, effectiveFrom: e.target.value })}
                  required
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm"
                />
              </div>
              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setShowAssign(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 rounded-lg"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg"
                >
                  Assign
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
