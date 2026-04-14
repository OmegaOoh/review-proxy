<template>
    <div>
        <div
            class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8"
        >
            <div>
                <h2 class="text-3xl font-bold text-gray-900 dark:text-white">
                    Repositories
                </h2>
                <p class="text-gray-600 dark:text-gray-400">
                    Manage and monitor your repository reviews.
                </p>
            </div>
            <router-link
                to="/manage"
                class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-xl shadow-sm transition-all flex items-center gap-2"
            >
                <i class="pi pi-cog"></i>
                Manage My Repos
            </router-link>
        </div>

        <div
            v-if="loading"
            class="flex flex-col items-center justify-center py-20"
        >
            <i class="pi pi-spin pi-spinner text-4xl text-blue-600 mb-4"></i>
            <span class="text-gray-600">Loading repositories...</span>
        </div>

        <div
            v-else-if="error"
            class="p-4 bg-red-50 text-red-700 rounded-xl border border-red-100 flex items-center gap-3"
        >
            <i class="pi pi-exclamation-circle text-xl"></i>
            {{ error }}
        </div>

        <div
            v-else-if="repositories.length === 0"
            class="py-20 text-center bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700"
        >
            <div
                class="w-20 h-20 bg-gray-100 dark:bg-gray-700 rounded-full flex items-center justify-center mx-auto mb-4"
            >
                <i class="pi pi-inbox text-4xl text-gray-300"></i>
            </div>
            <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-2">
                No repositories found
            </h3>
            <p class="text-gray-600 dark:text-gray-400 mb-6">
                Start by depositing a repository from your GitHub account.
            </p>
            <router-link
                to="/manage"
                class="px-6 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-xl shadow-sm transition-all inline-flex items-center gap-2"
            >
                <i class="pi pi-plus"></i>
                Deposit Repository
            </router-link>
        </div>

        <div
            v-else
            class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
        >
            <div
                v-for="repo in repositories"
                :key="repo.id"
                class="group p-6 bg-white dark:bg-gray-800 rounded-2xl shadow-sm hover:shadow-md border border-gray-200 dark:border-gray-700 flex flex-col cursor-pointer transition-all duration-300"
                @click="goToRepo(repo)"
            >
                <div class="flex justify-between items-start mb-4">
                    <h3
                        class="text-lg font-bold text-gray-900 dark:text-white group-hover:text-blue-600 dark:group-hover:text-blue-400 truncate transition-colors"
                        :title="repo.gitHubRepoId"
                    >
                        {{ repo.gitHubRepoId }}
                    </h3>
                    <a
                        :href="`https://github.com/${repo.gitHubRepoId}`"
                        target="_blank"
                        rel="noopener noreferrer"
                        @click.stop
                        class="p-2 text-gray-500 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded-lg transition-all flex-shrink-0"
                        title="View on GitHub"
                    >
                        <i class="pi pi-github"></i>
                    </a>
                </div>
                <p
                    class="text-gray-600 dark:text-gray-400 text-sm mb-6 line-clamp-2 min-h-[2.5rem]"
                >
                    {{ repo.description || "No description provided." }}
                </p>

                <div
                    class="mt-auto pt-4 border-t border-gray-100 dark:border-gray-700/50"
                >
                    <div
                        class="flex items-center justify-between text-[11px] font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-500"
                    >
                        <div class="flex items-center gap-2">
                            <img
                                v-if="repo.owner"
                                :src="
                                    repo.owner.gitHubAvatarUrl ||
                                    `https://github.com/${repo.owner.gitHubUsername}.png`
                                "
                                class="w-5 h-5 rounded-full border border-gray-200 dark:border-gray-700 shadow-sm"
                            />
                            <i v-else class="pi pi-user text-[10px]"></i>
                            <span>{{
                                repo.owner?.gitHubUsername ||
                                repo.ownerId.substring(0, 8)
                            }}</span>
                        </div>
                        <div class="flex items-center gap-1">
                            <i class="pi pi-calendar text-[10px]"></i>
                            <span>{{ formatDate(repo.createdAt) }}</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import { storeToRefs } from "pinia";
import { useRepoStore } from "../stores/repositories";
import type { Repository } from "../types";

const router = useRouter();
const repoStore = useRepoStore();
const { repositories, loading, error } = storeToRefs(repoStore);

const goToRepo = (repo: Repository) => {
    router.push({ path: `/repository/${repo.id}` });
};

const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString(undefined, {
        month: "short",
        day: "numeric",
        year: "numeric",
    });
};

onMounted(() => {
    repoStore.fetchAllRepositories();
});
</script>
