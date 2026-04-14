<template>
    <div class="space-y-8">
        <div
            class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4"
        >
            <div>
                <button
                    @click="router.back()"
                    class="mb-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition-colors flex items-center gap-1"
                >
                    <i class="pi pi-arrow-left text-[10px]"></i>
                    Back
                </button>
                <h2
                    class="text-3xl font-extrabold text-gray-900 dark:text-white tracking-tight"
                >
                    My Repositories
                </h2>
                <p class="text-gray-600 dark:text-gray-400">
                    Deposit and configure your GitHub repositories.
                </p>
            </div>
            <button
                @click="showDepositModal = true"
                class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-2xl shadow-lg shadow-blue-200 dark:shadow-none transition-all flex items-center gap-2"
            >
                <i class="pi pi-plus"></i>
                Deposit Repository
            </button>
        </div>

        <div
            v-if="error"
            class="p-4 bg-red-50 text-red-700 rounded-2xl border border-red-200 flex items-center gap-3"
        >
            <i class="pi pi-exclamation-circle text-xl"></i>
            {{ error }}
        </div>

        <div
            v-if="loading"
            class="flex flex-col items-center justify-center py-20"
        >
            <i class="pi pi-spin pi-spinner text-4xl text-blue-600 mb-4"></i>
            <span class="text-gray-600 dark:text-gray-400"
                >Loading your repositories...</span
            >
        </div>

        <div
            v-else-if="myRepositories.length === 0"
            class="py-20 text-center bg-white dark:bg-gray-800 rounded-3xl shadow-sm border border-gray-200 dark:border-gray-700"
        >
            <div
                class="w-20 h-20 bg-gray-100 dark:bg-gray-700 rounded-full flex items-center justify-center mx-auto mb-4"
            >
                <i class="pi pi-folder-open text-4xl text-gray-300"></i>
            </div>
            <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-2">
                No active deposits
            </h3>
            <p class="text-gray-600 dark:text-gray-400 mb-8">
                You haven't deposited any repositories to Review Proxy yet.
            </p>
            <button
                @click="showDepositModal = true"
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
                class="group p-6 bg-white dark:bg-gray-800 rounded-3xl shadow-sm border border-gray-200 dark:border-gray-700 flex flex-col hover:border-blue-200 dark:hover:border-blue-900/50 transition-all duration-300"
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
                        class="p-2 text-gray-500 dark:text-gray-400 hover:text-blue-500 transition-colors"
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
                    class="mt-auto pt-6 border-t border-gray-100 dark:border-gray-700 flex flex-wrap gap-2"
                >
                    <button
                        @click="router.push(`/repository/${repo.id}`)"
                        class="flex-1 px-3 py-2 text-xs font-bold bg-blue-50 text-blue-600 dark:bg-blue-900/20 dark:text-blue-400 rounded-xl hover:bg-blue-100 transition-colors flex items-center justify-center gap-2"
                    >
                        <i class="pi pi-cog"></i> Config / Detail
                    </button>
                    <button
                        @click="handleOptOut(repo)"
                        class="flex-1 px-3 py-2 text-xs font-bold bg-red-50 text-red-600 dark:bg-red-900/20 dark:text-red-400 rounded-xl hover:bg-red-100 transition-colors flex items-center justify-center gap-2"
                    >
                        <i class="pi pi-sign-out"></i> Opt-out
                    </button>
                </div>
            </div>
        </div>

        <DepositRepositoryModal
            :show="showDepositModal"
            :user="props.user"
            @close="showDepositModal = false"
            @deposited="onRepoDeposited"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import { useRouter } from "vue-router";
import { storeToRefs } from "pinia";
import { useRepoStore } from "../stores/repositories";
import { useConfirm } from "primevue/useconfirm";
import DepositRepositoryModal from "./DepositRepositoryModal.vue";
import type { User, Repository } from "../types";

const router = useRouter();
const repoStore = useRepoStore();
const confirm = useConfirm();
const { repositories, loading, error } = storeToRefs(repoStore);

const props = defineProps<{
    user: User;
}>();

const myRepositories = computed(() =>
    repositories.value.filter((r) => r.ownerId === props.user.id),
);

const showDepositModal = ref(false);

const onRepoDeposited = () => {
    showDepositModal.value = false;
};

const handleOptOut = async (repo: Repository) => {
    confirm.require({
        message: `Are you sure you want to opt-out from ${repo.gitHubRepoId}? This will remove all Review Proxy configuration for this repository.`,
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
                await repoStore.deleteRepository(repo.id);
            } catch (err) {
                console.error("Failed to opt-out", err);
            }
        },
    });
};

onMounted(() => {
    repoStore.fetchAllRepositories();
});
</script>
