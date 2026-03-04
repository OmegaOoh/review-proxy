<script setup lang="ts">
import { ref } from "vue";

const appTitle = ref("Review Proxy");
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
        </div>
    </div>
</template>
