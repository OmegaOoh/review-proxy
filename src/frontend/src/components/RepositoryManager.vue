<script setup lang="ts">
import { ref, onMounted } from 'vue';

const props = defineProps<{
    user: any;
}>();

const repositories = ref<any[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const showDepositModal = ref(false);
const depositForm = ref({
    githubRepoId: '',
    description: ''
});
const depositing = ref(false);

const fetchRepositories = async () => {
    loading.value = true;
    error.value = null;
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = {
            'Accept': 'application/json'
        };
        if (token) {
            headers["Authorization"] = `Bearer ${token}`;
        }

        // Fetching repositories owned by the current user
        const response = await fetch(`/api/repositories?ownerId=${props.user.id}`, { headers });
        if (response.ok) {
            repositories.value = await response.json();
        } else if (response.status === 404) {
            repositories.value = [];
        } else {
            throw new Error('Failed to load repositories');
        }
    } catch (err: any) {
        console.error(err);
        error.value = err.message || 'An error occurred while fetching repositories.';
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
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
        if (token) {
            headers["Authorization"] = `Bearer ${token}`;
        }

        const payload = {
            githubRepoId: depositForm.value.githubRepoId,
            ownerId: props.user.id,
            description: depositForm.value.description
        };

        const response = await fetch(`/api/repositories`, {
            method: 'POST',
            headers,
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            showDepositModal.value = false;
            depositForm.value = { githubRepoId: '', description: '' };
            await fetchRepositories();
        } else {
            throw new Error('Failed to deposit repository');
        }
    } catch (err: any) {
        console.error(err);
        error.value = err.message || 'An error occurred while depositing the repository.';
    } finally {
        depositing.value = false;
    }
};

onMounted(() => {
    fetchRepositories();
});
</script>

<template>
    <div class="mt-8">
        <div class="flex justify-between items-center mb-6">
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">My Repositories</h2>
            <button
                @click="showDepositModal = true"
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

        <div v-else-if="repositories.length === 0" class="text-center py-12 bg-white dark:bg-gray-800 rounded-lg shadow-sm">
            <i class="pi pi-folder-open text-4xl text-gray-400 mb-3"></i>
            <p class="text-gray-500 dark:text-gray-400">You haven't deposited any repositories yet.</p>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div
                v-for="repo in repositories"
                :key="repo.id"
                class="p-6 bg-white dark:bg-gray-800 rounded-lg shadow-md border border-gray-100 dark:border-gray-700"
            >
                <div class="flex justify-between items-start mb-4">
                    <h3 class="text-lg font-semibold text-gray-900 dark:text-white truncate" :title="repo.githubRepoId">
                        {{ repo.githubRepoId }}
                    </h3>
                </div>
                <p class="text-gray-600 dark:text-gray-300 text-sm mb-4 h-10 overflow-hidden text-ellipsis line-clamp-2">
                    {{ repo.description || 'No description provided.' }}
                </p>
                <div class="flex items-center justify-between text-xs text-gray-500 dark:text-gray-400">
                    <span>ID: {{ repo.id.substring(0, 8) }}...</span>
                </div>
            </div>
        </div>

        <!-- Deposit Modal -->
        <div v-if="showDepositModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
            <div class="bg-white dark:bg-gray-800 rounded-lg max-w-md w-full p-6 shadow-xl">
                <div class="flex justify-between items-center mb-4">
                    <h3 class="text-xl font-bold text-gray-900 dark:text-white">Deposit Repository</h3>
                    <button @click="showDepositModal = false" class="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200">
                        <i class="pi pi-times"></i>
                    </button>
                </div>

                <form @submit.prevent="depositRepository" class="space-y-4">
                    <div>
                        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">GitHub Repository ID / Name</label>
                        <input
                            v-model="depositForm.githubRepoId"
                            type="text"
                            required
                            placeholder="e.g., owner/repo"
                            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                        />
                    </div>

                    <div>
                        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Description</label>
                        <textarea
                            v-model="depositForm.description"
                            rows="3"
                            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                            placeholder="Brief description of the repository..."
                        ></textarea>
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
                            <i v-if="depositing" class="pi pi-spin pi-spinner"></i>
                            Deposit
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>
