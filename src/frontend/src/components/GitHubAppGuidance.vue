<script setup lang="ts">
defineProps<{
    installationUrl?: string;
}>();
</script>

<template>
    <div class="mb-8 p-6 bg-blue-50 dark:bg-blue-900/20 border border-blue-100 dark:border-blue-800 rounded-3xl">
        <h4 class="text-blue-900 dark:text-blue-300 font-bold mb-2 flex items-center gap-2">
            <i class="pi pi-info-circle"></i>
            GitHub App Required
        </h4>
        <p class="text-sm text-blue-700 dark:text-blue-400 leading-relaxed mb-4">
            To allow ReviewProxy to manage issues, you must first install our GitHub App on your target repositories.
        </p>
        <a
            v-if="installationUrl"
            :href="installationUrl"
            target="_blank"
            class="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold rounded-xl transition-all shadow-md shadow-blue-200 dark:shadow-none"
        >
            <i class="pi pi-external-link"></i>
            Install GitHub App
        </a>
    </div>
</template>
