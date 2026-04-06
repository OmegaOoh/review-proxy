<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useAuth } from "./composables/useAuth";
import UserProfile from "./components/UserProfile.vue";
import Login from "./components/Login.vue";

const { user, isAuthenticated, loading, fetchUser, signOut } = useAuth();

// reactive flag reflecting whether dark mode is active (reads from document root)
const dark = ref(
    typeof document !== "undefined" &&
        document.documentElement.classList.contains("dark"),
);

const toggleDark = () => {
    dark.value = !dark.value;
    if (dark.value) {
        document.documentElement.classList.add("dark");
    } else {
        document.documentElement.classList.remove("dark");
    }
};

onMounted(() => {
    fetchUser();
});
</script>

<template>
    <div
        class="min-h-screen bg-gray-50 dark:bg-gray-900 text-gray-900 dark:text-gray-100"
    >
        <!-- Topbar -->
        <header class="bg-white dark:bg-gray-800 shadow-sm">
            <div
                class="container mx-auto px-4 py-3 flex items-center justify-between"
            >
                <div class="flex items-center space-x-3">
                    <div class="text-2xl font-semibold">Review Proxy</div>
                    <span class="text-sm text-gray-500 dark:text-gray-400"
                        >Manage repo reviews effortlessly</span
                    >
                </div>

                <div class="flex items-center space-x-3">
                    <button
                        @click="toggleDark"
                        class="p-2 rounded hover:bg-gray-100 dark:hover:bg-gray-700"
                        :aria-pressed="dark"
                        :title="dark ? 'Switch to light' : 'Switch to dark'"
                    >
                        <i v-if="!dark" class="pi pi-sun text-xl"></i>
                        <i v-else class="pi pi-moon text-xl"></i>
                    </button>

                    <template v-if="isAuthenticated && user">
                        <div class="flex items-center space-x-3">
                            <UserProfile />
                            <button
                                @click="signOut"
                                class="px-3 py-1 bg-red-600 hover:bg-red-700 text-white rounded flex items-center"
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
            <div v-if="loading" class="mt-6 flex items-center">
                <i class="pi pi-spin pi-spinner text-2xl text-blue-600"></i>
                <span class="ml-3">Loading user data...</span>
            </div>

            <div v-else>
                <template v-if="isAuthenticated && user">
                    <router-view :user="user" />
                </template>

                <template v-else>
                    <div class="max-w-lg mx-auto">
                        <Login />
                    </div>
                </template>
            </div>
        </main>
    </div>
</template>
