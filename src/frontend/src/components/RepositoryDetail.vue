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
                <h1
                    class="text-3xl font-bold text-gray-900 dark:text-white mb-2"
                >
                    {{ repo.githubRepoId || repo.gitHubRepoId }}
                </h1>
                <p class="text-gray-600 dark:text-gray-300">
                    {{ repo.description || "No description provided." }}
                </p>
            </div>

            <TabView value="0">
                <TabPanel value="0" header="Issues">
                    <RepositoryIssues :repo="repo" :user="props.user" />
                </TabPanel>
                <TabPanel value="1" header="Auditors">
                    <RepositoryAuditors :repo="repo" :user="props.user" />
                </TabPanel>
            </TabView>
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
import TabView from "primevue/tabview";
import TabPanel from "primevue/tabpanel";
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
                throw new Error("Repository not found");
            }
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
