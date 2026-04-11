<template>
    <div class="space-y-6">
        <div
            class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4"
        >
            <div>
                <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
                    Issues
                </h2>
                <p class="text-sm text-gray-500">
                    Track and review code changes.
                </p>
            </div>
            <button
                @click="showCreateModal = true"
                class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-xl shadow-sm transition-all flex items-center gap-2"
            >
                <i class="pi pi-plus"></i> New Issue
            </button>
        </div>

        <div
            v-if="loading"
            class="flex flex-col items-center justify-center py-20"
        >
            <i class="pi pi-spin pi-spinner text-3xl text-blue-600 mb-4"></i>
            <span class="text-gray-500">Loading issues...</span>
        </div>

        <div
            v-else-if="error"
            class="p-4 bg-red-50 text-red-700 rounded-xl border border-red-100"
        >
            {{ error }}
        </div>

        <div
            v-else-if="filteredIssues.length === 0"
            class="text-center py-20 bg-gray-50 dark:bg-gray-800/50 rounded-2xl border-2 border-dashed border-gray-200 dark:border-gray-700"
        >
            <i class="pi pi-file-edit text-4xl text-gray-300 mb-4"></i>
            <p class="text-lg text-gray-500">
                No issues found for this repository.
            </p>
            <button
                @click="showCreateModal = true"
                class="mt-4 text-blue-600 font-semibold hover:underline"
            >
                Create your first issue
            </button>
        </div>

        <div v-else class="grid grid-cols-1 gap-4">
            <div
                v-for="issue in filteredIssues"
                :key="issue.id"
                class="group p-5 bg-white dark:bg-gray-800 rounded-2xl border border-gray-100 dark:border-gray-700 hover:shadow-md transition-all duration-200 flex flex-col sm:flex-row justify-between gap-6"
            >
                <div
                    class="flex-grow cursor-pointer"
                    @click="openViewModal(issue)"
                >
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
                    <template v-if="issue.ownerId === props.user?.id">
                        <button
                            v-if="issue.status !== IssueStatus.Approved"
                            @click="openEditModal(issue)"
                            class="px-4 py-2 text-sm font-bold bg-blue-50 text-blue-600 dark:bg-blue-900/20 dark:text-blue-400 rounded-xl hover:bg-blue-100 dark:hover:bg-blue-900/40 transition-colors flex items-center justify-center gap-2"
                        >
                            <i class="pi pi-pencil"></i> Edit
                        </button>
                        <button
                            v-if="issue.status !== IssueStatus.Approved"
                            @click="handleDeleteIssue(issue.id)"
                            class="px-4 py-2 text-sm font-bold bg-red-50 text-red-600 dark:bg-red-900/20 dark:text-red-400 rounded-xl hover:bg-red-100 dark:hover:bg-red-900/40 transition-colors flex items-center justify-center gap-2"
                        >
                            <i class="pi pi-trash"></i> Delete
                        </button>
                    </template>
                    <template
                        v-if="
                            isAuditor &&
                            issue.status === IssueStatus.SubmitForReview &&
                            issue.ownerId !== props.user?.id
                        "
                    >
                        <button
                            @click="issueStore.approveIssue(issue.id)"
                            class="px-4 py-2 text-sm font-bold bg-green-600 text-white rounded-xl hover:bg-green-700 transition-colors flex items-center justify-center gap-2"
                        >
                            <i class="pi pi-check"></i> Approve
                        </button>
                        <button
                            @click="issueStore.rejectIssue(issue.id)"
                            class="px-4 py-2 text-sm font-bold bg-red-600 text-white rounded-xl hover:bg-red-700 transition-colors flex items-center justify-center gap-2"
                        >
                            <i class="pi pi-times"></i> Reject
                        </button>
                    </template>
                </div>
            </div>
        </div>

        <!-- View Issue Modal -->
        <transition name="modal">
            <div
                v-if="showViewModal && viewingIssue"
                class="fixed inset-0 bg-gray-900/60 backdrop-blur-sm flex items-center justify-center z-[100] p-4"
                @click.self="closeModal"
            >
                <div
                    class="bg-white dark:bg-gray-800 rounded-3xl max-w-4xl w-full max-h-[90vh] flex flex-col shadow-2xl overflow-hidden border border-gray-100 dark:border-gray-700"
                >
                    <div
                        class="p-8 border-b border-gray-100 dark:border-gray-700 flex justify-between items-start"
                    >
                        <div>
                            <div class="flex items-center gap-3 mb-4">
                                <span
                                    class="px-2.5 py-1 text-[11px] font-bold uppercase tracking-wider rounded-lg"
                                    :class="getStatusClass(viewingIssue.status)"
                                >
                                    {{ formatStatus(viewingIssue.status) }}
                                </span>
                                <span class="text-sm text-gray-500"
                                    >Created by
                                    {{
                                        viewingIssue.owner?.gitHubUsername ||
                                        viewingIssue.ownerId.substring(0, 8)
                                    }}
                                    on
                                    {{
                                        formatDate(viewingIssue.createdAt)
                                    }}</span
                                >
                            </div>
                            <h3
                                class="text-3xl font-extrabold text-gray-900 dark:text-white leading-tight"
                            >
                                {{ viewingIssue.title }}
                            </h3>
                        </div>
                        <button
                            @click="closeModal"
                            class="p-2 text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 rounded-xl transition-all"
                        >
                            <i class="pi pi-times text-xl"></i>
                        </button>
                    </div>

                    <div
                        class="flex-grow overflow-y-auto p-8 bg-gray-50/30 dark:bg-gray-900/10"
                    >
                        <div class="prose dark:prose-invert max-w-none">
                            <p
                                class="text-lg text-gray-800 dark:text-gray-200 whitespace-pre-line leading-relaxed"
                            >
                                {{
                                    viewingIssue.body ||
                                    "No description provided."
                                }}
                            </p>
                        </div>
                    </div>

                    <div
                        class="p-6 border-t border-gray-100 dark:border-gray-700 flex justify-end"
                    >
                        <button
                            @click="closeModal"
                            class="px-8 py-3 bg-gray-900 text-white dark:bg-white dark:text-gray-900 rounded-2xl font-bold hover:opacity-90 transition-all"
                        >
                            Close
                        </button>
                    </div>
                </div>
            </div>
        </transition>

        <!-- Create/Edit Issue Modal -->
        <transition name="modal">
            <div
                v-if="showCreateModal"
                class="fixed inset-0 bg-gray-900/60 backdrop-blur-sm flex items-center justify-center z-[100] p-4"
                @click.self="closeModal"
            >
                <div
                    class="bg-white dark:bg-gray-800 rounded-3xl max-w-4xl w-full max-h-[95vh] flex flex-col shadow-2xl border border-gray-100 dark:border-gray-700"
                >
                    <div
                        class="p-8 border-b border-gray-100 dark:border-gray-700"
                    >
                        <h3
                            class="text-2xl font-extrabold text-gray-900 dark:text-white"
                        >
                            {{
                                editingIssue ? "Edit Issue" : "Create New Issue"
                            }}
                        </h3>
                    </div>

                    <div class="p-8 space-y-6 flex-grow overflow-y-auto">
                        <div>
                            <label
                                class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2"
                                >Title</label
                            >
                            <input
                                v-model="issueForm.title"
                                type="text"
                                class="w-full px-4 py-3 border border-gray-200 dark:border-gray-700 rounded-xl dark:bg-gray-900 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all"
                                placeholder="What needs to be reviewed?"
                            />
                        </div>

                        <div>
                            <label
                                class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2"
                                >Description</label
                            >
                            <textarea
                                v-model="issueForm.body"
                                rows="10"
                                class="w-full px-4 py-3 border border-gray-200 dark:border-gray-700 rounded-xl dark:bg-gray-900 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all resize-none"
                                placeholder="Describe the changes in detail..."
                            ></textarea>
                        </div>

                        <div>
                            <label
                                class="block text-xs font-bold uppercase tracking-widest text-gray-500 mb-2"
                                >Status</label
                            >
                            <select
                                v-model="issueForm.status"
                                class="w-full px-4 py-3 border border-gray-200 dark:border-gray-700 rounded-xl dark:bg-gray-900 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all appearance-none"
                            >
                                <option :value="IssueStatus.Draft">
                                    Draft
                                </option>
                                <option :value="IssueStatus.SubmitForReview">
                                    Submit for Review
                                </option>
                            </select>
                        </div>
                    </div>

                    <div
                        class="p-8 border-t border-gray-100 dark:border-gray-700 flex justify-end gap-3"
                    >
                        <button
                            @click="closeModal"
                            class="px-6 py-3 text-gray-600 font-bold bg-gray-100 hover:bg-gray-200 rounded-2xl dark:bg-gray-700 dark:text-gray-300 dark:hover:bg-gray-600 transition-all"
                            :disabled="saving"
                        >
                            Cancel
                        </button>
                        <button
                            @click="handleSaveIssue"
                            class="px-8 py-3 bg-blue-600 text-white font-bold rounded-2xl hover:bg-blue-700 shadow-md shadow-blue-200 dark:shadow-none transition-all flex items-center gap-2"
                            :disabled="saving || !issueForm.title"
                        >
                            <i v-if="saving" class="pi pi-spin pi-spinner"></i>
                            {{
                                saving
                                    ? "Saving..."
                                    : editingIssue
                                      ? "Update Issue"
                                      : "Create Issue"
                            }}
                        </button>
                    </div>
                </div>
            </div>
        </transition>
    </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { storeToRefs } from "pinia";
import { useIssueStore } from "../stores/issues";
import { IssueStatus, type Repository, type User, type Issue } from "../types";

const props = defineProps<{
    repo: Repository;
    user: User;
}>();

const issueStore = useIssueStore();
const { issues, loading, error } = storeToRefs(issueStore);

const showCreateModal = ref(false);
const showViewModal = ref(false);
const saving = ref(false);
const editingIssue = ref<Issue | null>(null);
const viewingIssue = ref<Issue | null>(null);

const issueForm = ref<{
    title: string;
    body: string;
    status: IssueStatus;
}>({
    title: "",
    body: "",
    status: IssueStatus.Draft,
});

const isAuditor = computed(() => {
    if (!props.repo || !props.user) return false;
    const auditorIds = props.repo.auditorsIds || [];
    return (
        auditorIds.includes(props.user.id) ||
        props.repo.ownerId === props.user.id
    );
});

const filteredIssues = computed(() => {
    return issues.value.filter((issue) => {
        if (issue.status !== IssueStatus.Draft) return true;
        return issue.ownerId === props.user?.id;
    });
});

const openEditModal = (issue: Issue) => {
    editingIssue.value = issue;
    issueForm.value = {
        title: issue.title,
        body: issue.body,
        status:
            issue.status === IssueStatus.Rejected
                ? IssueStatus.Draft
                : issue.status,
    };
    showCreateModal.value = true;
};

const openViewModal = (issue: Issue) => {
    viewingIssue.value = issue;
    showViewModal.value = true;
};

const closeModal = () => {
    showCreateModal.value = false;
    showViewModal.value = false;
    editingIssue.value = null;
    viewingIssue.value = null;
    issueForm.value = {
        title: "",
        body: "",
        status: IssueStatus.Draft,
    };
};

const handleSaveIssue = async () => {
    saving.value = true;
    try {
        if (editingIssue.value) {
            await issueStore.updateIssue(editingIssue.value.id, {
                title: issueForm.value.title,
                body: issueForm.value.body,
                status: issueForm.value.status,
            });
        } else {
            await issueStore.createIssue({
                title: issueForm.value.title,
                body: issueForm.value.body,
                repositoryId: props.repo.id,
                status: issueForm.value.status,
            });
        }
        closeModal();
    } catch (err) {
        console.error("Failed to save issue", err);
        alert("Failed to save issue. Please try again.");
    } finally {
        saving.value = false;
    }
};

const handleDeleteIssue = async (id: string) => {
    if (
        !confirm(
            "Are you sure you want to delete this issue? This action cannot be undone.",
        )
    ) {
        return;
    }
    try {
        await issueStore.deleteIssue(id);
    } catch (err) {
        console.error("Failed to delete issue", err);
    }
};

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
