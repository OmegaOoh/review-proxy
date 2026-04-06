<template>
    <div class="mt-8">
        <div v-if="loading" class="flex justify-center py-8">
            <i class="pi pi-spin pi-spinner text-3xl text-blue-600"></i>
        </div>
        <div v-else-if="error" class="p-4 bg-red-100 text-red-700 rounded-md">
            {{ error }}
        </div>
        <div v-else-if="repo">
            <div class="mb-6">
                <button
                    @click="goBack"
                    class="mb-4 px-4 py-2 bg-gray-600 text-white rounded hover:bg-gray-700 transition"
                >
                    ← Back
                </button>
                <div class="flex items-center gap-3 mb-2">
                    <h1
                        class="text-3xl font-bold text-gray-900 dark:text-white"
                    >
                        {{ repo.githubRepoId || repo.gitHubRepoId }}
                    </h1>
                    <a
                        :href="`https://github.com/${repo.githubRepoId || repo.gitHubRepoId}`"
                        target="_blank"
                        rel="noopener noreferrer"
                        class="text-gray-400 hover:text-blue-500 transition-colors mt-1"
                        title="View on GitHub"
                    >
                        <i class="pi pi-external-link text-xl"></i>
                    </a>
                </div>
                <div v-if="isEditingDescription" class="flex gap-2">
                    <input
                        v-model="editDescriptionText"
                        class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                        placeholder="Repository description..."
                    />
                    <button
                        @click="saveDescription"
                        class="px-3 py-2 bg-green-600 hover:bg-green-700 text-white rounded"
                    >
                        Save
                    </button>
                    <button
                        @click="cancelEditDescription"
                        class="px-3 py-2 bg-gray-500 hover:bg-gray-600 text-white rounded"
                    >
                        Cancel
                    </button>
                </div>
                <div v-else class="flex items-center gap-2">
                    <p class="text-gray-600 dark:text-gray-300">
                        {{ repo.description || "No description provided." }}
                    </p>
                    <button
                        v-if="props.user.id === repo.ownerId"
                        @click="startEditDescription"
                        class="text-blue-500 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
                        title="Edit Description"
                    >
                        <i class="pi pi-pencil"></i>
                    </button>
                </div>
            </div>

            <div class="mb-4 border-b border-gray-200 dark:border-gray-700">
                <nav class="flex space-x-4" aria-label="Tabs">
                    <button
                        @click="activeTab = 'issues'"
                        :class="[
                            activeTab === 'issues'
                                ? 'bg-gray-100 text-gray-900 dark:bg-gray-800 dark:text-white border-b-2 border-blue-500'
                                : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300',
                            'px-3 py-2 font-medium text-sm rounded-t-md transition-colors',
                        ]"
                    >
                        Issues
                    </button>
                    <button
                        @click="activeTab = 'auditors'"
                        :class="[
                            activeTab === 'auditors'
                                ? 'bg-gray-100 text-gray-900 dark:bg-gray-800 dark:text-white border-b-2 border-blue-500'
                                : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300',
                            'px-3 py-2 font-medium text-sm rounded-t-md transition-colors',
                        ]"
                    >
                        Auditors
                    </button>
                </nav>
            </div>

            <div v-show="activeTab === 'issues'">
                <RepositoryIssues :repo="repo" :user="props.user" />
            </div>
            <div v-show="activeTab === 'auditors'">
                <RepositoryAuditors :repo="repo" :user="props.user" />
            </div>
        </div>
        <div v-else>
            <p class="text-gray-500 dark:text-gray-400">
                Repository not found.
            </p>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import RepositoryIssues from "./RepositoryIssues.vue";
import RepositoryAuditors from "./RepositoryAuditors.vue";

const props = defineProps<{
    user: any;
}>();

const route = useRoute();
const router = useRouter();
const repoId = route.params.id as string;

const repo = ref<any>(history.state?.repo || null);
const loading = ref(false);
const error = ref<string | null>(null);

const activeTab = ref("issues");

const isEditingDescription = ref(false);
const editDescriptionText = ref("");

const startEditDescription = () => {
    editDescriptionText.value = repo.value.description || "";
    isEditingDescription.value = true;
};

const cancelEditDescription = () => {
    isEditingDescription.value = false;
};

const saveDescription = async () => {
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = {
            "Content-Type": "application/json",
            Accept: "application/json",
        };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const payload = {
            description: editDescriptionText.value,
        };

        const response = await fetch(`/api/repositories/${repo.value.id}`, {
            method: "PATCH",
            headers,
            body: JSON.stringify(payload),
        });

        if (response.ok) {
            repo.value.description = editDescriptionText.value;
            isEditingDescription.value = false;
        } else {
            console.error("Failed to update description");
        }
    } catch (err) {
        console.error("Error updating description", err);
    }
};

const fetchRepo = async () => {
    loading.value = true;
    error.value = null;
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = { Accept: "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(`/api/repositories`, {
            headers,
        });
        if (response.ok) {
            const allRepos = await response.json();
            repo.value = allRepos.find((r: any) => r.id === repoId);
            if (!repo.value) {
                error.value = "Repository not found";
            }
        } else if (response.status === 404) {
            error.value = "Repository not found";
        } else {
            throw new Error("Failed to fetch repositories");
        }
    } catch (err: any) {
        console.error(err);
        error.value =
            err.message || "An error occurred while fetching repository.";
    } finally {
        loading.value = false;
    }
};

onMounted(() => {
    if (!repo.value) {
        fetchRepo();
    }
});

const goBack = () => {
    router.back();
};
</script>
