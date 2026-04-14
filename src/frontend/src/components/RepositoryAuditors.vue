<template>
    <div class="space-y-8">
        <div>
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                Auditors
            </h2>
            <p class="text-sm text-gray-600 dark:text-gray-400">
                Users authorized to review and approve issues in this
                repository.
            </p>
        </div>

        <div v-if="loading" class="flex items-center justify-center py-12">
            <i class="pi pi-spin pi-spinner text-3xl text-blue-600"></i>
        </div>

        <div
            v-else-if="error"
            class="p-4 bg-red-50 text-red-700 rounded-xl border border-red-200"
        >
            {{ error }}
        </div>

        <div v-else class="max-w-3xl">
            <!-- Add Auditor (Owner Only) -->
            <div
                v-if="props.user.id === props.repo.ownerId"
                class="relative mb-8"
            >
                <label
                    class="block text-xs font-bold uppercase tracking-widest text-gray-600 dark:text-gray-400 mb-3"
                    >Add New Auditor</label
                >
                <div class="relative">
                    <i
                        class="pi pi-search absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 dark:text-gray-400"
                    ></i>
                    <input
                        type="text"
                        v-model="userSearchQuery"
                        placeholder="Search by GitHub username..."
                        class="w-full pl-11 pr-4 py-3 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-2xl shadow-sm focus:ring-2 focus:ring-blue-500 outline-none transition-all"
                    />
                </div>

                <!-- Search Results Dropdown -->
                <transition name="fade">
                    <div
                        v-if="
                            userSearchQuery &&
                            (isSearching || searchResults.length > 0)
                        "
                        class="absolute z-50 mt-2 w-full bg-white dark:bg-gray-800 shadow-xl rounded-2xl border border-gray-200 dark:border-gray-700 max-h-64 overflow-auto p-2"
                    >
                        <div
                            v-if="isSearching"
                            class="p-4 text-sm text-gray-600 dark:text-gray-400 text-center flex items-center justify-center gap-2"
                        >
                            <i class="pi pi-spin pi-spinner"></i>
                            Searching...
                        </div>
                        <div v-else>
                            <div
                                v-for="user in searchResults"
                                :key="user.id"
                                @click="handleAddAuditor(user)"
                                class="p-3 hover:bg-gray-100 dark:hover:bg-gray-700/50 cursor-pointer rounded-xl flex items-center justify-between group transition-colors"
                            >
                                <div class="flex items-center gap-3">
                                    <img
                                        :src="
                                            user.gitHubAvatarUrl ||
                                            `https://github.com/${user.gitHubUsername}.png`
                                        "
                                        class="w-8 h-8 rounded-full border border-gray-200 dark:border-gray-600"
                                    />
                                    <span
                                        class="font-bold text-gray-900 dark:text-white"
                                        >{{ user.gitHubUsername }}</span
                                    >
                                </div>
                                <i
                                    class="pi pi-plus text-blue-500 opacity-0 group-hover:opacity-100 transition-opacity"
                                ></i>
                            </div>
                        </div>
                    </div>
                </transition>
            </div>

            <div
                class="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 overflow-hidden shadow-sm"
            >
                <!-- Owner Section -->
                <div
                    class="p-4 bg-gray-100/50 dark:bg-gray-900/20 border-b border-gray-200 dark:border-gray-700"
                >
                    <div class="flex items-center justify-between">
                        <div class="flex items-center gap-3">
                            <div class="relative">
                                <img
                                    :src="
                                        ownerDetails?.gitHubAvatarUrl ||
                                        `https://github.com/${ownerDetails?.gitHubUsername || 'github'}.png`
                                    "
                                    class="w-10 h-10 rounded-full border-2 border-blue-500 p-0.5"
                                />
                                <span
                                    class="absolute -bottom-1 -right-1 bg-blue-500 text-white text-[8px] font-black uppercase px-1 rounded border border-white"
                                    >Owner</span
                                >
                            </div>
                            <div>
                                <div
                                    class="font-bold text-gray-900 dark:text-white"
                                >
                                    {{
                                        ownerDetails?.gitHubUsername ||
                                        "Loading owner..."
                                    }}
                                </div>
                                <div
                                    class="text-[10px] text-gray-500 dark:text-gray-400 font-bold uppercase tracking-widest"
                                >
                                    Repository Creator
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Auditors List -->
                <div class="divide-y divide-gray-100 dark:divide-gray-700/50">
                    <div
                        v-if="!props.repo.auditors?.length"
                        class="p-8 text-center text-gray-500 dark:text-gray-400 text-sm italic"
                    >
                        No additional auditors have been assigned.
                    </div>

                    <div
                        v-for="auditor in props.repo.auditors"
                        :key="auditor.id"
                        class="p-4 flex items-center justify-between hover:bg-gray-100/30 dark:hover:bg-gray-700/10 transition-colors"
                    >
                        <div class="flex items-center gap-3">
                            <img
                                :src="
                                    auditor.gitHubAvatarUrl ||
                                    `https://github.com/${auditor.gitHubUsername}.png`
                                "
                                class="w-10 h-10 rounded-full border border-gray-200 dark:border-gray-700"
                            />
                            <div>
                                <div
                                    class="font-bold text-gray-900 dark:text-white"
                                >
                                    {{ auditor.gitHubUsername }}
                                </div>
                                <div
                                    class="text-[10px] text-gray-500 dark:text-gray-400 font-bold uppercase tracking-widest"
                                >
                                    Authorized Reviewer
                                </div>
                            </div>
                        </div>
                        <button
                            v-if="props.user.id === props.repo.ownerId"
                            @click="handleRemoveAuditor(auditor.id)"
                            class="p-2 text-gray-500 dark:text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-xl transition-all"
                            title="Remove Auditor"
                        >
                            <i class="pi pi-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import { useRepoStore } from "../stores/repositories";
import { IdentityService } from "../api/identities";
import { useConfirm } from "primevue/useconfirm";
import type { Repository, User } from "../types";

const props = defineProps<{
    repo: Repository;
    user: User;
}>();

const repoStore = useRepoStore();
const confirm = useConfirm();
const userSearchQuery = ref("");
// ...
const searchResults = ref<User[]>([]);
const isSearching = ref(false);
const ownerDetails = ref<User | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);
let searchTimeout: any = null;

const fetchOwnerDetails = async () => {
    try {
        ownerDetails.value = await IdentityService.getUser(props.repo.ownerId);
    } catch (err) {
        console.error("Failed to fetch owner details", err);
    }
};

const handleSearchUsers = async (query: string) => {
    if (!query || query.length < 2) {
        searchResults.value = [];
        return;
    }
    isSearching.value = true;
    try {
        const users = await IdentityService.searchUsers(query);
        // Filter out existing auditors and owner
        searchResults.value = users.filter(
            (u) =>
                u.id !== props.repo.ownerId &&
                !props.repo.auditors?.some((a) => a.id === u.id),
        );
    } catch (err) {
        console.error("User search failed", err);
    } finally {
        isSearching.value = false;
    }
};

watch(userSearchQuery, (newVal) => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        handleSearchUsers(newVal);
    }, 400);
});

const handleAddAuditor = async (user: User) => {
    try {
        await repoStore.addAuditors(props.repo.id, [user.id]);
        userSearchQuery.value = "";
        searchResults.value = [];
    } catch (err) {
        console.error("Failed to add auditor", err);
    }
};

const handleRemoveAuditor = async (userId: string) => {
    confirm.require({
        message: "Are you sure you want to remove this auditor?",
        header: "Remove Auditor",
        icon: "pi pi-exclamation-triangle",
        rejectProps: {
            label: "Cancel",
            severity: "secondary",
            outlined: true,
        },
        acceptProps: {
            label: "Remove",
            severity: "danger",
        },
        accept: async () => {
            try {
                await repoStore.removeAuditor(props.repo.id, userId);
            } catch (err) {
                console.error("Failed to remove auditor", err);
            }
        },
    });
};

onMounted(() => {
    fetchOwnerDetails();
});
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
    transition: all 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
    transform: translateY(-10px);
}
</style>
