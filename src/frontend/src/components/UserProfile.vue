<script setup lang="ts">
import { storeToRefs } from "pinia";
import { useAuthStore } from "../stores/auth";

const authStore = useAuthStore();
const { user, isAuthenticated } = storeToRefs(authStore);
</script>

<template>
    <div v-if="isAuthenticated && user" class="flex items-center group">
        <div
            class="flex items-center space-x-3 bg-gray-50 dark:bg-gray-700/50 p-1 pr-3 rounded-full border border-gray-100 dark:border-gray-600 transition-all hover:border-blue-200 dark:hover:border-blue-900/50"
        >
            <img
                :src="
                    user.gitHubAvatarUrl ||
                    `https://github.com/${user.gitHubUsername}.png`
                "
                alt="Avatar"
                class="w-8 h-8 rounded-full border border-white dark:border-gray-800 shadow-sm"
            />
            <div class="flex flex-col">
                <span
                    class="text-sm font-bold text-gray-900 dark:text-white leading-tight"
                >
                    {{ user.gitHubUsername }}
                </span>
                <span
                    class="text-[10px] text-gray-400 font-bold uppercase tracking-tighter"
                >
                    GitHub Profile
                </span>
            </div>
        </div>
    </div>
</template>
