/**
 * Standardized API client using fetch.
 * Centralizes token management, headers, and error handling.
 */

// Use an absolute URL for the API Gateway
const API_BASE_URL = import.meta.env.VITE_API_URL || "http://localhost:8000";

async function request<T>(
  endpoint: string,
  options: RequestInit = {},
): Promise<T> {
  const token = localStorage.getItem("token");
  const headers: HeadersInit = {
    "Content-Type": "application/json",
    Accept: "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers,
  };

  // Ensure endpoint starts with a slash if it's a relative path
  const url = endpoint.startsWith("http")
    ? endpoint
    : `${API_BASE_URL}${endpoint.startsWith("/") ? "" : "/"}${endpoint}`;

  const response = await fetch(url, { ...options, headers });

  if (!response.ok) {
    if (response.status === 401) {
      // Auto-cleanup on unauthorized
      localStorage.removeItem("token");
    }
    let errorMessage = `API error: ${response.statusText}`;
    try {
      const errorBody = await response.json();
      errorMessage = errorBody.message || errorMessage;
    } catch {
      // No JSON body
    }
    throw new Error(errorMessage);
  }

  if (response.status === 204) {
    return {} as T;
  }

  return response.json();
}

export const api = {
  get: <T>(endpoint: string, options?: RequestInit) =>
    request<T>(endpoint, { ...options, method: "GET" }),

  post: <T>(endpoint: string, body?: any, options?: RequestInit) =>
    request<T>(endpoint, {
      ...options,
      method: "POST",
      body: body ? JSON.stringify(body) : undefined,
    }),

  put: <T>(endpoint: string, body?: any, options?: RequestInit) =>
    request<T>(endpoint, {
      ...options,
      method: "PUT",
      body: body ? JSON.stringify(body) : undefined,
    }),

  patch: <T>(endpoint: string, body?: any, options?: RequestInit) =>
    request<T>(endpoint, {
      ...options,
      method: "PATCH",
      body: body ? JSON.stringify(body) : undefined,
    }),

  delete: <T>(endpoint: string, body?: any, options?: RequestInit) =>
    request<T>(endpoint, {
      ...options,
      method: "DELETE",
      body: body ? JSON.stringify(body) : undefined,
    }),
};
