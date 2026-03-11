<script setup lang="ts">
import { ref, onMounted } from "vue";

const appTitle = ref("Review Proxy");
const user = ref<any>(null);

const fetchUser = async () => {
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = {};
        if (token) {
            headers["Authorization"] = `Bearer ${token}`;
        }

        const response = await fetch("/api/identities/me", { headers });
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
    }
};

const signIn = () => {
    window.location.href = `/api/identities/signin?returnUrl=${encodeURIComponent(window.location.href)}`;
};

const signOut = () => {
    localStorage.removeItem("token");
    window.location.href = `/api/identities/signout?returnUrl=${encodeURIComponent(window.location.href)}`;
};

onMounted(() => {
    fetchUser();
});
</script>

<template>
    <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
        <div class="container mx-auto px-4 py-8">
            <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-6">
                {{ appTitle }}
            </h1>
            <p class="text-gray-700 dark:text-gray-300 mb-4">
                Welcome to the Review Proxy application! This is a placeholder
                for the main content of the app.
            </p>

            <div
                v-if="user"
                class="mt-6 p-6 bg-white dark:bg-gray-800 rounded-lg shadow-md max-w-sm"
            >
                <div class="flex items-center space-x-4 mb-4">
                    <img
                        v-if="user.gitHubUsername"
                        :src="`https://github.com/${user.gitHubUsername}.png`"
                        alt="Avatar"
                        class="w-16 h-16 rounded-full border-2 border-gray-200 dark:border-gray-700"
                    />
                    <div>
                        <h2
                            class="text-xl font-semibold text-gray-900 dark:text-white"
                        >
                            {{ user.gitHubUsername }}
                        </h2>
                        <p class="text-sm text-gray-500 dark:text-gray-400">
                            GitHub User
                        </p>
                    </div>
                </div>
                <button
                    @click="signOut"
                    class="w-full px-4 py-2 bg-red-600 hover:bg-red-700 text-white font-medium rounded-lg transition-colors"
                >
                    Sign Out
                </button>
            </div>

            <div v-else class="mt-6">
                <button
                    @click="signIn"
                    class="px-6 py-3 bg-gray-900 hover:bg-gray-800 text-white font-medium rounded-lg flex items-center space-x-2 transition-colors dark:bg-white dark:text-gray-900 dark:hover:bg-gray-100"
                >
                    <i class="pi pi-github text-xl"></i>
                    <span>Sign In with GitHub</span>
                </button>
            </div>
        </div>
    </div>
</template>
