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
            <IssueListItem
                v-for="issue in filteredIssues"
                :key="issue.id"
                :issue="issue"
                :user="user"
                :is-auditor="isAuditor"
                @view="openViewModal"
                @edit="openEditModal"
                @delete="handleDeleteIssue"
                @approve="issueStore.approveIssue"
                @reject="issueStore.rejectIssue"
            />
        </div>

        <IssueViewModal
            :show="showViewModal"
            :issue="viewingIssue"
            @close="closeModals"
        />

        <IssueFormModal
            :show="showCreateModal"
            :repo="repo"
            :editing-issue="editingIssue"
            :saving="saving"
            @close="closeModals"
            @save="handleSaveIssue"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { storeToRefs } from "pinia";
import { useIssueStore } from "../stores/issues";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { IssueStatus, type Repository, type User, type Issue } from "../types";
import IssueListItem from "./IssueListItem.vue";
import IssueViewModal from "./IssueViewModal.vue";
import IssueFormModal from "./IssueFormModal.vue";

const props = defineProps<{
    repo: Repository;
    user: User;
}>();

const issueStore = useIssueStore();
const confirm = useConfirm();
const toast = useToast();
const { issues, loading, error } = storeToRefs(issueStore);

const showCreateModal = ref(false);
const showViewModal = ref(false);
const saving = ref(false);
const editingIssue = ref<Issue | null>(null);
const viewingIssue = ref<Issue | null>(null);

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
    showCreateModal.value = true;
};

const openViewModal = (issue: Issue) => {
    viewingIssue.value = issue;
    showViewModal.value = true;
};

const closeModals = () => {
    showCreateModal.value = false;
    showViewModal.value = false;
    editingIssue.value = null;
    viewingIssue.value = null;
};

const handleSaveIssue = async (form: {
    title: string;
    body: string;
    status: IssueStatus;
}) => {
    saving.value = true;
    try {
        if (editingIssue.value) {
            await issueStore.updateIssue(editingIssue.value.id, form);
        } else {
            await issueStore.createIssue({
                ...form,
                repositoryId: props.repo.id,
            });
        }
        closeModals();
        toast.add({
            severity: "success",
            summary: "Success",
            detail: "Issue saved successfully",
            life: 3000,
        });
    } catch (err) {
        console.error("Failed to save issue", err);
        toast.add({
            severity: "error",
            summary: "Error",
            detail: "Failed to save issue. Please try again.",
            life: 5000,
        });
    } finally {
        saving.value = false;
    }
};

const handleDeleteIssue = async (id: string) => {
    confirm.require({
        message: "Are you sure you want to delete this issue?",
        header: "Delete Issue",
        icon: "pi pi-exclamation-triangle",
        rejectProps: {
            label: "Cancel",
            severity: "secondary",
            outlined: true,
        },
        acceptProps: {
            label: "Delete",
            severity: "danger",
        },
        accept: async () => {
            try {
                await issueStore.deleteIssue(id);
                toast.add({
                    severity: "success",
                    summary: "Success",
                    detail: "Issue deleted successfully",
                    life: 3000,
                });
            } catch (err) {
                console.error("Failed to delete issue", err);
                toast.add({
                    severity: "error",
                    summary: "Error",
                    detail: "Failed to delete issue",
                    life: 5000,
                });
            }
        },
    });
};
</script>
