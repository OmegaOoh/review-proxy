<template>
    <div class="space-y-6">
        <div class="flex justify-between items-center">
            <h2 class="text-2xl font-semibold text-gray-800 dark:text-white">
                Issues
            </h2>
            <button
                @click="showCreateModal = true"
                class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition flex items-center gap-2"
            >
                <i class="pi pi-plus"></i> Create Issue
            </button>
        </div>

        <div v-if="loading" class="flex items-center justify-center py-8">
            <i class="pi pi-spin pi-spinner text-3xl text-blue-600"></i>
        </div>

        <div v-else-if="error" class="p-4 bg-red-100 text-red-700 rounded-md">
            {{ error }}
        </div>

        <div
            v-else-if="filteredIssues.length === 0"
            class="text-center py-8 text-gray-500 dark:text-gray-400"
        >
            No issues found. Create a new issue to get started.
        </div>

        <div v-else class="space-y-4">
            <div
                v-for="issue in filteredIssues"
                :key="issue.id"
                class="p-4 border rounded-md border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 flex flex-col sm:flex-row justify-between gap-4"
            >
                <div>
                    <div class="flex items-center gap-3 mb-2">
                        <h3
                            class="text-lg font-medium text-gray-900 dark:text-white"
                        >
                            {{ issue.title }}
                        </h3>
                        <span
                            class="px-2 py-1 text-xs font-semibold rounded-full"
                            :class="{
                                'bg-gray-200 text-gray-800':
                                    issue.status === 'Draft',
                                'bg-blue-100 text-blue-800':
                                    issue.status === 'SubmitForReview',
                                'bg-green-100 text-green-800':
                                    issue.status === 'Approved',
                                'bg-red-100 text-red-800':
                                    issue.status === 'Rejected',
                            }"
                        >
                            {{
                                issue.status === "SubmitForReview"
                                    ? "Submit for Review"
                                    : issue.status
                            }}
                        </span>
                    </div>
                    <p
                        @click="openViewModal(issue)"
                        class="text-sm text-gray-600 dark:text-gray-300 mb-2 whitespace-pre-line line-clamp-3 cursor-pointer hover:text-gray-900 dark:hover:text-white transition-colors"
                        title="Click to view full description"
                    >
                        {{ issue.body || "No description provided." }}
                    </p>
                    <div class="text-xs text-gray-500 flex items-center gap-4">
                        <span
                            ><i class="pi pi-user mr-1"></i>
                            {{ issue.ownerId.substring(0, 8) }}...</span
                        >
                        <span
                            ><i class="pi pi-calendar mr-1"></i>
                            {{ formatDate(issue.createdAt) }}</span
                        >
                    </div>
                </div>

                <div class="flex items-start gap-2">
                    <template v-if="issue.ownerId === props.user?.id">
                        <button
                            v-if="issue.status !== 'Approved'"
                            @click="openEditModal(issue)"
                            class="px-3 py-1.5 text-sm bg-blue-100 text-blue-600 rounded hover:bg-blue-200 transition flex items-center gap-1"
                        >
                            <i class="pi pi-pencil"></i> Edit
                        </button>
                        <button
                            v-if="issue.status !== 'Approved'"
                            @click="deleteIssue(issue.id)"
                            class="px-3 py-1.5 text-sm bg-red-100 text-red-600 rounded hover:bg-red-200 transition flex items-center gap-1"
                        >
                            <i class="pi pi-trash"></i> Delete
                        </button>
                    </template>
                    <template
                        v-if="
                            isAuditor &&
                            issue.status === 'SubmitForReview' &&
                            issue.ownerId !== props.user?.id
                        "
                    >
                        <button
                            @click="approveIssue(issue.id)"
                            class="px-3 py-1.5 text-sm bg-green-600 text-white rounded hover:bg-green-700 transition flex items-center gap-1"
                        >
                            <i class="pi pi-check"></i> Approve
                        </button>
                        <button
                            @click="rejectIssue(issue.id)"
                            class="px-3 py-1.5 text-sm bg-red-600 text-white rounded hover:bg-red-700 transition flex items-center gap-1"
                        >
                            <i class="pi pi-times"></i> Reject
                        </button>
                    </template>
                </div>
            </div>
        </div>

        <!-- View Issue Modal -->
        <div
            v-if="showViewModal && viewingIssue"
            class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
        >
            <div
                class="bg-white dark:bg-gray-800 rounded-lg max-w-4xl w-full h-[80vh] flex flex-col p-6 shadow-2xl"
            >
                <div class="flex justify-between items-start mb-4">
                    <div>
                        <h3
                            class="text-2xl font-bold text-gray-900 dark:text-white"
                        >
                            {{ viewingIssue.title }}
                        </h3>
                        <div class="mt-1 flex items-center gap-2">
                            <span
                                class="px-2 py-0.5 text-xs font-semibold rounded-full"
                                :class="{
                                    'bg-gray-200 text-gray-800':
                                        viewingIssue.status === 'Draft',
                                    'bg-blue-100 text-blue-800':
                                        viewingIssue.status ===
                                        'SubmitForReview',
                                    'bg-green-100 text-green-800':
                                        viewingIssue.status === 'Approved',
                                    'bg-red-100 text-red-800':
                                        viewingIssue.status === 'Rejected',
                                }"
                            >
                                {{
                                    viewingIssue.status === "SubmitForReview"
                                        ? "Submit for Review"
                                        : viewingIssue.status
                                }}
                            </span>
                            <span class="text-xs text-gray-500">
                                Created by
                                {{ viewingIssue.ownerId.substring(0, 8) }}... on
                                {{ formatDate(viewingIssue.createdAt) }}
                            </span>
                        </div>
                    </div>
                    <button
                        @click="closeModal"
                        class="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                    >
                        <i class="pi pi-times text-xl"></i>
                    </button>
                </div>

                <div
                    class="flex-grow overflow-y-auto mt-4 p-4 bg-gray-50 dark:bg-gray-700/50 rounded-lg border border-gray-100 dark:border-gray-600"
                >
                    <p
                        class="text-gray-800 dark:text-gray-200 whitespace-pre-line leading-relaxed"
                    >
                        {{ viewingIssue.body || "No description provided." }}
                    </p>
                </div>

                <div class="mt-6 flex justify-end">
                    <button
                        @click="closeModal"
                        class="px-6 py-2 bg-gray-900 text-white dark:bg-white dark:text-gray-900 rounded-lg font-medium hover:bg-gray-800 dark:hover:bg-gray-100 transition-colors"
                    >
                        Close
                    </button>
                </div>
            </div>
        </div>

        <!-- Create/Edit Issue Modal -->
        <div
            v-if="showCreateModal"
            class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
        >
            <div
                class="bg-white dark:bg-gray-800 rounded-lg max-w-5xl w-full h-[90vh] flex flex-col p-6"
            >
                <h3
                    class="text-xl font-bold mb-4 text-gray-900 dark:text-white"
                >
                    {{ editingIssue ? "Edit Issue" : "Create New Issue" }}
                </h3>

                <div class="space-y-4 flex-grow flex flex-col">
                    <div>
                        <label
                            class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                            >Title</label
                        >
                        <input
                            v-model="issueForm.title"
                            type="text"
                            class="w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:border-gray-600 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Issue title"
                        />
                    </div>

                    <div class="flex-grow flex flex-col">
                        <label
                            class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                            >Description</label
                        >
                        <textarea
                            v-model="issueForm.body"
                            class="w-full flex-grow px-3 py-2 border rounded-md dark:bg-gray-700 dark:border-gray-600 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
                            placeholder="Detailed description..."
                        ></textarea>
                    </div>

                    <div>
                        <label
                            class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                            >Status</label
                        >
                        <select
                            v-model="issueForm.status"
                            class="w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:border-gray-600 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                        >
                            <option value="Draft">Draft</option>
                            <option value="SubmitForReview">
                                Submit for Review
                            </option>
                        </select>
                    </div>
                </div>

                <div class="mt-6 flex justify-end gap-3">
                    <button
                        @click="closeModal"
                        class="px-4 py-2 text-gray-600 bg-gray-100 hover:bg-gray-200 rounded dark:bg-gray-700 dark:text-gray-300 dark:hover:bg-gray-600 transition"
                        :disabled="saving"
                    >
                        Cancel
                    </button>
                    <button
                        @click="saveIssue"
                        class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition flex items-center gap-2"
                        :disabled="saving"
                    >
                        <i v-if="saving" class="pi pi-spin pi-spinner"></i>
                        {{
                            saving
                                ? editingIssue
                                    ? "Updating..."
                                    : "Creating..."
                                : editingIssue
                                  ? "Update Issue"
                                  : "Save Issue"
                        }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";

const props = defineProps<{
    repo: any;
    user: any;
}>();

const issues = ref<any[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);
const showCreateModal = ref(false);
const showViewModal = ref(false);
const saving = ref(false);
const editingIssue = ref<any>(null);
const viewingIssue = ref<any>(null);

const issueForm = ref({
    title: "",
    body: "",
    status: "Draft",
});

const isAuditor = computed(() => {
    if (!props.repo || !props.user) return false;
    const auditorIds = props.repo.auditors || props.repo.auditorsIds || [];
    return (
        auditorIds.includes(props.user.id) ||
        props.repo.ownerId === props.user.id
    );
});

const filteredIssues = computed(() => {
    return issues.value.filter((issue) => {
        if (issue.status !== "Draft") return true;
        return issue.ownerId === props.user?.id;
    });
});

const fetchIssues = async () => {
    loading.value = true;
    error.value = null;
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = { Accept: "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch("/api/issues", { headers });
        if (response.ok) {
            const allIssues = await response.json();
            issues.value = allIssues.filter(
                (issue: any) => issue.repositoryId === props.repo.id,
            );
        } else if (response.status === 404 || response.status === 401) {
            issues.value = [];
        } else {
            throw new Error("Failed to load issues");
        }
    } catch (err: any) {
        console.error(err);
        error.value = err.message || "An error occurred while fetching issues.";
    } finally {
        loading.value = false;
    }
};

const openEditModal = (issue: any) => {
    editingIssue.value = issue;
    issueForm.value = {
        title: issue.title,
        body: issue.body,
        status: issue.status === "Rejected" ? "Draft" : issue.status,
    };
    showCreateModal.value = true;
};

const openViewModal = (issue: any) => {
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
        status: "Draft",
    };
};

const saveIssue = async () => {
    if (editingIssue.value) {
        await updateIssue();
    } else {
        await createIssue();
    }
};

const createIssue = async () => {
    if (!issueForm.value.title) {
        alert("Title is required");
        return;
    }

    saving.value = true;
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = {
            "Content-Type": "application/json",
            Accept: "application/json",
        };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch("/api/issues", {
            method: "POST",
            headers,
            body: JSON.stringify({
                title: issueForm.value.title,
                body: issueForm.value.body,
                repositoryId: props.repo.id,
                status: issueForm.value.status,
            }),
        });

        if (response.ok) {
            const newIssue = await response.json();
            issues.value.push(newIssue);
            closeModal();
        } else {
            const data = await response.json().catch(() => ({}));
            throw new Error(data.message || "Failed to create issue");
        }
    } catch (err: any) {
        console.error(err);
        alert(err.message || "An error occurred while creating the issue.");
    } finally {
        saving.value = false;
    }
};

const updateIssue = async () => {
    if (!issueForm.value.title) {
        alert("Title is required");
        return;
    }

    saving.value = true;
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = {
            "Content-Type": "application/json",
            Accept: "application/json",
        };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(`/api/issues/${editingIssue.value.id}`, {
            method: "PUT",
            headers,
            body: JSON.stringify({
                title: issueForm.value.title,
                body: issueForm.value.body,
                status: issueForm.value.status,
            }),
        });

        if (response.ok) {
            const updatedIssue = await response.json();
            const index = issues.value.findIndex(
                (i) => i.id === updatedIssue.id,
            );
            if (index !== -1) {
                issues.value[index] = updatedIssue;
            }
            closeModal();
        } else {
            const data = await response.json().catch(() => ({}));
            throw new Error(data.message || "Failed to update issue");
        }
    } catch (err: any) {
        console.error(err);
        alert(err.message || "An error occurred while updating the issue.");
    } finally {
        saving.value = false;
    }
};

const approveIssue = async (id: string) => {
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = { Accept: "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(`/api/issues/${id}/approve`, {
            method: "POST",
            headers,
        });

        if (response.ok) {
            const index = issues.value.findIndex((i) => i.id === id);
            if (index !== -1) {
                issues.value[index].status = "Approved";
            }
        } else {
            const data = await response.json().catch(() => ({}));
            throw new Error(data.message || "Failed to approve issue");
        }
    } catch (err: any) {
        console.error(err);
        alert(err.message || "An error occurred while approving the issue.");
    }
};

const rejectIssue = async (id: string) => {
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = { Accept: "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(`/api/issues/${id}/reject`, {
            method: "POST",
            headers,
        });

        if (response.ok) {
            const index = issues.value.findIndex((i) => i.id === id);
            if (index !== -1) {
                issues.value[index].status = "Rejected";
            }
        } else {
            const data = await response.json().catch(() => ({}));
            throw new Error(data.message || "Failed to reject issue");
        }
    } catch (err: any) {
        console.error(err);
        alert(err.message || "An error occurred while rejecting the issue.");
    }
};

const deleteIssue = async (id: string) => {
    if (!confirm("Are you sure you want to delete this issue?")) {
        return;
    }

    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = { Accept: "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(`/api/issues/${id}`, {
            method: "DELETE",
            headers,
        });

        if (response.ok) {
            issues.value = issues.value.filter((i) => i.id !== id);
        } else {
            const data = await response.json().catch(() => ({}));
            throw new Error(data.message || "Failed to delete issue");
        }
    } catch (err: any) {
        console.error(err);
        alert(err.message || "An error occurred while deleting the issue.");
    }
};

const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString(undefined, {
        year: "numeric",
        month: "short",
        day: "numeric",
    });
};

onMounted(() => {
    fetchIssues();
});
</script>
