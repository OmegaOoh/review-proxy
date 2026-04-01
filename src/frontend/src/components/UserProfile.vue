<script setup lang="ts">
import { useAuth } from "../composables/useAuth";

const { user, isAuthenticated, signOut } = useAuth();
</script>

<template>
    <div
        v-if="isAuthenticated && user"
        class="p-6 bg-white dark:bg-gray-800 rounded-lg shadow-md max-w-sm"
    >
        <div class="flex items-center space-x-4 mb-4">
            <img
                v-if="user.gitHubUsername"
                :src="`https://github.com/${user.gitHubUsername}.png`"
                alt="Avatar"
                class="w-16 h-16 rounded-full border-2 border-gray-200 dark:border-gray-700"
            />
            <div>
                <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
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
</template>
