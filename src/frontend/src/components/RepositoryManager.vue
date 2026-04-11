<template>
    <div class="space-y-8">
        <div
            class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4"
        >
            <div>
                <button
                    @click="router.back()"
                    class="mb-2 text-sm text-gray-500 hover:text-gray-900 dark:hover:text-white transition-colors flex items-center gap-1"
                >
                    <i class="pi pi-arrow-left text-[10px]"></i>
                    Back
                </button>
                <h2
                    class="text-3xl font-extrabold text-gray-900 dark:text-white tracking-tight"
                >
                    My Repositories
                </h2>
                <p class="text-gray-500">
                    Deposit and configure your GitHub repositories.
                </p>
            </div>
            <button
                @click="openDepositModal"
                class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-2xl shadow-lg shadow-blue-200 dark:shadow-none transition-all flex items-center gap-2"
            >
                <i class="pi pi-plus"></i>
                Deposit Repository
            </button>
        </div>

        <div
            v-if="error"
            class="p-4 bg-red-50 text-red-700 rounded-2xl border border-red-100 flex items-center gap-3"
        >
            <i class="pi pi-exclamation-circle text-xl"></i>
            {{ error }}
        </div>

        <div
            v-if="loading"
            class="flex flex-col items-center justify-center py-20"
        >
            <i class="pi pi-spin pi-spinner text-4xl text-blue-600 mb-4"></i>
            <span class="text-gray-500">Loading your repositories...</span>
        </div>

        <div
            v-else-if="myRepositories.length === 0"
            class="py-20 text-center bg-white dark:bg-gray-800 rounded-3xl shadow-sm border border-gray-100 dark:border-gray-700"
        >
            <div
                class="w-20 h-20 bg-gray-50 dark:bg-gray-700 rounded-full flex items-center justify-center mx-auto mb-4"
            >
                <i class="pi pi-folder-open text-4xl text-gray-300"></i>
            </div>
            <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-2">
                No active deposits
            </h3>
            <p class="text-gray-500 mb-8">
                You haven't deposited any repositories to Review Proxy yet.
            </p>
            <button
                @click="openDepositModal"
                class="px-8 py-3 bg-gray-900 text-white dark:bg-white dark:text-gray-900 rounded-2xl font-bold hover:opacity-90 transition-all"
            >
                Start Depositing
            </button>
        </div>

        <div
            v-else
            class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
        >
            <div
                v-for="repo in myRepositories"
                :key="repo.id"
                class="group p-6 bg-white dark:bg-gray-800 rounded-3xl shadow-sm border border-gray-100 dark:border-gray-700 flex flex-col hover:border-blue-200 dark:hover:border-blue-900/50 transition-all duration-300"
            >
                <div class="flex justify-between items-start mb-4">
                    <h3
                        class="text-lg font-bold text-gray-900 dark:text-white truncate pr-2"
                    >
                        {{ repo.gitHubRepoId }}
                    </h3>
                    <a
                        :href="`https://github.com/${repo.gitHubRepoId}`"
                        target="_blank"
                        rel="noopener noreferrer"
                        class="p-2 text-gray-400 hover:text-blue-500 transition-colors"
                        title="View on GitHub"
                    >
                        <i class="pi pi-github"></i>
                    </a>
                </div>

                <p
                    class="text-gray-600 dark:text-gray-400 text-sm mb-6 line-clamp-2 h-10"
                >
                    {{ repo.description || "No description provided." }}
                </p>

                <div
                    class="mt-auto pt-6 border-t border-gray-50 dark:border-gray-700/50 flex flex-wrap gap-2"
                >
                    <button
                        @click="openCreateIssueModal(repo)"
                        class="flex-1 px-3 py-2 text-xs font-bold bg-green-50 text-green-600 dark:bg-green-900/20 dark:text-green-400 rounded-xl hover:bg-green-100 transition-colors flex items-center justify-center gap-2"
                    >
                        <i class="pi pi-plus"></i> Issue
                    </button>
                    <button
                        @click="router.push(`/repository/${repo.id}`)"
                        class="flex-1 px-3 py-2 text-xs font-bold bg-blue-50 text-blue-600 dark:bg-blue-900/20 dark:text-blue-400 rounded-xl hover:bg-blue-100 transition-colors flex items-center justify-center gap-2"
                    >
                        <i class="pi pi-cog"></i> Config
                    </button>
                </div>
            </div>
        </div>

        <!-- Deposit Modal -->
        <transition name="modal">
            <div
                v-if="showDepositModal"
                class="fixed inset-0 bg-gray-900/60 backdrop-blur-sm flex items-center justify-center z-[100] p-4"
                @click.self="showDepositModal = false"
            >
                <div
                    class="bg-white dark:bg-gray-800 rounded-3xl max-w-lg w-full p-8 shadow-2xl border border-gray-100 dark:border-gray-700"
                >
                    <div class="flex justify-between items-center mb-8">
                        <h3
                            class="text-2xl font-extrabold text-gray-900 dark:text-white"
                        >
                            Deposit Repository
                        </h3>
                        <button
                            @click="showDepositModal = false"
                            class="p-2 text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 rounded-xl transition-all"
                        >
                            <i class="pi pi-times text-xl"></i>
                        </button>
                    </div>

                    <form @submit.prevent="handleDeposit" class="space-y-6">
                        <div>
                            <label
                                class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2"
                                >Target GitHub Repository</label
                            >
                            <div class="relative">
                                <select
                                    v-model="depositForm.githubRepoId"
                                    required
                                    class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-900 border border-gray-200 dark:border-gray-700 rounded-2xl text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all appearance-none"
                                >
                                    <option value="" disabled>
                                        Select from your account
                                    </option>
                                    <option v-if="loadingGithubRepos" disabled>
                                        Fetching repos...
                                    </option>
                                    <option
                                        v-for="repo in githubRepositories"
                                        :key="repo.id"
                                        :value="repo.full_name"
                                    >
                                        {{ repo.full_name }}
                                    </option>
                                </select>
                                <i
                                    class="pi pi-chevron-down absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"
                                ></i>
                            </div>
                        </div>

                        <div>
                            <label
                                class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2"
                                >Short Description</label
                            >
                            <textarea
                                v-model="depositForm.description"
                                rows="2"
                                class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-900 border border-gray-200 dark:border-gray-700 rounded-2xl text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all resize-none"
                                placeholder="What is this repository for?"
                            ></textarea>
                        </div>

                        <div class="relative">
                            <label
                                class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2"
                                >Initial Auditors</label
                            >
                            <div class="relative mb-3">
                                <i
                                    class="pi pi-search absolute left-4 top-1/2 -translate-y-1/2 text-gray-400"
                                ></i>
                                <input
                                    type="text"
                                    v-model="userSearchQuery"
                                    placeholder="Search by username..."
                                    class="w-full pl-11 pr-4 py-3 bg-gray-50 dark:bg-gray-900 border border-gray-200 dark:border-gray-700 rounded-2xl text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all"
                                />
                            </div>

                            <!-- User Search Results -->
                            <div
                                v-if="
                                    userSearchQuery &&
                                    (isSearching || searchResults.length > 0)
                                "
                                class="absolute z-[110] mt-1 w-full bg-white dark:bg-gray-800 shadow-2xl rounded-2xl border border-gray-100 dark:border-gray-700 max-h-48 overflow-auto p-2"
                            >
                                <div
                                    v-if="isSearching"
                                    class="p-4 text-center text-sm text-gray-500"
                                >
                                    <i class="pi pi-spin pi-spinner mr-2"></i
                                    >Searching...
                                </div>
                                <div
                                    v-for="user in searchResults"
                                    :key="user.id"
                                    @click="addAuditor(user)"
                                    class="p-3 hover:bg-gray-50 dark:hover:bg-gray-700/50 cursor-pointer rounded-xl flex items-center justify-between group transition-colors"
                                >
                                    <div class="flex items-center gap-3">
                                        <img
                                            :src="user.gitHubAvatarUrl"
                                            class="w-8 h-8 rounded-full"
                                        />
                                        <span
                                            class="font-bold text-gray-900 dark:text-white text-sm"
                                            >{{ user.gitHubUsername }}</span
                                        >
                                    </div>
                                    <i
                                        class="pi pi-plus text-blue-500 opacity-0 group-hover:opacity-100 transition-opacity"
                                    ></i>
                                </div>
                            </div>

                            <!-- Selected Auditors Chips -->
                            <div class="flex flex-wrap gap-2">
                                <span
                                    v-for="auditor in selectedAuditors"
                                    :key="auditor.id"
                                    class="inline-flex items-center gap-2 pl-2 pr-1.5 py-1.5 bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400 text-xs font-bold rounded-xl border border-blue-100 dark:border-blue-800/50"
                                >
                                    <img
                                        :src="auditor.gitHubAvatarUrl"
                                        class="w-4 h-4 rounded-full"
                                    />
                                    {{ auditor.gitHubUsername }}
                                    <button
                                        type="button"
                                        @click="removeAuditor(auditor.id)"
                                        class="p-1 hover:bg-blue-200 dark:hover:bg-blue-800 rounded-lg transition-colors"
                                    >
                                        <i class="pi pi-times text-[10px]"></i>
                                    </button>
                                </span>
                            </div>
                        </div>

                        <div class="flex justify-end gap-3 mt-10">
                            <button
                                type="button"
                                @click="showDepositModal = false"
                                class="px-6 py-3 text-gray-600 font-bold bg-gray-100 hover:bg-gray-200 rounded-2xl transition-all"
                            >
                                Cancel
                            </button>
                            <button
                                type="submit"
                                :disabled="
                                    depositing || !depositForm.githubRepoId
                                "
                                class="px-8 py-3 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-2xl shadow-lg shadow-blue-200 dark:shadow-none transition-all disabled:opacity-50 flex items-center gap-2"
                            >
                                <i
                                    v-if="depositing"
                                    class="pi pi-spin pi-spinner"
                                ></i>
                                Confirm Deposit
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </transition>

        <CreateIssueModal
            v-if="selectedRepoForIssue"
            :repo="selectedRepoForIssue"
            :user="props.user"
            :show="showCreateIssueModal"
            @close="closeCreateIssueModal"
            @created="onIssueCreated"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from "vue";
import { useRouter } from "vue-router";
import { storeToRefs } from "pinia";
import { useRepoStore } from "../stores/repositories";
import { api } from "../api/client";
import CreateIssueModal from "./CreateIssueModal.vue";
import type { User, Repository } from "../types";

const router = useRouter();
const repoStore = useRepoStore();
const { repositories, loading, error } = storeToRefs(repoStore);

const props = defineProps<{
    user: User;
}>();

const myRepositories = computed(() =>
    repositories.value.filter((r) => r.ownerId === props.user.id),
);

// Deposit State
const showDepositModal = ref(false);
const depositForm = ref({
    githubRepoId: "",
    description: "",
});
const selectedAuditors = ref<User[]>([]);
const depositing = ref(false);
const githubRepositories = ref<any[]>([]);
const loadingGithubRepos = ref(false);

// Search State
const userSearchQuery = ref("");
const searchResults = ref<User[]>([]);
const isSearching = ref(false);
let searchTimeout: any = null;

// Issue State
const showCreateIssueModal = ref(false);
const selectedRepoForIssue = ref<Repository | null>(null);

const fetchGithubRepos = async () => {
    if (githubRepositories.value.length > 0) return;
    loadingGithubRepos.value = true;
    try {
        githubRepositories.value = await api.get<any[]>(
            "/api/sync/repositories",
        );
    } catch (err) {
        console.error("Failed to fetch GitHub repos", err);
    } finally {
        loadingGithubRepos.value = false;
    }
};

const handleSearchUsers = async (query: string) => {
    if (!query) {
        searchResults.value = [];
        return;
    }
    isSearching.value = true;
    try {
        const users = await api.get<User[]>(
            `/api/identities?query=${encodeURIComponent(query)}`,
        );
        searchResults.value = users.filter((u) => u.id !== props.user.id);
    } catch (err) {
        console.error("User search failed", err);
    } finally {
        isSearching.value = false;
    }
};

watch(userSearchQuery, (newVal) => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => handleSearchUsers(newVal), 400);
});

const openDepositModal = () => {
    showDepositModal.value = true;
    fetchGithubRepos();
};

const addAuditor = (user: User) => {
    if (!selectedAuditors.value.find((u) => u.id === user.id)) {
        selectedAuditors.value.push(user);
    }
    userSearchQuery.value = "";
    searchResults.value = [];
};

const removeAuditor = (id: string) => {
    selectedAuditors.value = selectedAuditors.value.filter((u) => u.id !== id);
};

const handleDeposit = async () => {
    depositing.value = true;
    try {
        const githubToken = localStorage.getItem("github_token");
        await repoStore.depositRepository({
            githubRepoId: depositForm.value.githubRepoId,
            description: depositForm.value.description,
            gitHubToken: githubToken || undefined,
            auditors: selectedAuditors.value.map((a) => a.id),
        });
        showDepositModal.value = false;
        depositForm.value = { githubRepoId: "", description: "" };
        selectedAuditors.value = [];
    } catch (err) {
        console.error("Deposit failed", err);
    } finally {
        depositing.value = false;
    }
};

const openCreateIssueModal = (repo: Repository) => {
    selectedRepoForIssue.value = repo;
    showCreateIssueModal.value = true;
};

const closeCreateIssueModal = () => {
    showCreateIssueModal.value = false;
    selectedRepoForIssue.value = null;
};

const onIssueCreated = () => {
    closeCreateIssueModal();
    // Repository store might need refreshing or we just navigate
    router.push(`/repository/${selectedRepoForIssue.value?.id}`);
};

onMounted(() => {
    repoStore.fetchAllRepositories();
});
</script>

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
</style>
