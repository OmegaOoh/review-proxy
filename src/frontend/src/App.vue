<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import { storeToRefs } from "pinia";
import { useAuthStore } from "./stores/auth";
import UserProfile from "./components/UserProfile.vue";
import Login from "./components/Login.vue";
import Toast from "primevue/toast";
import ConfirmDialog from "primevue/confirmdialog";

const authStore = useAuthStore();
const { user, isAuthenticated, loading } = storeToRefs(authStore);

// Dark mode state
const dark = ref(false);

const toggleDark = () => {
    dark.value = !dark.value;
};

// Reactively handle theme changes to DOM and LocalStorage
watch(dark, (newVal) => {
    if (newVal) {
        document.documentElement.classList.add("dark");
        localStorage.setItem("theme", "dark");
    } else {
        document.documentElement.classList.remove("dark");
        localStorage.setItem("theme", "light");
    }
});

onMounted(() => {
    // Determine initial state from DOM class (which was set by main.ts initTheme)
    const isActuallyDark = document.documentElement.classList.contains("dark");
    dark.value = isActuallyDark;

    // Sync with localStorage/preference
    const storedTheme = localStorage.getItem("theme");
    if (!storedTheme) {
        const prefersDark = window.matchMedia(
            "(prefers-color-scheme: dark)",
        ).matches;
        if (prefersDark !== isActuallyDark) {
            dark.value = prefersDark;
        }
    } else {
        const shouldBeDark = storedTheme === "dark";
        if (shouldBeDark !== isActuallyDark) {
            dark.value = shouldBeDark;
        }
    }

    authStore.fetchUser();
});
</script>

<template>
    <div
        class="min-h-screen bg-slate-100 dark:bg-gray-900 text-gray-900 dark:text-gray-100"
    >
        <Toast />
        <ConfirmDialog />

        <!-- Topbar -->
        <header
            class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 shadow-sm sticky top-0 z-40"
        >
            <div
                class="container mx-auto px-4 py-3 flex items-center justify-between"
            >
                <router-link to="/" class="flex items-center space-x-3 group">
                    <div
                        class="text-2xl font-bold bg-blue-600 text-white px-2 py-1 rounded shadow-sm group-hover:bg-blue-700 transition-colors"
                    >
                        RP
                    </div>
                    <div class="hidden sm:block">
                        <div class="text-xl font-semibold leading-tight">
                            Review Proxy
                        </div>
                        <div class="text-xs text-gray-600 dark:text-gray-400">
                            Streamlined Repo Reviews
                        </div>
                    </div>
                </router-link>

                <div class="flex items-center space-x-4">
                    <button
                        @click="toggleDark"
                        class="p-2 rounded-full hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors cursor-pointer"
                        :aria-pressed="dark"
                        :title="dark ? 'Switch to light' : 'Switch to dark'"
                    >
                        <i v-if="!dark" class="pi pi-sun text-xl"></i>
                        <i v-else class="pi pi-moon text-xl"></i>
                    </button>

                    <template v-if="isAuthenticated && user">
                        <div class="flex items-center space-x-4">
                            <UserProfile />
                            <button
                                @click="authStore.signOut"
                                class="px-3 py-1.5 border border-red-200 dark:border-red-900/30 text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg flex items-center text-sm font-medium transition-colors cursor-pointer"
                            >
                                <i class="pi pi-sign-out mr-2"></i>
                                <span>Sign out</span>
                            </button>
                        </div>
                    </template>
                </div>
            </div>
        </header>

        <!-- Main content -->
        <main class="container mx-auto px-4 py-8">
            <div
                v-if="loading && !user"
                class="mt-20 flex flex-col items-center justify-center"
            >
                <i
                    class="pi pi-spin pi-spinner text-4xl text-blue-600 mb-4"
                ></i>
                <span class="text-gray-600">Initializing session...</span>
            </div>

            <div v-else>
                <template v-if="isAuthenticated && user">
                    <router-view :user="user" />
                </template>

                <template v-else>
                    <div class="max-w-lg mx-auto mt-12">
                        <Login />
                    </div>
                </template>
            </div>
        </main>
    </div>
</template>
