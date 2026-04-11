<script setup lang="ts">
import { useAuthStore } from "../stores/auth";

const authStore = useAuthStore();
</script>

<template>
    <div
        class="mt-20 flex flex-col items-center justify-center p-12 bg-white dark:bg-gray-800 rounded-[2.5rem] shadow-2xl shadow-blue-100/50 dark:shadow-none border border-gray-100 dark:border-gray-700 max-w-md mx-auto text-center"
    >
        <div
            class="w-24 h-24 bg-blue-50 dark:bg-blue-900/20 rounded-full flex items-center justify-center mb-8"
        >
            <i class="pi pi-shield text-5xl text-blue-600"></i>
        </div>

        <h2
            class="text-3xl font-black text-gray-900 dark:text-white mb-4 tracking-tight"
        >
            Welcome to Review Proxy
        </h2>

        <p class="text-gray-500 dark:text-gray-400 mb-10 leading-relaxed">
            A specialized proxy for managing and auditing repository issues.
            Please sign in with GitHub to continue.
        </p>

        <button
            @click="authStore.signIn"
            class="group px-8 py-4 bg-gray-900 hover:bg-black text-white font-bold rounded-2xl flex items-center space-x-4 transition-all dark:bg-white dark:text-gray-900 dark:hover:bg-gray-100 w-full justify-center shadow-lg hover:shadow-xl hover:-translate-y-0.5"
        >
            <i
                class="pi pi-github text-2xl group-hover:scale-110 transition-transform"
            ></i>
            <span>Continue with GitHub</span>
        </button>

        <p class="mt-8 text-xs text-gray-400 font-medium">
            By signing in, you agree to our Terms of Service.
        </p>
    </div>
</template>
