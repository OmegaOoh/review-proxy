<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import { useRouter } from "vue-router";
import CreateIssueModal from "./CreateIssueModal.vue";

const router = useRouter();

const props = defineProps<{
    user: any;
}>();

const repositories = ref<any[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const showDepositModal = ref(false);
const depositForm = ref({
    githubRepoId: "",
    description: "",
});
const depositSelectedAuditors = ref<any[]>([]);
const depositing = ref(false);

const githubRepositories = ref<any[]>([]);
const loadingGithubRepos = ref(false);

const showAuditorsModal = ref(false);
const selectedRepo = ref<any>(null);
const loadingUsers = ref(false);
const initialAuditorIds = ref<string[]>([]);
const manageSelectedAuditors = ref<any[]>([]);
const savingAuditors = ref(false);

// User search state
const userSearchQuery = ref("");
const searchResults = ref<any[]>([]);
const isSearching = ref(false);
let searchTimeout: any = null;

const allUsersCache = ref<Map<string, any>>(new Map());

// Issue creation state
const showCreateIssueModal = ref(false);
const selectedRepoForIssue = ref<any>(null);

const fetchRepositories = async () => {
    loading.value = true;
    error.value = null;
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = { Accept: "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(
            `/api/repositories?ownerId=${props.user.id}`,
            { headers },
        );
        if (response.ok) {
            repositories.value = await response.json();
        } else if (response.status === 404) {
            repositories.value = [];
        } else {
            throw new Error("Failed to load repositories");
        }
    } catch (err: any) {
        console.error(err);
        error.value =
            err.message || "An error occurred while fetching repositories.";
    } finally {
        loading.value = false;
    }
};

const depositRepository = async () => {
    if (!depositForm.value.githubRepoId) return;

    depositing.value = true;
    error.value = null;
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = {
            "Content-Type": "application/json",
            Accept: "application/json",
        };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const payload = {
            githubRepoId: depositForm.value.githubRepoId,
            description: depositForm.value.description,
            auditors: depositSelectedAuditors.value.map((a) => a.id),
        };

        const response = await fetch(`/api/repositories`, {
            method: "POST",
            headers,
            body: JSON.stringify(payload),
        });

        if (response.ok) {
            showDepositModal.value = false;
            depositForm.value = { githubRepoId: "", description: "" };
            depositSelectedAuditors.value = [];
            userSearchQuery.value = "";
            await fetchRepositories();
        } else {
            throw new Error("Failed to deposit repository");
        }
    } catch (err: any) {
        console.error(err);
        error.value =
            err.message || "An error occurred while depositing the repository.";
    } finally {
        depositing.value = false;
    }
};

const fetchGithubRepositories = async () => {
    if (githubRepositories.value.length > 0) return;
    loadingGithubRepos.value = true;
    try {
        const response = await fetch(
            `https://api.github.com/users/${props.user.gitHubUsername}/repos`,
        );
        if (response.ok) {
            githubRepositories.value = await response.json();
        }
    } catch (err) {
        console.error("Failed to fetch GitHub repositories", err);
    } finally {
        loadingGithubRepos.value = false;
    }
};

const searchUsers = async (query: string) => {
    if (!query) {
        searchResults.value = [];
        return;
    }
    isSearching.value = true;
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = { Accept: "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(
            `/api/identities?query=${encodeURIComponent(query)}`,
            { headers },
        );
        if (response.ok) {
            const users = await response.json();
            searchResults.value = users.filter(
                (u: any) => u.id !== props.user.id,
            );
            users.forEach((u: any) => allUsersCache.value.set(u.id, u));
        }
    } catch (err) {
        console.error("Failed to search users", err);
    } finally {
        isSearching.value = false;
    }
};

watch(userSearchQuery, (newVal) => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        searchUsers(newVal);
    }, 300);
});

const addAuditor = (user: any, targetList: any[]) => {
    if (!targetList.find((u) => u.id === user.id)) {
        targetList.push(user);
    }
    userSearchQuery.value = "";
    searchResults.value = [];
};

const removeAuditor = (userId: string, targetList: any[]) => {
    const idx = targetList.findIndex((u) => u.id === userId);
    if (idx !== -1) targetList.splice(idx, 1);
};

const prefetchAllUsers = async () => {
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = { Accept: "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;
        const response = await fetch(`/api/identities`, { headers });
        if (response.ok) {
            const users = await response.json();
            users.forEach((u: any) => allUsersCache.value.set(u.id, u));
        }
    } catch (err) {
        console.error(err);
    }
};

const openDepositModal = () => {
    showDepositModal.value = true;
    depositSelectedAuditors.value = [];
    userSearchQuery.value = "";
    searchResults.value = [];
    fetchGithubRepositories();
};

const openAuditorsModal = async (repo: any) => {
    selectedRepo.value = repo;
    manageSelectedAuditors.value = [];
    initialAuditorIds.value = [];
    userSearchQuery.value = "";
    searchResults.value = [];
    showAuditorsModal.value = true;
    loadingUsers.value = true;

    if (allUsersCache.value.size === 0) {
        await prefetchAllUsers();
    }

    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = { Accept: "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(`/api/repositories/${repo.id}/auditors`, {
            headers,
        });
        if (response.ok) {
            const auditorIds = await response.json();
            initialAuditorIds.value = [...auditorIds];

            for (const id of auditorIds) {
                if (id === repo.ownerId) continue;
                const cachedUser = allUsersCache.value.get(id);
                if (cachedUser) {
                    manageSelectedAuditors.value.push(cachedUser);
                } else {
                    manageSelectedAuditors.value.push({
                        id,
                        gitHubUsername: "Unknown User",
                        githubUsername: "Unknown User",
                    });
                }
            }
        }
    } catch (err) {
        console.error("Failed to fetch auditors", err);
    } finally {
        loadingUsers.value = false;
    }
};

const saveAuditors = async () => {
    if (!selectedRepo.value) return;
    savingAuditors.value = true;

    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = {
            "Content-Type": "application/json",
            Accept: "application/json",
        };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const currentSelectedIds = manageSelectedAuditors.value.map(
            (u) => u.id,
        );

        const toAdd = currentSelectedIds.filter(
            (id) =>
                !initialAuditorIds.value.includes(id) &&
                id !== selectedRepo.value.ownerId,
        );
        const toRemove = initialAuditorIds.value.filter(
            (id) =>
                !currentSelectedIds.includes(id) &&
                id !== selectedRepo.value.ownerId,
        );

        if (toAdd.length > 0) {
            await fetch(`/api/repositories/${selectedRepo.value.id}/auditors`, {
                method: "POST",
                headers,
                body: JSON.stringify(toAdd),
            });
        }

        if (toRemove.length > 0) {
            await fetch(`/api/repositories/${selectedRepo.value.id}/auditors`, {
                method: "DELETE",
                headers,
                body: JSON.stringify(toRemove),
            });
        }

        showAuditorsModal.value = false;
    } catch (err) {
        console.error("Failed to update auditors", err);
    } finally {
        savingAuditors.value = false;
    }
};

const openCreateIssueModal = (repo: any) => {
    selectedRepoForIssue.value = repo;
    showCreateIssueModal.value = true;
};

const onIssueCreated = () => {
    showCreateIssueModal.value = false;
    selectedRepoForIssue.value = null;
};

onMounted(() => {
    fetchRepositories();
    prefetchAllUsers(); // Best effort to pre-warm cache
});
</script>

<template>
    <div class="mt-8">
        <div class="flex items-center mb-6">
            <button
                @click="router.back()"
                class="mr-4 p-2 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 transition-colors rounded-full hover:bg-gray-100 dark:hover:bg-gray-800"
                title="Go Back"
            >
                <i class="pi pi-arrow-left"></i>
            </button>
            <h2
                class="text-2xl font-bold text-gray-900 dark:text-white flex-grow"
            >
                My Repositories
            </h2>
            <button
                @click="openDepositModal"
                class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors flex items-center gap-2"
            >
                <i class="pi pi-plus"></i>
                Deposit Repository
            </button>
        </div>

        <div v-if="error" class="mb-4 p-4 bg-red-100 text-red-700 rounded-lg">
            {{ error }}
        </div>

        <div v-if="loading" class="flex justify-center py-8">
            <i class="pi pi-spin pi-spinner text-3xl text-blue-600"></i>
        </div>

        <div
            v-else-if="repositories.length === 0"
            class="text-center py-12 bg-white dark:bg-gray-800 rounded-lg shadow-sm"
        >
            <i class="pi pi-folder-open text-4xl text-gray-400 mb-3"></i>
            <p class="text-gray-500 dark:text-gray-400">
                You haven't deposited any repositories yet.
            </p>
        </div>

        <div
            v-else
            class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
        >
            <div
                v-for="repo in repositories"
                :key="repo.id"
                class="p-6 bg-white dark:bg-gray-800 rounded-lg shadow-md border border-gray-100 dark:border-gray-700 flex flex-col"
            >
                <div class="flex justify-between items-start mb-4">
                    <h3
                        class="text-lg font-semibold text-gray-900 dark:text-white truncate"
                        :title="repo.githubRepoId || repo.gitHubRepoId"
                    >
                        {{ repo.githubRepoId || repo.gitHubRepoId }}
                    </h3>
                    <a
                        :href="`https://github.com/${repo.githubRepoId || repo.gitHubRepoId}`"
                        target="_blank"
                        rel="noopener noreferrer"
                        @click.stop
                        class="text-gray-400 hover:text-blue-500 transition-colors ml-2 flex-shrink-0"
                        title="View on GitHub"
                    >
                        <i class="pi pi-external-link"></i>
                    </a>
                </div>
                <p
                    class="text-gray-600 dark:text-gray-300 text-sm mb-4 h-10 overflow-hidden text-ellipsis line-clamp-2 flex-grow"
                >
                    {{ repo.description || "No description provided." }}
                </p>
                <div
                    class="flex items-center justify-between text-xs text-gray-500 dark:text-gray-400 mt-auto pt-4 border-t border-gray-100 dark:border-gray-700"
                >
                    <span>ID: {{ repo.id.substring(0, 8) }}...</span>
                    <div class="flex gap-2">
                        <button
                            @click="openCreateIssueModal(repo)"
                            class="text-green-600 hover:text-green-800 dark:text-green-400 dark:hover:text-green-300 flex items-center gap-1 transition-colors"
                        >
                            <i class="pi pi-plus"></i>
                            Create Issue
                        </button>
                        <button
                            v-if="repo.ownerId === props.user.id"
                            @click="openAuditorsModal(repo)"
                            class="text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 flex items-center gap-1 transition-colors"
                        >
                            <i class="pi pi-users"></i>
                            Manage Auditors
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Deposit Modal -->
        <div
            v-if="showDepositModal"
            class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
        >
            <div
                class="bg-white dark:bg-gray-800 rounded-lg max-w-md w-full p-6 shadow-xl overflow-visible"
            >
                <div class="flex justify-between items-center mb-4">
                    <h3 class="text-xl font-bold text-gray-900 dark:text-white">
                        Deposit Repository
                    </h3>
                    <button
                        @click="showDepositModal = false"
                        class="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                    >
                        <i class="pi pi-times"></i>
                    </button>
                </div>

                <form @submit.prevent="depositRepository" class="space-y-4">
                    <div>
                        <label
                            class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                            >GitHub Repository</label
                        >
                        <select
                            v-model="depositForm.githubRepoId"
                            required
                            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                        >
                            <option value="" disabled>
                                Select a repository
                            </option>
                            <option v-if="loadingGithubRepos" value="" disabled>
                                Loading...
                            </option>
                            <option
                                v-for="repo in githubRepositories"
                                :key="repo.id"
                                :value="repo.full_name"
                            >
                                {{ repo.full_name }}
                            </option>
                        </select>
                    </div>

                    <div>
                        <label
                            class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                            >Description</label
                        >
                        <textarea
                            v-model="depositForm.description"
                            rows="2"
                            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                            placeholder="Brief description..."
                        ></textarea>
                    </div>

                    <div class="relative">
                        <label
                            class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                            >Add Auditors</label
                        >
                        <input
                            type="text"
                            v-model="userSearchQuery"
                            placeholder="Search GitHub username..."
                            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                        />
                        <div
                            v-if="userSearchQuery"
                            class="absolute z-10 mt-1 w-full bg-white dark:bg-gray-700 shadow-lg rounded-md border border-gray-200 dark:border-gray-600 max-h-48 overflow-auto"
                        >
                            <div
                                v-if="isSearching"
                                class="p-2 text-sm text-gray-500 text-center"
                            >
                                Searching...
                            </div>
                            <div
                                v-else-if="searchResults.length === 0"
                                class="p-2 text-sm text-gray-500 text-center"
                            >
                                No users found
                            </div>
                            <div
                                v-for="user in searchResults"
                                :key="user.id"
                                @click="
                                    addAuditor(user, depositSelectedAuditors)
                                "
                                class="p-2 hover:bg-gray-100 dark:hover:bg-gray-600 cursor-pointer flex items-center gap-2 text-sm"
                            >
                                <img
                                    v-if="
                                        user.gitHubAvatarUrl ||
                                        user.githubAvatarUrl
                                    "
                                    :src="
                                        user.gitHubAvatarUrl ||
                                        user.githubAvatarUrl
                                    "
                                    class="w-6 h-6 rounded-full"
                                />
                                <span class="dark:text-white">{{
                                    user.gitHubUsername || user.githubUsername
                                }}</span>
                            </div>
                        </div>

                        <div class="mt-2 flex flex-wrap gap-2">
                            <span
                                v-for="auditor in depositSelectedAuditors"
                                :key="auditor.id"
                                class="inline-flex items-center gap-1 px-2 py-1 bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300 text-xs font-medium rounded-full"
                            >
                                {{
                                    auditor.gitHubUsername ||
                                    auditor.githubUsername
                                }}
                                <button
                                    type="button"
                                    @click="
                                        removeAuditor(
                                            auditor.id,
                                            depositSelectedAuditors,
                                        )
                                    "
                                    class="text-blue-600 hover:text-blue-900 dark:text-blue-400 dark:hover:text-blue-100"
                                >
                                    <i class="pi pi-times text-[10px]"></i>
                                </button>
                            </span>
                        </div>
                    </div>

                    <div class="flex justify-end gap-3 mt-6">
                        <button
                            type="button"
                            @click="showDepositModal = false"
                            class="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 dark:text-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 rounded-lg font-medium transition-colors"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            :disabled="depositing || !depositForm.githubRepoId"
                            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                        >
                            <i
                                v-if="depositing"
                                class="pi pi-spin pi-spinner"
                            ></i>
                            Deposit
                        </button>
                    </div>
                </form>
            </div>
        </div>

        <!-- Auditors Modal -->
        <div
            v-if="showAuditorsModal"
            class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
        >
            <div
                class="bg-white dark:bg-gray-800 rounded-lg max-w-md w-full p-6 shadow-xl flex flex-col max-h-[90vh] overflow-visible"
            >
                <div class="flex justify-between items-center mb-4">
                    <h3 class="text-xl font-bold text-gray-900 dark:text-white">
                        Manage Auditors
                    </h3>
                    <button
                        @click="showAuditorsModal = false"
                        class="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                    >
                        <i class="pi pi-times"></i>
                    </button>
                </div>

                <div v-if="loadingUsers" class="flex justify-center py-8">
                    <i class="pi pi-spin pi-spinner text-2xl text-blue-600"></i>
                </div>

                <div v-else class="flex flex-col gap-4">
                    <div class="relative">
                        <label
                            class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                            >Search & Add Auditor</label
                        >
                        <input
                            type="text"
                            v-model="userSearchQuery"
                            placeholder="Search GitHub username..."
                            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                        />
                        <div
                            v-if="userSearchQuery"
                            class="absolute z-10 mt-1 w-full bg-white dark:bg-gray-700 shadow-lg rounded-md border border-gray-200 dark:border-gray-600 max-h-48 overflow-auto"
                        >
                            <div
                                v-if="isSearching"
                                class="p-2 text-sm text-gray-500 text-center"
                            >
                                Searching...
                            </div>
                            <div
                                v-else-if="searchResults.length === 0"
                                class="p-2 text-sm text-gray-500 text-center"
                            >
                                No users found
                            </div>
                            <div
                                v-for="user in searchResults"
                                :key="user.id"
                                @click="
                                    addAuditor(user, manageSelectedAuditors)
                                "
                                class="p-2 hover:bg-gray-100 dark:hover:bg-gray-600 cursor-pointer flex items-center gap-2 text-sm"
                            >
                                <img
                                    v-if="
                                        user.gitHubAvatarUrl ||
                                        user.githubAvatarUrl
                                    "
                                    :src="
                                        user.gitHubAvatarUrl ||
                                        user.githubAvatarUrl
                                    "
                                    class="w-6 h-6 rounded-full"
                                />
                                <span class="dark:text-white">{{
                                    user.gitHubUsername || user.githubUsername
                                }}</span>
                            </div>
                        </div>
                    </div>

                    <div>
                        <label
                            class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                            >Current Auditors</label
                        >
                        <div
                            class="flex flex-col gap-2 border border-gray-200 dark:border-gray-700 rounded-md p-2 max-h-60 overflow-y-auto"
                        >
                            <!-- Owner is always an auditor, display them specifically -->
                            <div
                                class="flex items-center justify-between p-2 bg-gray-50 dark:bg-gray-700/50 rounded border border-gray-100 dark:border-gray-600"
                            >
                                <div class="flex items-center gap-2">
                                    <span
                                        class="text-sm font-medium text-gray-900 dark:text-gray-300"
                                        >{{ props.user.gitHubUsername }}</span
                                    >
                                    <span
                                        class="text-xs bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300 px-2 py-0.5 rounded"
                                        >Owner</span
                                    >
                                </div>
                            </div>

                            <div
                                v-if="manageSelectedAuditors.length === 0"
                                class="text-center py-2 text-gray-500 dark:text-gray-400 text-sm"
                            >
                                No additional auditors.
                            </div>

                            <div
                                v-for="auditor in manageSelectedAuditors"
                                :key="auditor.id"
                                class="flex items-center justify-between p-2 hover:bg-gray-50 dark:hover:bg-gray-700 rounded border border-transparent hover:border-gray-200 dark:hover:border-gray-600 transition-colors"
                            >
                                <div class="flex items-center gap-2">
                                    <img
                                        v-if="
                                            auditor.gitHubAvatarUrl ||
                                            auditor.githubAvatarUrl
                                        "
                                        :src="
                                            auditor.gitHubAvatarUrl ||
                                            auditor.githubAvatarUrl
                                        "
                                        class="w-6 h-6 rounded-full"
                                    />
                                    <span
                                        class="text-sm font-medium text-gray-900 dark:text-gray-300"
                                        >{{
                                            auditor.gitHubUsername ||
                                            auditor.githubUsername
                                        }}</span
                                    >
                                </div>
                                <button
                                    @click="
                                        removeAuditor(
                                            auditor.id,
                                            manageSelectedAuditors,
                                        )
                                    "
                                    class="text-red-500 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300 p-1"
                                    title="Remove Auditor"
                                >
                                    <i class="pi pi-trash"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>

                <div
                    class="flex justify-end gap-3 mt-6 pt-4 border-t border-gray-100 dark:border-gray-700"
                >
                    <button
                        type="button"
                        @click="showAuditorsModal = false"
                        class="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 dark:text-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 rounded-lg font-medium transition-colors"
                    >
                        Cancel
                    </button>
                    <button
                        @click="saveAuditors"
                        :disabled="savingAuditors"
                        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                    >
                        <i
                            v-if="savingAuditors"
                            class="pi pi-spin pi-spinner"
                        ></i>
                        Save Changes
                    </button>
                </div>
            </div>
        </div>
    </div>

    <CreateIssueModal
        :repo="selectedRepoForIssue"
        :user="props.user"
        :show="showCreateIssueModal"
        @close="showCreateIssueModal = false"
        @created="onIssueCreated"
    />
</template>
