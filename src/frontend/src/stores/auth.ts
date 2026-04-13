import { defineStore } from "pinia";
import { ref, computed } from "vue";
import type { User } from "../types";
import { AuthService } from "../api/auth";
import { IdentityService } from "../api/identities";

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

      if (!token) {
        const syncData = await AuthService.syncSession();
        token = syncData?.token || null;
      }

      if (!token) {
        user.value = null;
        return;
      }

      try {
        user.value = await IdentityService.getMe();
      } catch (err: any) {
        // Potential auto-recovery for expired or stale tokens
        if (err.message.includes("401") || err.message.includes("Not Found")) {
          localStorage.removeItem("token");
          localStorage.removeItem("github_token");
          const syncData = await AuthService.syncSession();
          if (syncData?.token) {
            user.value = await IdentityService.getMe();
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
    AuthService.signIn();
  }

  function signOut() {
    AuthService.signOut();
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
