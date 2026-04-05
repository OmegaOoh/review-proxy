<script setup lang="ts">
import { onMounted } from "vue";
import { useAuth } from "./composables/useAuth";
import UserProfile from "./components/UserProfile.vue";
import Login from "./components/Login.vue";

const { user, isAuthenticated, loading, fetchUser } = useAuth();

onMounted(() => {
    fetchUser();
});
</script>

<template>
    <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
        <div class="container mx-auto px-4 py-8">
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
                    <router-view :user="user" />
                </template>
                <template v-else>
                    <Login />
                </template>
            </div>
        </div>
    </div>
</template>
