<script setup lang="ts">
import { ref, onMounted, watch } from "vue";

const props = defineProps<{
    user: any;
}>();

const repositories = ref<any[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);
const loadingAuditors = ref(false);

// User search state
const userSearchQuery = ref("");
const searchResults = ref<any[]>([]);
const isSearching = ref(false);
let searchTimeout: any = null;

const allUsersCache = ref<Map<string, any>>(new Map());

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
            await fetchAuditorsForAllRepos();
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

const fetchAuditorsForAllRepos = async () => {
    loadingAuditors.value = true;
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = { Accept: "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const promises = repositories.value.map(async (repo) => {
            try {
                const response = await fetch(
                    `/api/repositories/${repo.id}/auditors`,
                    { headers },
                );
                if (response.ok) {
                    const auditorIds = await response.json();
                    repo.auditors = auditorIds
                        .filter((id: string) => id !== repo.ownerId)
                        .map((id: string) => allUsersCache.value.get(id))
                        .filter(Boolean);
                    initializeAuditors(repo);
                }
            } catch (err) {
                console.error(
                    `Failed to fetch auditors for repo ${repo.id}`,
                    err,
                );
            }
        });

        await Promise.all(promises);
    } catch (err) {
        console.error("Failed to fetch auditors", err);
    } finally {
        loadingAuditors.value = false;
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

const addAuditor = (user: any, repo: any) => {
    // Add to repo's auditors
    if (!repo.auditors) repo.auditors = [];
    if (!repo.auditors.find((u: any) => u.id === user.id)) {
        repo.auditors.push(user);
    }
    userSearchQuery.value = "";
    searchResults.value = [];
};

const removeAuditor = (userId: string, repo: any) => {
    if (repo.auditors) {
        repo.auditors = repo.auditors.filter((u: any) => u.id !== userId);
    }
};

const saveAuditors = async (repo: any) => {
    if (!repo) return;
    const savingKey = `saving-${repo.id}`;
    (repo as any)[savingKey] = true;

    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = {
            "Content-Type": "application/json",
            Accept: "application/json",
        };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const currentAuditorIds = repo.auditors?.map((u: any) => u.id) || [];
        const initialAuditorIds = repo.initialAuditors || [];

        const toAdd = currentAuditorIds.filter(
            (id: string) =>
                !initialAuditorIds.includes(id) && id !== repo.ownerId,
        );
        const toRemove = initialAuditorIds.filter(
            (id: string) =>
                !currentAuditorIds.includes(id) && id !== repo.ownerId,
        );

        if (toAdd.length > 0) {
            await fetch(`/api/repositories/${repo.id}/auditors`, {
                method: "POST",
                headers,
                body: JSON.stringify(toAdd),
            });
        }

        if (toRemove.length > 0) {
            await fetch(`/api/repositories/${repo.id}/auditors`, {
                method: "DELETE",
                headers,
                body: JSON.stringify(toRemove),
            });
        }

        repo.initialAuditors = [...currentAuditorIds];
    } catch (err) {
        console.error("Failed to update auditors", err);
    } finally {
        (repo as any)[savingKey] = false;
    }
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

onMounted(() => {
    fetchRepositories();
    prefetchAllUsers(); // Ensure users are cached before fetching auditors
});

const initializeAuditors = (repo: any) => {
    repo.initialAuditors = repo.auditors?.map((u: any) => u.id) || [];
};
</script>

<template>
    <div class="mt-8">
        <div class="flex justify-between items-center mb-6">
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
                Auditor Manager
            </h2>
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

        <div v-else class="space-y-6">
            <div
                v-for="repo in repositories"
                :key="repo.id"
                class="bg-white dark:bg-gray-800 rounded-lg shadow-md border border-gray-100 dark:border-gray-700 p-6"
            >
                <div class="flex justify-between items-start mb-4">
                    <h3
                        class="text-lg font-semibold text-gray-900 dark:text-white truncate"
                        :title="repo.githubRepoId || repo.gitHubRepoId"
                    >
                        {{ repo.githubRepoId || repo.gitHubRepoId }}
                    </h3>
                </div>
                <p class="text-gray-600 dark:text-gray-300 text-sm mb-4">
                    {{ repo.description || "No description provided." }}
                </p>

                <div class="flex justify-end mb-4">
                    <button
                        @click="saveAuditors(repo)"
                        :disabled="(repo as any)[`saving-${repo.id}`]"
                        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                    >
                        <i
                            v-if="(repo as any)[`saving-${repo.id}`]"
                            class="pi pi-spin pi-spinner"
                        ></i>
                        Save Auditors
                    </button>
                </div>

                <div class="relative mb-4">
                    <label
                        class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                        >Add Auditor</label
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
                            @click="addAuditor(user, repo)"
                            class="p-2 hover:bg-gray-100 dark:hover:bg-gray-600 cursor-pointer flex items-center gap-2 text-sm"
                        >
                            <img
                                v-if="
                                    user.gitHubAvatarUrl || user.githubAvatarUrl
                                "
                                :src="
                                    user.gitHubAvatarUrl || user.githubAvatarUrl
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
                        <!-- Owner is always an auditor -->
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
                            v-if="!repo.auditors || repo.auditors.length === 0"
                            class="text-center py-2 text-gray-500 dark:text-gray-400 text-sm"
                        >
                            No additional auditors.
                        </div>

                        <div
                            v-for="auditor in repo.auditors"
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
                                @click="removeAuditor(auditor.id, repo)"
                                class="text-red-500 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300 p-1"
                                title="Remove Auditor"
                            >
                                <i class="pi pi-trash"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
