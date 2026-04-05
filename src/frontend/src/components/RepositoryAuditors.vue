<template>
    <div class="space-y-6">
        <h2 class="text-2xl font-semibold text-gray-800 dark:text-white">
            Auditors
        </h2>

        <div v-if="loading" class="flex items-center justify-center py-8">
            <i class="pi pi-spin pi-spinner text-3xl text-blue-600"></i>
        </div>

        <div v-else-if="error" class="p-4 bg-red-100 text-red-700 rounded-md">
            {{ error }}
        </div>

        <div v-else>
            <div
                v-if="props.user.id === props.repo.ownerId"
                class="relative mb-6"
            >
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
                        @click="addAuditor(user)"
                        class="p-2 hover:bg-gray-100 dark:hover:bg-gray-600 cursor-pointer flex items-center gap-2 text-sm"
                    >
                        <img
                            v-if="user.gitHubAvatarUrl || user.githubAvatarUrl"
                            :src="user.gitHubAvatarUrl || user.githubAvatarUrl"
                            class="w-6 h-6 rounded-full"
                        />
                        <span class="dark:text-white">{{
                            user.gitHubUsername || user.githubUsername
                        }}</span>
                    </div>
                </div>
            </div>

            <div
                class="flex flex-col gap-2 border border-gray-200 dark:border-gray-700 rounded-md p-4"
            >
                <!-- Owner -->
                <div
                    class="flex items-center justify-between p-2 bg-gray-50 dark:bg-gray-700/50 rounded border border-gray-100 dark:border-gray-600"
                >
                    <div class="flex items-center gap-2">
                        <span
                            class="text-sm font-medium text-gray-900 dark:text-gray-300"
                        >
                            {{ ownerUsername || "Owner" }}
                        </span>
                        <span
                            class="text-xs bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300 px-2 py-0.5 rounded"
                        >
                            Owner
                        </span>
                    </div>
                </div>

                <div
                    v-if="auditors.length === 0"
                    class="text-center py-2 text-gray-500 dark:text-gray-400 text-sm"
                >
                    No additional auditors.
                </div>

                <div
                    v-for="auditor in auditors"
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
                        >
                            {{
                                auditor.gitHubUsername || auditor.githubUsername
                            }}
                        </span>
                    </div>
                    <button
                        v-if="props.user.id === props.repo.ownerId"
                        @click="removeAuditor(auditor.id)"
                        class="text-red-500 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300 p-1"
                        title="Remove Auditor"
                    >
                        <i class="pi pi-trash"></i>
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from "vue";

const props = defineProps<{
    repo: any;
    user: any;
}>();

const auditors = ref<any[]>([]);
const userSearchQuery = ref("");
const searchResults = ref<any[]>([]);
const isSearching = ref(false);
let searchTimeout: any = null;
const ownerUsername = ref<string>("");
const loading = ref(false);
const error = ref<string | null>(null);

const fetchAuditors = async () => {
    loading.value = true;
    error.value = null;
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = { Accept: "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(
            `/api/repositories/${props.repo.id}/auditors`,
            { headers },
        );
        if (response.ok) {
            const auditorIds = await response.json();
            const filteredIds = auditorIds.filter(
                (id: string) => id !== props.repo.ownerId,
            );

            // Fetch user details for each auditor
            const userPromises = filteredIds.map(async (id: string) => {
                try {
                    const userResponse = await fetch(`/api/identities/${id}`, {
                        headers,
                    });
                    if (userResponse.ok) {
                        return await userResponse.json();
                    } else {
                        return {
                            id,
                            gitHubUsername: "Unknown",
                            githubUsername: "Unknown",
                        };
                    }
                } catch (err) {
                    console.error(`Failed to fetch user ${id}`, err);
                    return {
                        id,
                        gitHubUsername: "Unknown",
                        githubUsername: "Unknown",
                    };
                }
            });

            auditors.value = await Promise.all(userPromises);
        } else {
            auditors.value = [];
        }

        // Fetch owner username
        const ownerResponse = await fetch(
            `/api/identities/${props.repo.ownerId}`,
            { headers },
        );
        if (ownerResponse.ok) {
            const owner = await ownerResponse.json();
            ownerUsername.value =
                owner.gitHubUsername || owner.githubUsername || "Unknown";
        }
    } catch (err: any) {
        console.error(err);
        error.value =
            err.message || "An error occurred while fetching auditors.";
    } finally {
        loading.value = false;
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
                (u: any) =>
                    u.id !== props.user.id &&
                    !auditors.value.find((a) => a.id === u.id),
            );
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

const addAuditor = async (user: any) => {
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = {
            "Content-Type": "application/json",
            Accept: "application/json",
        };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(
            `/api/repositories/${props.repo.id}/auditors`,
            {
                method: "POST",
                headers,
                body: JSON.stringify([user.id]),
            },
        );

        if (response.ok) {
            auditors.value.push(user);
            userSearchQuery.value = "";
            searchResults.value = [];
        }
    } catch (err) {
        console.error("Failed to add auditor", err);
    }
};

const removeAuditor = async (userId: string) => {
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = {
            "Content-Type": "application/json",
            Accept: "application/json",
        };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(
            `/api/repositories/${props.repo.id}/auditors`,
            {
                method: "DELETE",
                headers,
                body: JSON.stringify([userId]),
            },
        );

        if (response.ok) {
            auditors.value = auditors.value.filter((a) => a.id !== userId);
        }
    } catch (err) {
        console.error("Failed to remove auditor", err);
    }
};

onMounted(() => {
    fetchAuditors();
});
</script>
