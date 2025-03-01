const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

function getApiKey(): string | null {
  if (typeof window === "undefined") return process.env.NEXT_PUBLIC_API_KEY || null;
  return localStorage.getItem("apiKey") || process.env.NEXT_PUBLIC_API_KEY || null;
}

export async function apiFetch<T>(
  path: string,
  options?: RequestInit
): Promise<T> {
  const apiKey = getApiKey();
  const headers: Record<string, string> = {
    ...(options?.headers as Record<string, string>),
  };
  if (apiKey) headers["X-Api-Key"] = apiKey;

  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  });

  if (!res.ok) {
    const error = await res.text();
    throw new Error(error || `API error: ${res.status}`);
  }

  return res.json();
}

export async function apiPost<T>(
  path: string,
  body: unknown
): Promise<T> {
  return apiFetch<T>(path, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
}

export async function apiPostForm<T>(
  path: string,
  formData: FormData
): Promise<T> {
  return apiFetch<T>(path, {
    method: "POST",
    body: formData,
  });
}

export function getPhotoUrl(photoPath: string | null | undefined): string | null {
  if (!photoPath) return null;
  return `${API_BASE}${photoPath}`;
}
