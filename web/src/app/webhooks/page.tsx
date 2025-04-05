"use client";

import { useEffect, useState } from "react";
import { apiFetch, apiPost, apiDelete } from "@/lib/api";

interface Webhook {
  id: string;
  url: string;
  events: string;
  isActive: boolean;
  createdAt: string;
  lastTriggeredAt: string | null;
}

export default function WebhooksPage() {
  const [webhooks, setWebhooks] = useState<Webhook[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [url, setUrl] = useState("");
  const [events, setEvents] = useState("check_in,check_out");
  const [newSecret, setNewSecret] = useState<string | null>(null);

  const fetchWebhooks = async () => {
    try {
      const data = await apiFetch<Webhook[]>("/api/webhooks");
      setWebhooks(data);
    } catch (err) {
      console.error("Failed to load webhooks:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchWebhooks();
  }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const result = await apiPost<{ secret: string }>("/api/webhooks", { url, events });
      setNewSecret(result.secret);
      setShowForm(false);
      setUrl("");
      fetchWebhooks();
    } catch (err) {
      console.error("Failed to create webhook:", err);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this webhook?")) return;
    try {
      await apiDelete(`/api/webhooks/${id}`);
      fetchWebhooks();
    } catch (err) {
      console.error("Failed to delete webhook:", err);
    }
  };

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 py-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Webhooks</h1>
          <p className="text-sm text-gray-500 mt-1">Receive notifications when events occur</p>
        </div>
        <button
          onClick={() => setShowForm(true)}
          className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors"
        >
          + New Webhook
        </button>
      </div>

      {newSecret && (
        <div className="mb-6 bg-yellow-50 border border-yellow-200 rounded-xl p-4">
          <p className="text-sm font-medium text-yellow-800">Webhook secret (shown only once):</p>
          <code className="text-sm font-mono bg-yellow-100 px-2 py-1 rounded mt-1 block break-all">
            {newSecret}
          </code>
          <button
            onClick={() => setNewSecret(null)}
            className="mt-2 text-xs text-yellow-700 hover:text-yellow-800"
          >
            Dismiss
          </button>
        </div>
      )}

      {loading ? (
        <div className="text-center py-12 text-gray-500">Loading webhooks...</div>
      ) : webhooks.length === 0 ? (
        <div className="text-center py-12 text-gray-500">
          <p className="text-lg">No webhooks configured</p>
          <p className="text-sm mt-1">Create a webhook to receive event notifications</p>
        </div>
      ) : (
        <div className="space-y-3">
          {webhooks.map((wh) => (
            <div key={wh.id} className="bg-white rounded-xl border border-gray-200 p-4 flex items-center justify-between">
              <div>
                <div className="font-medium text-gray-900 text-sm break-all">{wh.url}</div>
                <div className="text-xs text-gray-500 mt-1">
                  Events: {wh.events} |
                  {wh.lastTriggeredAt
                    ? ` Last triggered: ${new Date(wh.lastTriggeredAt).toLocaleString()}`
                    : " Never triggered"}
                </div>
              </div>
              <button
                onClick={() => handleDelete(wh.id)}
                className="px-3 py-1.5 text-sm text-red-600 hover:text-red-700 hover:bg-red-50 rounded-lg transition-colors"
              >
                Delete
              </button>
            </div>
          ))}
        </div>
      )}

      {showForm && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl max-w-md w-full p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">New Webhook</h2>
            <form onSubmit={handleCreate} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">URL</label>
                <input
                  type="url"
                  value={url}
                  onChange={(e) => setUrl(e.target.value)}
                  placeholder="https://example.com/webhook"
                  required
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Events</label>
                <input
                  type="text"
                  value={events}
                  onChange={(e) => setEvents(e.target.value)}
                  placeholder="check_in,check_out"
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm"
                />
                <p className="text-xs text-gray-400 mt-1">Comma-separated: check_in, check_out, absence</p>
              </div>
              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setShowForm(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 rounded-lg"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg"
                >
                  Create
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
