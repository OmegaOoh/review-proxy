import { api, API_BASE_URL } from "./client";

export interface SyncData {
  token: string;
  github_token?: string;
}

export const AuthService = {
  async syncSession(): Promise<SyncData | null> {
    try {
      const syncData = await api.get<SyncData>("/api/sync/me");
      if (syncData.token) {
        localStorage.setItem("token", syncData.token);
        if (syncData.github_token) {
          localStorage.setItem("github_token", syncData.github_token);
        }
        return syncData;
      }
    } catch (err) {
      console.warn("Authentication session sync failed", err);
    }
    return null;
  },

  signIn() {
    window.location.href = `${API_BASE_URL}/api/sync/signin?returnUrl=${encodeURIComponent(window.location.href)}`;
  },

  signOut() {
    localStorage.removeItem("token");
    localStorage.removeItem("github_token");
    window.location.href = `${API_BASE_URL}/api/sync/signout?returnUrl=${encodeURIComponent(window.location.href)}`;
  },
};
