import { ref, computed } from "vue";

// Shared state for the application
const user = ref<any>(null);
const loading = ref(true);

export function useAuth() {
  const fetchUser = async () => {
    loading.value = true;
    try {
      let token = localStorage.getItem("token");

      // If no token in local storage, check if we have an active GitHub session cookie
      if (!token) {
        try {
          const syncResponse = await fetch("/api/sync/me");
          if (syncResponse.ok) {
            const data = await syncResponse.json();
            if (data.token) {
              token = data.token;
              localStorage.setItem("token", data.token);
            }
          }
        } catch (err) {
          console.warn("Could not synchronize authentication session", err);
        }
      }

      if (!token) {
        user.value = null;
        return;
      }

      const headers: HeadersInit = {
        Authorization: `Bearer ${token}`,
      };

      let response = await fetch("/api/identities/me", { headers });

      // Auto-recovery: If token is rejected (e.g. expired), try to get a new one from the active session cookie
      if (!response.ok && response.status === 401) {
        localStorage.removeItem("token");
        try {
          const syncResponse = await fetch("/api/sync/me");
          if (syncResponse.ok) {
            const data = await syncResponse.json();
            if (data.token) {
              localStorage.setItem("token", data.token);
              headers.Authorization = `Bearer ${data.token}`;
              response = await fetch("/api/identities/me", { headers });
            }
          }
        } catch (err) {
          console.warn("Auto-recovery failed", err);
        }
      }

      if (response.ok) {
        const data = await response.json();
        user.value = data;
      } else {
        user.value = null;
        localStorage.removeItem("token");
      }
    } catch (error) {
      console.error("Failed to fetch user", error);
      user.value = null;
    } finally {
      loading.value = false;
    }
  };

  const signIn = () => {
    window.location.href = `/api/sync/signin?returnUrl=${encodeURIComponent(window.location.href)}`;
  };

  const signOut = () => {
    localStorage.removeItem("token");
    window.location.href = `/api/sync/signout?returnUrl=${encodeURIComponent(window.location.href)}`;
  };

  return {
    user,
    loading,
    isAuthenticated: computed(() => !!user.value),
    fetchUser,
    signIn,
    signOut,
  };
}
