<script setup lang="ts">
import { useAuth } from '../composables/useAuth';

const { signIn } = useAuth();
</script>

<template>
    <div class="mt-6 flex flex-col items-center justify-center p-8 bg-white dark:bg-gray-800 rounded-lg shadow-md max-w-md mx-auto">
        <div class="w-16 h-16 bg-gray-100 dark:bg-gray-700 rounded-full flex items-center justify-center mb-4">
            <i class="pi pi-lock text-3xl text-gray-500 dark:text-gray-400"></i>
        </div>
        <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">Authentication Required</h2>
        <p class="text-gray-600 dark:text-gray-300 mb-6 text-center">
            Please sign in with your GitHub account to access your repositories and manage your reviews.
        </p>
        <button
            @click="signIn"
            class="px-6 py-3 bg-gray-900 hover:bg-gray-800 text-white font-medium rounded-lg flex items-center space-x-3 transition-colors dark:bg-white dark:text-gray-900 dark:hover:bg-gray-100 w-full justify-center"
        >
            <i class="pi pi-github text-xl"></i>
            <span>Sign In with GitHub</span>
        </button>
    </div>
</template>
