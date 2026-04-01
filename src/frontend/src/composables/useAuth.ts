import { ref, computed } from 'vue';

// Shared state for the application
const user = ref<any>(null);
const loading = ref(true);

export function useAuth() {
    const fetchUser = async () => {
        loading.value = true;
        try {
            const token = localStorage.getItem("token");
            const headers: HeadersInit = {};
            if (token) {
                headers["Authorization"] = `Bearer ${token}`;
            }

            const response = await fetch("/api/sync/me", { headers });
            if (response.ok) {
                const data = await response.json();
                user.value = data.user;
                if (data.token) {
                    localStorage.setItem("token", data.token);
                }
            } else {
                user.value = null;
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
        signOut
    };
}
