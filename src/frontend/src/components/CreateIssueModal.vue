<script setup lang="ts">
import { ref } from "vue";
import { useIssueStore } from "../stores/issues";
import { IssueStatus, type Repository, type User } from "../types";

const props = defineProps<{
    repo: Repository | null;
    user: User;
    show: boolean;
}>();

const emit = defineEmits<{
    close: [];
    created: [issue: any];
}>();

const issueStore = useIssueStore();

const issueForm = ref<{
    title: string;
    body: string;
    status: IssueStatus;
}>({
    title: "",
    body: "",
    status: IssueStatus.Draft,
});

const creating = ref(false);

const closeModal = () => {
    emit("close");
};

const handleCreateIssue = async () => {
    if (!issueForm.value.title || !props.repo) {
        return;
    }

    creating.value = true;
    try {
        const newIssue = await issueStore.createIssue({
            title: issueForm.value.title,
            body: issueForm.value.body,
            repositoryId: props.repo.id,
            status: issueForm.value.status,
        });

        emit("created", newIssue);
        issueForm.value = {
            title: "",
            body: "",
            status: IssueStatus.Draft,
        };
    } catch (err) {
        console.error("Failed to create issue", err);
        alert("An error occurred while creating the issue.");
    } finally {
        creating.value = false;
    }
};
</script>

<template>
    <transition name="modal">
        <div
            v-if="show"
            class="fixed inset-0 bg-gray-900/60 backdrop-blur-sm flex items-center justify-center p-4 z-[100]"
            @click.self="closeModal"
        >
            <div
                class="bg-white dark:bg-gray-800 rounded-[2.5rem] w-full max-w-4xl h-full max-h-[90vh] p-8 shadow-2xl flex flex-col overflow-hidden border border-gray-100 dark:border-gray-700"
            >
                <div class="flex justify-between items-center mb-8">
                    <div>
                        <h3
                            class="text-2xl font-black text-gray-900 dark:text-white tracking-tight"
                        >
                            Create Issue
                        </h3>
                        <p
                            class="text-sm text-gray-500 font-bold uppercase tracking-widest mt-1"
                        >
                            For {{ repo?.gitHubRepoId }}
                        </p>
                    </div>
                    <button
                        @click="closeModal"
                        class="p-2 text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 rounded-2xl transition-all"
                    >
                        <i class="pi pi-times text-xl"></i>
                    </button>
                </div>

                <div class="flex flex-col gap-6 flex-grow overflow-y-auto pr-2">
                    <div>
                        <label
                            class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2"
                            >Title</label
                        >
                        <input
                            v-model="issueForm.title"
                            type="text"
                            class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-900 border border-gray-200 dark:border-gray-700 rounded-2xl text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all"
                            placeholder="Briefly describe the issue..."
                        />
                    </div>

                    <div class="flex-grow flex flex-col min-h-[300px]">
                        <label
                            class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2"
                            >Detailed Description</label
                        >
                        <textarea
                            v-model="issueForm.body"
                            class="w-full flex-grow px-4 py-3 bg-gray-50 dark:bg-gray-900 border border-gray-200 dark:border-gray-700 rounded-3xl text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all resize-none"
                            placeholder="Provide full details for the reviewers..."
                        ></textarea>
                    </div>

                    <div>
                        <label
                            class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2"
                            >Status</label
                        >
                        <div class="relative">
                            <select
                                v-model="issueForm.status"
                                class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-900 border border-gray-200 dark:border-gray-700 rounded-2xl text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all appearance-none"
                            >
                                <option :value="IssueStatus.Draft">
                                    Draft
                                </option>
                                <option :value="IssueStatus.SubmitForReview">
                                    Submit for Review
                                </option>
                            </select>
                            <i
                                class="pi pi-chevron-down absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"
                            ></i>
                        </div>
                    </div>
                </div>

                <div
                    class="mt-8 pt-8 border-t border-gray-100 dark:border-gray-700 flex justify-end gap-3"
                >
                    <button
                        @click="closeModal"
                        class="px-6 py-3 text-gray-600 font-bold bg-gray-100 hover:bg-gray-200 rounded-2xl transition-all"
                        :disabled="creating"
                    >
                        Cancel
                    </button>
                    <button
                        @click="handleCreateIssue"
                        class="px-10 py-3 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-2xl shadow-lg shadow-blue-200 dark:shadow-none transition-all flex items-center gap-2"
                        :disabled="creating || !issueForm.title"
                    >
                        <i v-if="creating" class="pi pi-spin pi-spinner"></i>
                        {{ creating ? "Creating..." : "Save Issue" }}
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
    transform: scale(0.95) translateY(20px);
}
</style>
