<template>
    <div class="mt-8">
        <div class="flex justify-end mb-4">
            <button
                @click="router.push('/manage')"
                class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors flex items-center gap-2"
            >
                <i class="pi pi-cog"></i>
                Manage Repositories
            </button>
        </div>
        <TabView>
            <TabPanel value="0" header="All Repository">
                <div
                    class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
                >
                    <div
                        v-for="repo in allRepos"
                        :key="repo.id"
                        class="p-6 bg-white dark:bg-gray-800 rounded-lg shadow-md border border-gray-100 dark:border-gray-700 flex flex-col cursor-pointer hover:shadow-lg transition-shadow"
                        @click="goToRepo(repo)"
                    >
                        <div class="flex justify-between items-start mb-4">
                            <h3
                                class="text-lg font-semibold text-gray-900 dark:text-white truncate"
                                :title="repo.githubRepoId || repo.gitHubRepoId"
                            >
                                {{ repo.githubRepoId || repo.gitHubRepoId }}
                            </h3>
                        </div>
                        <p
                            class="text-gray-600 dark:text-gray-300 text-sm mb-4 h-10 overflow-hidden text-ellipsis line-clamp-2 flex-grow"
                        >
                            {{ repo.description || "No description provided." }}
                        </p>
                        <div
                            class="flex items-center justify-between text-xs text-gray-500 dark:text-gray-400 mt-auto pt-4 border-t border-gray-100 dark:border-gray-700"
                        >
                            <span
                                >Owner:
                                {{ repo.ownerId.substring(0, 8) }}...</span
                            >
                            <span
                                >Auditors:
                                {{ repo.auditors?.length || 0 }}</span
                            >
                        </div>
                    </div>
                </div>
                <div
                    v-if="!loading && !error && allRepos.length === 0"
                    class="py-12 text-center text-gray-500 dark:text-gray-400"
                >
                    <i class="pi pi-inbox text-4xl mb-3 block"></i>
                    <p class="text-lg mb-4">No repositories found.</p>
                    <button
                        @click="router.push('/manage')"
                        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors inline-flex items-center gap-2"
                    >
                        <i class="pi pi-plus"></i>
                        Deposit Repository
                    </button>
                </div>
                <div v-if="loading" class="flex justify-center py-8">
                    <i class="pi pi-spin pi-spinner text-3xl text-blue-600"></i>
                </div>
                <div
                    v-if="error"
                    class="p-4 bg-red-100 text-red-700 rounded-md"
                >
                    {{ error }}
                </div>
            </TabPanel>
        </TabView>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import { useRouter } from "vue-router";
import TabView from "primevue/tabview";
import TabPanel from "primevue/tabpanel";

const props = defineProps<{
    user: any;
}>();

const router = useRouter();

const allRepos = ref<any[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const fetchAllRepos = async () => {
    loading.value = true;
    error.value = null;
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = { Accept: "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(`/api/repositories`, { headers });
        if (response.ok) {
            allRepos.value = await response.json();
            await fetchAuditorsForAllRepos();
        } else if (response.status === 404) {
            allRepos.value = [];
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
    const token = localStorage.getItem("token");
    const headers: HeadersInit = { Accept: "application/json" };
    if (token) headers["Authorization"] = `Bearer ${token}`;

    const promises = allRepos.value.map(async (repo) => {
        try {
            const response = await fetch(
                `/api/repositories/${repo.id}/auditors`,
                { headers },
            );
            if (response.ok) {
                repo.auditors = await response.json();
            } else {
                repo.auditors = [];
            }
        } catch (err) {
            console.error(`Failed to fetch auditors for repo ${repo.id}`, err);
            repo.auditors = [];
        }
    });

    await Promise.all(promises);
};

const goToRepo = (repo: any) => {
    router.push({ path: `/repository/${repo.id}`, state: { repo } });
};

onMounted(() => {
    fetchAllRepos();
});
</script>
