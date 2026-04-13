<script setup lang="ts">
import { ref, watch } from "vue";
import { IdentityService } from "../api/identities";
import type { User } from "../types";

const props = defineProps<{
    user: User;
}>();

const selectedAuditors = defineModel<User[]>({ default: [] });

const userSearchQuery = ref("");
const searchResults = ref<User[]>([]);
const isSearching = ref(false);
let searchTimeout: any = null;

const handleSearchUsers = async (query: string) => {
    if (!query) {
        searchResults.value = [];
        return;
    }
    isSearching.value = true;
    try {
        const users = await IdentityService.searchUsers(query);
        // Filter out current user and already selected auditors
        searchResults.value = users.filter(
            (u) =>
                u.id !== props.user.id &&
                !selectedAuditors.value.some((a) => a.id === u.id),
        );
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

const addAuditor = (user: User) => {
    selectedAuditors.value.push(user);
    userSearchQuery.value = "";
    searchResults.value = [];
};

const removeAuditor = (id: string) => {
    selectedAuditors.value = selectedAuditors.value.filter((u) => u.id !== id);
};
</script>

<template>
    <div class="relative">
        <label class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2">
            Initial Auditors
        </label>
        <div class="relative mb-3">
            <i class="pi pi-search absolute left-4 top-1/2 -translate-y-1/2 text-gray-400"></i>
            <input
                type="text"
                v-model="userSearchQuery"
                placeholder="Search by username..."
                class="w-full pl-11 pr-4 py-3 bg-gray-50 dark:bg-gray-900 border border-gray-200 dark:border-gray-700 rounded-2xl text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all"
            />
        </div>

        <div v-if="userSearchQuery && (isSearching || searchResults.length > 0)" class="absolute z-[110] mt-1 w-full bg-white dark:bg-gray-800 shadow-2xl rounded-2xl border border-gray-100 dark:border-gray-700 max-h-48 overflow-auto p-2">
            <div v-if="isSearching" class="p-4 text-center text-sm text-gray-500">
                <i class="pi pi-spin pi-spinner mr-2"></i>Searching...
            </div>
            <div
                v-for="user in searchResults"
                :key="user.id"
                @click="addAuditor(user)"
                class="p-3 hover:bg-gray-50 dark:hover:bg-gray-700/50 cursor-pointer rounded-xl flex items-center justify-between group transition-colors"
            >
                <div class="flex items-center gap-3">
                    <img :src="user.gitHubAvatarUrl" class="w-8 h-8 rounded-full" />
                    <span class="font-bold text-gray-900 dark:text-white text-sm">{{ user.gitHubUsername }}</span>
                </div>
                <i class="pi pi-plus text-blue-500 opacity-0 group-hover:opacity-100 transition-opacity"></i>
            </div>
        </div>

        <div class="flex flex-wrap gap-2">
            <span
                v-for="auditor in selectedAuditors"
                :key="auditor.id"
                class="inline-flex items-center gap-2 pl-2 pr-1.5 py-1.5 bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400 text-xs font-bold rounded-xl border border-blue-100 dark:border-blue-800/50"
            >
                <img :src="auditor.gitHubAvatarUrl" class="w-4 h-4 rounded-full" />
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
</template>
