<template>
    <div class="mt-4">
        <div
            v-if="loading && !repo"
            class="flex flex-col items-center justify-center py-20"
        >
            <i class="pi pi-spin pi-spinner text-4xl text-blue-600 mb-4"></i>
            <span class="text-gray-500">Loading repository details...</span>
        </div>

        <div
            v-else-if="error"
            class="p-4 bg-red-50 text-red-700 rounded-xl border border-red-100 flex items-center gap-3"
        >
            <i class="pi pi-exclamation-circle text-xl"></i>
            {{ error }}
        </div>

        <div v-else-if="repo">
            <div class="mb-8">
                <div class="flex justify-between items-start mb-6">
                    <button
                        @click="router.back()"
                        class="px-4 py-2 text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-gray-200 transition-colors flex items-center gap-2"
                    >
                        <i class="pi pi-arrow-left"></i>
                        Back
                    </button>

                    <button
                        v-if="repo.ownerId === props.user.id"
                        @click="handleOptOut"
                        class="px-4 py-2 bg-red-50 text-red-600 dark:bg-red-900/20 dark:text-red-400 rounded-xl hover:bg-red-100 transition-colors flex items-center gap-2 text-sm font-bold"
                    >
                        <i class="pi pi-sign-out"></i>
                        Opt-out
                    </button>
                </div>

                <div
                    class="flex flex-col md:flex-row md:items-end justify-between gap-4"
                >
                    <div class="flex-grow">
                        <div class="flex items-center gap-3 mb-2">
                            <h1
                                class="text-4xl font-extrabold text-gray-900 dark:text-white tracking-tight"
                            >
                                {{ repo.gitHubRepoId }}
                            </h1>
                            <a
                                :href="`https://github.com/${repo.gitHubRepoId}`"
                                target="_blank"
                                rel="noopener noreferrer"
                                class="text-gray-400 hover:text-blue-500 transition-colors"
                                title="View on GitHub"
                            >
                                <i class="pi pi-github text-2xl"></i>
                            </a>
                        </div>

                        <div
                            v-if="isEditingDescription"
                            class="flex flex-col sm:flex-row gap-2 mt-4 max-w-2xl"
                        >
                            <input
                                v-model="editDescriptionText"
                                class="flex-grow px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-xl shadow-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white outline-none"
                                placeholder="Repository description..."
                                @keyup.enter="saveDescription"
                                @keyup.esc="cancelEditDescription"
                                autofocus
                            />
                            <div class="flex gap-2">
                                <button
                                    @click="saveDescription"
                                    class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-xl transition-colors"
                                >
                                    Save
                                </button>
                                <button
                                    @click="cancelEditDescription"
                                    class="px-4 py-2 bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 font-semibold rounded-xl transition-colors"
                                >
                                    Cancel
                                </button>
                            </div>
                        </div>
                        <div v-else class="flex items-center group mt-2">
                            <p class="text-xl text-gray-600 dark:text-gray-400">
                                {{
                                    repo.description ||
                                    "No description provided."
                                }}
                            </p>
                            <button
                                v-if="props.user.id === repo.ownerId"
                                @click="startEditDescription"
                                class="ml-3 p-1.5 opacity-0 group-hover:opacity-100 text-gray-400 hover:text-blue-500 transition-all"
                                title="Edit Description"
                            >
                                <i class="pi pi-pencil"></i>
                            </button>
                        </div>
                    </div>

                    <div
                        class="flex items-center gap-4 text-sm font-medium text-gray-500 bg-white dark:bg-gray-800 p-3 rounded-2xl border border-gray-100 dark:border-gray-700 shadow-sm"
                    >
                        <div
                            class="flex items-center gap-2 px-3 border-r border-gray-100 dark:border-gray-700"
                        >
                            <img
                                v-if="repo.owner"
                                :src="
                                    repo.owner.gitHubAvatarUrl ||
                                    `https://github.com/${repo.owner.gitHubUsername}.png`
                                "
                                class="w-5 h-5 rounded-full border border-gray-100 dark:border-gray-700 shadow-sm"
                            />
                            <i v-else class="pi pi-user text-blue-500"></i>
                            <span>{{
                                repo.owner?.gitHubUsername ||
                                repo.ownerId.substring(0, 8)
                            }}</span>
                        </div>
                        <div class="flex items-center gap-2 px-3">
                            <i class="pi pi-calendar text-green-500"></i>
                            <span>{{ formatDate(repo.createdAt) }}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div class="mb-8 border-b border-gray-200 dark:border-gray-700">
                <nav class="flex space-x-8" aria-label="Tabs">
                    <button
                        v-for="tab in ['issues', 'auditors']"
                        :key="tab"
                        @click="activeTab = tab"
                        :class="[
                            activeTab === tab
                                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300',
                            'whitespace-nowrap py-4 px-1 border-b-2 font-bold text-sm capitalize transition-all duration-200',
                        ]"
                    >
                        {{ tab }}
                        <span
                            v-if="tab === 'issues' && issues.length"
                            class="ml-2 px-2 py-0.5 text-[10px] bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-400 rounded-full"
                        >
                            {{ issues.length }}
                        </span>
                    </button>
                </nav>
            </div>

            <transition name="fade" mode="out-in">
                <div v-if="activeTab === 'issues'" key="issues">
                    <RepositoryIssues :repo="repo" :user="props.user" />
                </div>
                <div v-else-if="activeTab === 'auditors'" key="auditors">
                    <RepositoryAuditors :repo="repo" :user="props.user" />
                </div>
            </transition>
        </div>

        <div v-else class="py-20 text-center">
            <i class="pi pi-search text-4xl text-gray-300 mb-4"></i>
            <p class="text-xl text-gray-500">Repository not found.</p>
            <button
                @click="router.push('/')"
                class="mt-4 text-blue-600 hover:underline"
            >
                Return Home
            </button>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { storeToRefs } from "pinia";
import { useRepoStore } from "../stores/repositories";
import { useIssueStore } from "../stores/issues";
import { useConfirm } from "primevue/useconfirm";
import RepositoryIssues from "./RepositoryIssues.vue";
import RepositoryAuditors from "./RepositoryAuditors.vue";
import type { User } from "../types";

const props = defineProps<{
    user: User;
}>();

const route = useRoute();
const router = useRouter();
const repoStore = useRepoStore();
const issueStore = useIssueStore();
const confirm = useConfirm();
const { repositories, loading, error } = storeToRefs(repoStore);
const { issues } = storeToRefs(issueStore);

const repoId = route.params.id as string;
// ...
const activeTab = ref("issues");
const isEditingDescription = ref(false);
const editDescriptionText = ref("");

const repo = computed(() => repositories.value.find((r) => r.id === repoId));

const startEditDescription = () => {
    editDescriptionText.value = repo.value?.description || "";
    isEditingDescription.value = true;
};

const cancelEditDescription = () => {
    isEditingDescription.value = false;
};

const saveDescription = async () => {
    if (!repo.value) return;
    try {
        await repoStore.updateRepositoryDescription(
            repo.value.id,
            editDescriptionText.value,
        );
        isEditingDescription.value = false;
    } catch (err) {
        console.error("Failed to update description", err);
    }
};

const handleOptOut = async () => {
    if (!repo.value) return;
    confirm.require({
        message: `Are you sure you want to opt-out from ${repo.value.gitHubRepoId}? This will remove all Review Proxy configuration for this repository.`,
        header: "Opt-out Repository",
        icon: "pi pi-exclamation-triangle",
        rejectProps: {
            label: "Cancel",
            severity: "secondary",
            outlined: true,
        },
        acceptProps: {
            label: "Opt-out",
            severity: "danger",
        },
        accept: async () => {
            try {
                await repoStore.deleteRepository(repo.value!.id);
                router.push("/");
            } catch (err) {
                console.error("Failed to opt-out", err);
            }
        },
    });
};

const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString(undefined, {
        year: "numeric",
        month: "short",
        day: "numeric",
    });
};

onMounted(async () => {
    if (repositories.value.length === 0) {
        await repoStore.fetchAllRepositories();
    }
    await issueStore.fetchIssuesForRepository(repoId);
});
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
    transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}
</style>
