<script setup lang="ts">
import { ref, computed } from "vue";

const props = defineProps<{
    githubRepositories: any[];
    loading: boolean;
    installationUrl?: string;
}>();

const modelValue = defineModel<string>({ required: true });

const emit = defineEmits<{
    refresh: [];
}>();

const repoSearchQuery = ref("");

const filteredInstalledRepos = computed(() =>
    props.githubRepositories.filter(
        (r) =>
            r.is_installed &&
            r.full_name
                .toLowerCase()
                .includes(repoSearchQuery.value.toLowerCase()),
    ),
);

const filteredUninstalledRepos = computed(() =>
    props.githubRepositories.filter(
        (r) =>
            !r.is_installed &&
            r.full_name
                .toLowerCase()
                .includes(repoSearchQuery.value.toLowerCase()),
    ),
);
</script>

<template>
    <div class="space-y-4">
        <div class="flex justify-between items-center mb-2">
            <label
                class="block text-xs font-bold uppercase tracking-widest text-gray-600 dark:text-gray-400"
            >
                Select GitHub Repository
            </label>
            <button
                type="button"
                @click="emit('refresh')"
                :disabled="loading"
                class="text-[10px] font-bold text-blue-600 hover:text-blue-700 flex items-center gap-1 transition-all disabled:opacity-50"
            >
                <i class="pi pi-refresh" :class="{ 'pi-spin': loading }"></i>
                Refresh List
            </button>
        </div>

        <div class="relative group">
            <i
                class="pi pi-search absolute left-4 top-4 text-gray-500 dark:text-gray-400"
            ></i>
            <input
                v-model="repoSearchQuery"
                type="text"
                :disabled="loading"
                placeholder="Search your GitHub repositories..."
                class="w-full pl-11 pr-4 py-3.5 bg-gray-100 dark:bg-gray-900 border border-gray-300 dark:border-gray-700 rounded-2xl text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all shadow-sm disabled:opacity-50"
            />
        </div>

        <div
            class="border border-gray-200 dark:border-gray-700 rounded-3xl overflow-hidden bg-gray-100/30 dark:bg-gray-900/30"
        >
            <div
                class="max-h-60 overflow-y-auto custom-scrollbar relative min-h-[100px]"
            >
                <div
                    v-if="loading"
                    class="absolute inset-0 bg-white/60 dark:bg-gray-800/60 backdrop-blur-[1px] z-10 flex flex-col items-center justify-center"
                >
                    <i
                        class="pi pi-spin pi-spinner text-2xl text-blue-600 mb-2"
                    ></i>
                    <span
                        class="text-[10px] font-bold text-gray-600 dark:text-gray-400 uppercase tracking-widest"
                        >Updating Repositories...</span
                    >
                </div>

                <div v-if="filteredInstalledRepos.length > 0">
                    <div
                        class="px-4 py-2 bg-gray-200/50 dark:bg-gray-800/50 text-[10px] font-bold uppercase tracking-widest text-gray-600 dark:text-gray-400 border-b border-gray-200 dark:border-gray-700"
                    >
                        Installed & Ready
                    </div>
                    <div
                        v-for="repo in filteredInstalledRepos"
                        :key="repo.id"
                        @click="modelValue = repo.full_name"
                        class="px-4 py-3 flex items-center justify-between cursor-pointer hover:bg-blue-50 dark:hover:bg-blue-900/20 transition-colors border-b border-gray-200/50 dark:border-gray-700/50 group"
                        :class="{
                            'bg-blue-50 dark:bg-blue-900/30 border-l-4 border-l-blue-500':
                                modelValue === repo.full_name,
                        }"
                    >
                        <div class="flex items-center gap-3 overflow-hidden">
                            <i
                                class="pi pi-github text-lg text-gray-500 dark:text-gray-400 group-hover:text-blue-500 transition-colors"
                            ></i>
                            <div class="flex flex-col min-w-0">
                                <span
                                    class="text-sm font-bold text-gray-900 dark:text-white truncate"
                                    >{{ repo.full_name }}</span
                                >
                                <span
                                    class="text-[10px] text-gray-600 dark:text-gray-400 truncate"
                                    >{{
                                        repo.description || "No description"
                                    }}</span
                                >
                            </div>
                        </div>
                        <i
                            v-if="modelValue === repo.full_name"
                            class="pi pi-check-circle text-blue-500"
                        ></i>
                        <div
                            v-else
                            class="w-5 h-5 rounded-full border-2 border-gray-300 dark:border-gray-700 group-hover:border-blue-200 transition-all"
                        ></div>
                    </div>
                </div>

                <div v-if="filteredUninstalledRepos.length > 0">
                    <div
                        class="px-4 py-2 bg-amber-50/50 dark:bg-amber-900/10 text-[10px] font-bold uppercase tracking-widest text-amber-600 dark:text-amber-500 border-b border-gray-200 dark:border-gray-700"
                    >
                        App Not Installed
                    </div>
                    <div
                        v-for="repo in filteredUninstalledRepos"
                        :key="repo.id"
                        class="px-4 py-3 flex items-center justify-between bg-white/50 dark:bg-gray-800/20 opacity-80 hover:opacity-100 transition-opacity border-b border-gray-200/50 dark:border-gray-700/50"
                    >
                        <div class="flex items-center gap-3 overflow-hidden">
                            <i
                                class="pi pi-github text-lg text-amber-400/50"
                            ></i>
                            <div class="flex flex-col min-w-0">
                                <span
                                    class="text-sm font-medium text-gray-700 dark:text-gray-400 truncate"
                                    >{{ repo.full_name }}</span
                                >
                                <span
                                    class="text-[10px] text-amber-600/70 dark:text-amber-500/70"
                                    >Requires App Installation</span
                                >
                            </div>
                        </div>
                        <a
                            v-if="installationUrl"
                            :href="installationUrl"
                            target="_blank"
                            class="px-3 py-1 bg-amber-100 dark:bg-amber-900/40 text-amber-700 dark:text-amber-400 text-[10px] font-bold rounded-lg hover:bg-amber-200 transition-all flex items-center gap-1.5"
                        >
                            <i class="pi pi-external-link"></i> Install
                        </a>
                    </div>
                </div>

                <div
                    v-if="
                        !loading &&
                        filteredInstalledRepos.length === 0 &&
                        filteredUninstalledRepos.length === 0
                    "
                    class="p-8 text-center"
                >
                    <i
                        class="pi pi-search text-2xl text-gray-400 dark:text-gray-500 mb-2"
                    ></i>
                    <p class="text-sm text-gray-600 dark:text-gray-400">
                        No repositories found matching your search.
                    </p>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
    width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
    background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
    background: #e2e8f0;
    border-radius: 10px;
}
.dark .custom-scrollbar::-webkit-scrollbar-thumb {
    background: #334155;
}
</style>
