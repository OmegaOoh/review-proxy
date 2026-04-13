<script setup lang="ts">
import { IssueStatus, type Issue, type User } from "../types";

const props = defineProps<{
    issue: Issue;
    user: User;
    isAuditor: boolean;
}>();

const emit = defineEmits<{
    view: [issue: Issue];
    edit: [issue: Issue];
    delete: [id: string];
    approve: [id: string];
    reject: [id: string];
}>();

const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString(undefined, {
        month: "short",
        day: "numeric",
        year: "numeric",
    });
};

const formatStatus = (status: IssueStatus) => {
    if (status === IssueStatus.SubmitForReview) return "Review Requested";
    return status;
};

const getStatusClass = (status: IssueStatus) => {
    switch (status) {
        case IssueStatus.Draft:
            return "bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-400";
        case IssueStatus.SubmitForReview:
            return "bg-blue-100 text-blue-600 dark:bg-blue-900/40 dark:text-blue-400";
        case IssueStatus.Approved:
            return "bg-green-100 text-green-600 dark:bg-green-900/40 dark:text-green-400";
        case IssueStatus.Rejected:
            return "bg-red-100 text-red-600 dark:bg-red-900/40 dark:text-red-400";
        default:
            return "bg-gray-100 text-gray-600";
    }
};
</script>

<template>
    <div
        class="group p-5 bg-white dark:bg-gray-800 rounded-2xl border border-gray-100 dark:border-gray-700 hover:shadow-md transition-all duration-200 flex flex-col sm:flex-row justify-between gap-6"
    >
        <div class="flex-grow cursor-pointer" @click="emit('view', issue)">
            <div class="flex items-center gap-3 mb-3">
                <span
                    class="px-2.5 py-1 text-[11px] font-bold uppercase tracking-wider rounded-lg shadow-sm"
                    :class="getStatusClass(issue.status)"
                >
                    {{ formatStatus(issue.status) }}
                </span>
                <h3
                    class="text-lg font-bold text-gray-900 dark:text-white group-hover:text-blue-400 transition-colors"
                >
                    {{ issue.title }}
                </h3>
            </div>
            <p
                class="text-gray-600 dark:text-gray-400 mb-4 line-clamp-2 transition-colors text-sm"
            >
                {{ issue.body || "No description provided." }}
            </p>
            <div
                class="flex items-center gap-6 text-[11px] font-bold text-gray-400 uppercase tracking-widest"
            >
                <div class="flex items-center gap-1.5">
                    <img
                        v-if="issue.owner"
                        :src="
                            issue.owner.gitHubAvatarUrl ||
                            `https://github.com/${issue.owner.gitHubUsername}.png`
                        "
                        class="w-4 h-4 rounded-full border border-gray-100 dark:border-gray-700 shadow-sm"
                    />
                    <i v-else class="pi pi-user text-[10px]"></i>
                    <span>{{
                        issue.owner?.gitHubUsername ||
                        issue.ownerId.substring(0, 8)
                    }}</span>
                </div>
                <div class="flex items-center gap-1.5">
                    <i class="pi pi-calendar text-[10px]"></i>
                    <span>{{ formatDate(issue.createdAt) }}</span>
                </div>
            </div>
        </div>

        <div class="flex sm:flex-col justify-end gap-2 shrink-0">
            <template v-if="issue.ownerId === user?.id">
                <button
                    v-if="issue.status !== IssueStatus.Approved"
                    @click="emit('edit', issue)"
                    class="px-4 py-2 text-sm font-bold bg-blue-50 text-blue-600 dark:bg-blue-900/20 dark:text-blue-400 rounded-xl hover:bg-blue-100 dark:hover:bg-blue-900/40 transition-colors flex items-center justify-center gap-2"
                >
                    <i class="pi pi-pencil"></i> Edit
                </button>
                <button
                    v-if="issue.status !== IssueStatus.Approved"
                    @click="emit('delete', issue.id)"
                    class="px-4 py-2 text-sm font-bold bg-red-50 text-red-600 dark:bg-red-900/20 dark:text-red-400 rounded-xl hover:bg-red-100 dark:hover:bg-red-900/40 transition-colors flex items-center justify-center gap-2"
                >
                    <i class="pi pi-trash"></i> Delete
                </button>
            </template>
            <template
                v-if="
                    isAuditor &&
                    issue.status === IssueStatus.SubmitForReview &&
                    issue.ownerId !== user?.id
                "
            >
                <button
                    @click="emit('approve', issue.id)"
                    class="px-4 py-2 text-sm font-bold bg-green-600 text-white rounded-xl hover:bg-green-700 transition-colors flex items-center justify-center gap-2"
                >
                    <i class="pi pi-check"></i> Approve
                </button>
                <button
                    @click="emit('reject', issue.id)"
                    class="px-4 py-2 text-sm font-bold bg-red-600 text-white rounded-xl hover:bg-red-700 transition-colors flex items-center justify-center gap-2"
                >
                    <i class="pi pi-times"></i> Reject
                </button>
            </template>
        </div>
    </div>
</template>
