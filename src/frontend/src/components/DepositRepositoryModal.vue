<script setup lang="ts">
import { ref, watch, onMounted } from "vue";
import { useRepoStore } from "../stores/repositories";
import { RepositoryService } from "../api/repositories";
import GitHubAppGuidance from "./GitHubAppGuidance.vue";
import GitHubRepoSelector from "./GitHubRepoSelector.vue";
import AuditorSelector from "./AuditorSelector.vue";
import type { User } from "../types";

const props = defineProps<{
    show: boolean;
    user: User;
}>();

const emit = defineEmits<{
    close: [];
    deposited: [repo: any];
}>();

const repoStore = useRepoStore();
const depositForm = ref({
    githubRepoId: "",
    description: "",
});

const selectedAuditors = ref<User[]>([]);
const depositing = ref(false);
const githubRepositories = ref<any[]>([]);
const loadingGithubRepos = ref(false);
const syncContext = ref<any>(null);

const fetchSyncContext = async () => {
    if (syncContext.value) return;
    try {
        syncContext.value = await RepositoryService.getSyncContext();
    } catch (err) {
        console.error("Failed to fetch sync context", err);
    }
};

const fetchGithubRepos = async (force = false) => {
    if (!force && githubRepositories.value.length > 0) return;
    loadingGithubRepos.value = true;
    try {
        githubRepositories.value =
            await RepositoryService.getSyncRepositories();
    } catch (err) {
        console.error("Failed to fetch GitHub repos", err);
    } finally {
        loadingGithubRepos.value = false;
    }
};

const handleDeposit = async () => {
    depositing.value = true;
    try {
        const githubToken = localStorage.getItem("github_token");
        const newRepo = await repoStore.depositRepository({
            githubRepoId: depositForm.value.githubRepoId,
            description: depositForm.value.description,
            gitHubToken: githubToken || undefined,
            auditors: selectedAuditors.value.map((a) => a.id),
        });
        emit("deposited", newRepo);
        depositForm.value = { githubRepoId: "", description: "" };
        selectedAuditors.value = [];
    } catch (err) {
        console.error("Deposit failed", err);
    } finally {
        depositing.value = false;
    }
};

onMounted(() => {
    if (props.show) {
        fetchGithubRepos();
        fetchSyncContext();
    }
});

watch(
    () => props.show,
    (newVal) => {
        if (newVal) {
            fetchGithubRepos();
            fetchSyncContext();
        }
    },
);
</script>

<template>
    <transition name="modal">
        <div
            v-if="show"
            class="fixed inset-0 bg-gray-900/60 backdrop-blur-sm flex items-center justify-center z-[100] p-4"
            @click.self="emit('close')"
        >
            <div
                class="bg-white dark:bg-gray-800 rounded-3xl max-w-lg w-full max-h-[90vh] flex flex-col shadow-2xl border border-gray-200 dark:border-gray-700 overflow-hidden"
            >
                <div
                    class="p-8 border-b border-gray-200 dark:border-gray-700 flex justify-between items-center shrink-0"
                >
                    <h3
                        class="text-2xl font-extrabold text-gray-900 dark:text-white"
                    >
                        Deposit Repository
                    </h3>
                    <button
                        @click="emit('close')"
                        class="p-2 text-gray-500 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 rounded-xl transition-all"
                    >
                        <i class="pi pi-times text-xl"></i>
                    </button>
                </div>

                <div class="flex-grow overflow-y-auto p-8 custom-scrollbar">
                    <GitHubAppGuidance
                        :installation-url="syncContext?.installation_url"
                    />

                    <form @submit.prevent="handleDeposit" class="space-y-6">
                        <GitHubRepoSelector
                            v-model="depositForm.githubRepoId"
                            :github-repositories="githubRepositories"
                            :loading="loadingGithubRepos"
                            :installation-url="syncContext?.installation_url"
                            @refresh="fetchGithubRepos(true)"
                        />

                        <div>
                            <label
                                class="block text-xs font-bold uppercase tracking-widest text-gray-600 dark:text-gray-400 mb-2"
                                >Short Description</label
                            >
                            <textarea
                                v-model="depositForm.description"
                                rows="2"
                                class="w-full px-4 py-3 bg-gray-100 dark:bg-gray-900 border border-gray-300 dark:border-gray-700 rounded-2xl text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all resize-none"
                                placeholder="What is this repository for?"
                            ></textarea>
                        </div>

                        <AuditorSelector
                            v-model="selectedAuditors"
                            :user="user"
                        />
                    </form>
                </div>

                <div
                    class="p-8 border-t border-gray-200 dark:border-gray-700 flex justify-end gap-3 shrink-0"
                >
                    <button
                        type="button"
                        @click="emit('close')"
                        class="px-6 py-3 text-gray-600 font-bold bg-gray-100 hover:bg-gray-200 rounded-2xl transition-all"
                    >
                        Cancel
                    </button>
                    <button
                        @click="handleDeposit"
                        :disabled="depositing || !depositForm.githubRepoId"
                        class="px-8 py-3 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-2xl shadow-lg shadow-blue-200 dark:shadow-none transition-all disabled:opacity-50 flex items-center gap-2"
                    >
                        <i v-if="depositing" class="pi pi-spin pi-spinner"></i>
                        Confirm Deposit
                    </button>
                </div>
            </div>
        </div>
    </transition>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
    transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.modal-enter-from,
.modal-leave-to {
    opacity: 0;
    transform: scale(0.95) translateY(20px);
}

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
