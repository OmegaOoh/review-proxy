<script setup lang="ts">
import { ref } from "vue";

const props = defineProps<{
    repo: any;
    user: any;
    show: boolean;
}>();

const emit = defineEmits<{
    close: [];
    created: [issue: any];
}>();

const issueForm = ref({
    title: "",
    body: "",
    status: "Draft",
});

const creating = ref(false);

const closeModal = () => {
    emit("close");
};

const createIssue = async () => {
    if (!issueForm.value.title) {
        alert("Title is required");
        return;
    }

    creating.value = true;
    try {
        const token = localStorage.getItem("token");
        const headers: HeadersInit = {
            "Content-Type": "application/json",
            Accept: "application/json",
        };
        if (token) {
            headers["Authorization"] = `Bearer ${token}`;
        }

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
            emit("created", newIssue);
            issueForm.value = {
                title: "",
                body: "",
                status: "Draft",
            };
        } else {
            const data = await response.json().catch(() => ({}));
            throw new Error(data.message || "Failed to create issue");
        }
    } catch (err: any) {
        console.error(err);
        alert(err.message || "An error occurred while creating the issue.");
    } finally {
        creating.value = false;
    }
};
</script>

<template>
    <div
        v-if="show"
        class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    >
        <div
            class="bg-white dark:bg-gray-800 rounded-lg w-full max-w-4xl h-full max-h-[90vh] p-6 shadow-xl flex flex-col overflow-hidden"
        >
            <div class="flex justify-between items-center mb-4">
                <h3 class="text-xl font-bold text-gray-900 dark:text-white">
                    Create Issue for
                    {{ repo?.githubRepoId || repo?.gitHubRepoId }}
                </h3>
                <button
                    @click="closeModal"
                    class="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                >
                    <i class="pi pi-times"></i>
                </button>
            </div>

            <div class="flex flex-col gap-4 flex-grow overflow-y-auto">
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

                <div class="flex-grow">
                    <label
                        class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                        >Description</label
                    >
                    <textarea
                        v-model="issueForm.body"
                        rows="10"
                        class="w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:border-gray-600 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 h-full resize-none"
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
                        <option value="Submitted">Submit for Review</option>
                    </select>
                </div>
            </div>

            <div class="mt-6 flex justify-end gap-3">
                <button
                    @click="closeModal"
                    class="px-4 py-2 text-gray-600 bg-gray-100 hover:bg-gray-200 rounded dark:bg-gray-700 dark:text-gray-300 dark:hover:bg-gray-600 transition"
                    :disabled="creating"
                >
                    Cancel
                </button>
                <button
                    @click="createIssue"
                    class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition flex items-center gap-2"
                    :disabled="creating || !issueForm.title"
                >
                    <i v-if="creating" class="pi pi-spin pi-spinner"></i>
                    {{ creating ? "Creating..." : "Save Issue" }}
                </button>
            </div>
        </div>
    </div>
</template>
