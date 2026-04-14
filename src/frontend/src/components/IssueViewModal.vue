<script setup lang="ts">
import { IssueStatus, type Issue } from "../types";

const props = defineProps<{
    show: boolean;
    issue: Issue | null;
}>();

const emit = defineEmits<{
    close: [];
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
            return "bg-gray-200 text-gray-600 dark:bg-gray-700 dark:text-gray-400";
        case IssueStatus.SubmitForReview:
            return "bg-blue-100 text-blue-600 dark:bg-blue-900/40 dark:text-blue-400";
        case IssueStatus.Approved:
            return "bg-green-100 text-green-600 dark:bg-green-900/40 dark:text-green-400";
        case IssueStatus.Rejected:
            return "bg-red-100 text-red-600 dark:bg-red-900/40 dark:text-red-400";
        default:
            return "bg-gray-200 text-gray-600";
    }
};
</script>

<template>
    <transition name="modal">
        <div
            v-if="show && issue"
            class="fixed inset-0 bg-gray-900/60 backdrop-blur-sm flex items-center justify-center z-[100] p-4"
            @click.self="emit('close')"
        >
            <div
                class="bg-white dark:bg-gray-800 rounded-3xl max-w-4xl w-full max-h-[90vh] flex flex-col shadow-2xl overflow-hidden border border-gray-200 dark:border-gray-700"
            >
                <div
                    class="p-8 border-b border-gray-200 dark:border-gray-700 flex justify-between items-start"
                >
                    <div>
                        <div class="flex items-center gap-3 mb-4">
                            <span
                                class="px-2.5 py-1 text-[11px] font-bold uppercase tracking-wider rounded-lg"
                                :class="getStatusClass(issue.status)"
                            >
                                {{ formatStatus(issue.status) }}
                            </span>
                            <span
                                class="text-sm text-gray-600 dark:text-gray-400"
                            >
                                Created by
                                {{
                                    issue.owner?.gitHubUsername ||
                                    issue.ownerId.substring(0, 8)
                                }}
                                on {{ formatDate(issue.createdAt) }}
                            </span>
                        </div>
                        <h3
                            class="text-3xl font-extrabold text-gray-900 dark:text-white leading-tight"
                        >
                            {{ issue.title }}
                        </h3>
                    </div>
                    <button
                        @click="emit('close')"
                        class="p-2 text-gray-500 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 rounded-xl transition-all"
                    >
                        <i class="pi pi-times text-xl"></i>
                    </button>
                </div>

                <div
                    class="flex-grow overflow-y-auto p-8 bg-gray-100/30 dark:bg-gray-900/10"
                >
                    <div class="prose dark:prose-invert max-w-none">
                        <p
                            class="text-lg text-gray-800 dark:text-gray-200 whitespace-pre-line leading-relaxed"
                        >
                            {{ issue.body || "No description provided." }}
                        </p>
                    </div>
                </div>

                <div
                    class="p-6 border-t border-gray-200 dark:border-gray-700 flex justify-end"
                >
                    <button
                        @click="emit('close')"
                        class="px-8 py-3 bg-gray-900 text-white dark:bg-white dark:text-gray-900 rounded-2xl font-bold hover:opacity-90 transition-all"
                    >
                        Close
                    </button>
                </div>
            </div>
        </div>
    </transition>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
    transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.modal-enter-from,
.modal-leave-to {
    opacity: 0;
    transform: scale(0.95);
}
</style>
