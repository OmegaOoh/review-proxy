<script setup lang="ts">
import { useAuth } from "../composables/useAuth";

const { user, isAuthenticated } = useAuth();
</script>

<template>
    <div v-if="isAuthenticated && user" class="flex items-center">
        <div class="flex items-center space-x-3">
            <img
                v-if="user.gitHubUsername"
                :src="`https://github.com/${user.gitHubUsername}.png`"
                alt="Avatar"
                class="w-10 h-10 rounded-full border-2 border-gray-200 dark:border-gray-700"
            />
            <div class="flex flex-col">
                <span
                    class="font-semibold text-gray-900 dark:text-white leading-tight"
                >
                    {{ user.gitHubUsername }}
                </span>
                <span class="text-xs text-gray-500 dark:text-gray-400">
                    GitHub User
                </span>
            </div>
        </div>
    </div>
</template>
