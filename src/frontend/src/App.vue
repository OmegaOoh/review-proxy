<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useAuth } from "./composables/useAuth";
import UserProfile from "./components/UserProfile.vue";
import RepositoryManager from "./components/RepositoryManager.vue";
import IssueManager from "./components/IssueManager.vue";
import Login from "./components/Login.vue";

const appTitle = ref("Review Proxy");
const { user, isAuthenticated, loading, fetchUser } = useAuth();

onMounted(() => {
    fetchUser();
});
</script>

<template>
    <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
        <div class="container mx-auto px-4 py-8">
            <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-6">
                {{ appTitle }}
            </h1>
            <p class="text-gray-700 dark:text-gray-300 mb-4">
                Welcome to the Review Proxy application! This is a placeholder
                for the main content of the app.
            </p>

            <div v-if="loading" class="mt-6 flex items-center">
                <i class="pi pi-spin pi-spinner text-2xl text-blue-600"></i>
                <span class="ml-3 text-gray-700 dark:text-gray-300"
                    >Loading user data...</span
                >
            </div>

            <div v-else>
                <template v-if="isAuthenticated && user">
                    <div class="mt-6">
                        <UserProfile />
                    </div>

                    <RepositoryManager :user="user" />
                    <IssueManager :user="user" />
                </template>
                <template v-else>
                    <Login />
                </template>
            </div>
        </div>
    </div>
</template>
