import { defineStore } from "pinia";
import { ref, computed } from "vue";
import type { User } from "../types";
import { api } from "../api/client";

export const useAuthStore = defineStore("auth", () => {
  const user = ref<User | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const isAuthenticated = computed(() => !!user.value);

  async function fetchUser() {
    loading.value = true;
    error.value = null;
    try {
      let token = localStorage.getItem("token");

      // Sync with session cookie if no token in local storage
      if (!token) {
        try {
          const syncData = await api.get<{ token: string }>("/api/sync/me");
          if (syncData.token) {
            token = syncData.token;
            localStorage.setItem("token", syncData.token);
          }
        } catch (err) {
          console.warn("Authentication session sync failed", err);
        }
      }

      if (!token) {
        user.value = null;
        return;
      }

      try {
        user.value = await api.get<User>("/api/identities/me");
      } catch (err: any) {
        // Potential auto-recovery for expired or stale tokens
        if (err.message.includes("401") || err.message.includes("Not Found")) {
          localStorage.removeItem("token");
          const syncData = await api.get<{ token: string }>("/api/sync/me");
          if (syncData.token) {
            localStorage.setItem("token", syncData.token);
            user.value = await api.get<User>("/api/identities/me");
            return;
          }
        }
        throw err;
      }
    } catch (err: any) {
      console.error("User profile fetch failed", err);
      user.value = null;
      error.value = err.message;
    } finally {
      loading.value = false;
    }
  }

  function signIn() {
    window.location.href = `/api/sync/signin?returnUrl=${encodeURIComponent(window.location.href)}`;
  }

  function signOut() {
    localStorage.removeItem("token");
    window.location.href = `/api/sync/signout?returnUrl=${encodeURIComponent(window.location.href)}`;
  }

  return {
    user,
    loading,
    error,
    isAuthenticated,
    fetchUser,
    signIn,
    signOut,
  };
});
