<script setup lang="ts">
import { ref, watch } from "vue";
import { IssueStatus, type Issue, type Repository } from "../types";

const props = defineProps<{
    show: boolean;
    repo: Repository;
    editingIssue: Issue | null;
    saving: boolean;
}>();

const emit = defineEmits<{
    close: [];
    save: [form: { title: string; body: string; status: IssueStatus }];
}>();

const issueForm = ref<{
    title: string;
    body: string;
    status: IssueStatus;
}>({
    title: "",
    body: "",
    status: IssueStatus.Draft,
});

watch(
    () => props.editingIssue,
    (issue) => {
        if (issue) {
            issueForm.value = {
                title: issue.title,
                body: issue.body,
                status:
                    issue.status === IssueStatus.Rejected
                        ? IssueStatus.Draft
                        : issue.status,
            };
        } else {
            issueForm.value = {
                title: "",
                body: "",
                status: IssueStatus.Draft,
            };
        }
    },
    { immediate: true },
);

const handleSave = () => {
    if (!issueForm.value.title) return;
    emit("save", { ...issueForm.value });
};
</script>

<template>
    <transition name="modal">
        <div
            v-if="show"
            class="fixed inset-0 bg-gray-900/60 backdrop-blur-sm flex items-center justify-center z-[100] p-4"
            @click.self="emit('close')"
        >
            <div
                class="bg-white dark:bg-gray-800 rounded-3xl max-w-4xl w-full max-h-[95vh] flex flex-col shadow-2xl border border-gray-100 dark:border-gray-700"
            >
                <div class="p-8 border-b border-gray-100 dark:border-gray-700 flex justify-between items-center">
                    <h3 class="text-2xl font-extrabold text-gray-900 dark:text-white">
                        {{ editingIssue ? "Edit Issue" : "Create New Issue" }}
                    </h3>
                    <button
                        @click="emit('close')"
                        class="p-2 text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 rounded-xl transition-all"
                    >
                        <i class="pi pi-times text-xl"></i>
                    </button>
                </div>

                <div class="p-8 space-y-6 flex-grow overflow-y-auto custom-scrollbar">
                    <div>
                        <label class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2">Title</label>
                        <input
                            v-model="issueForm.title"
                            type="text"
                            class="w-full px-4 py-3 border border-gray-200 dark:border-gray-700 rounded-xl dark:bg-gray-900 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all"
                            placeholder="What needs to be reviewed?"
                        />
                    </div>

                    <div>
                        <label class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2">Description</label>
                        <textarea
                            v-model="issueForm.body"
                            rows="10"
                            class="w-full px-4 py-3 border border-gray-200 dark:border-gray-700 rounded-xl dark:bg-gray-900 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all resize-none"
                            placeholder="Describe the changes in detail..."
                        ></textarea>
                    </div>

                    <div>
                        <label class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2">Status</label>
                        <div class="relative">
                            <select
                                v-model="issueForm.status"
                                class="w-full px-4 py-3 border border-gray-200 dark:border-gray-700 rounded-xl dark:bg-gray-900 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all appearance-none"
                            >
                                <option :value="IssueStatus.Draft">Draft</option>
                                <option :value="IssueStatus.SubmitForReview">Submit for Review</option>
                            </select>
                            <i class="pi pi-chevron-down absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"></i>
                        </div>
                    </div>
                </div>

                <div class="p-8 border-t border-gray-100 dark:border-gray-700 flex justify-end gap-3">
                    <button
                        @click="emit('close')"
                        class="px-6 py-3 text-gray-600 font-bold bg-gray-100 hover:bg-gray-200 rounded-2xl dark:bg-gray-700 dark:text-gray-300 dark:hover:bg-gray-600 transition-all"
                        :disabled="saving"
                    >
                        Cancel
                    </button>
                    <button
                        @click="handleSave"
                        class="px-8 py-3 bg-blue-600 text-white font-bold rounded-2xl hover:bg-blue-700 shadow-md shadow-blue-200 dark:shadow-none transition-all flex items-center gap-2"
                        :disabled="saving || !issueForm.title"
                    >
                        <i v-if="saving" class="pi pi-spin pi-spinner"></i>
                        {{ saving ? "Saving..." : editingIssue ? "Update Issue" : "Create Issue" }}
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

.custom-scrollbar::-webkit-scrollbar {
    width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
    background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
    background: #e2e8f0;
    border-radius: 10px;
}
.dark .custom-scrollbar::-webkit-scrollbar-thumb {
    background: #334155;
}
</style>
