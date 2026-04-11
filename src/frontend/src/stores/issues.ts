import { defineStore } from "pinia";
import { ref } from "vue";
import type {
  Issue,
  CreateIssueRequest,
  UpdateIssueRequest,
  User,
} from "../types";
import { api } from "../api/client";

export const useIssueStore = defineStore("issues", () => {
  const issues = ref<Issue[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchIssuesForRepository(repoId: string) {
    loading.value = true;
    error.value = null;
    try {
      const allIssues = await api.get<Issue[]>("/api/issues");
      issues.value = allIssues.filter((i) => i.repositoryId === repoId);
      await enrichIssues();
    } catch (err: any) {
      console.error("Failed to fetch issues", err);
      error.value = err.message;
    } finally {
      loading.value = false;
    }
  }

  async function enrichIssues() {
    const userCache = new Map<string, User>();
    const fetchUserDetails = async (id: string) => {
      if (userCache.has(id)) return userCache.get(id);
      try {
        const user = await api.get<User>(`/api/identities/${id}`);
        userCache.set(id, user);
        return user;
      } catch {
        return null;
      }
    };

    const promises = issues.value.map(async (issue) => {
      issue.owner = (await fetchUserDetails(issue.ownerId)) || undefined;
    });
    await Promise.all(promises);
  }

  async function createIssue(request: CreateIssueRequest) {
    try {
      const newIssue = await api.post<Issue>("/api/issues", request);
      issues.value.push(newIssue);
      await enrichIssues();
      return newIssue;
    } catch (err: any) {
      error.value = err.message;
      throw err;
    }
  }

  async function updateIssue(id: string, request: UpdateIssueRequest) {
    try {
      const updatedIssue = await api.put<Issue>(`/api/issues/${id}`, request);
      const index = issues.value.findIndex((i) => i.id === id);
      if (index !== -1) issues.value[index] = updatedIssue;
      await enrichIssues();
      return updatedIssue;
    } catch (err: any) {
      error.value = err.message;
      throw err;
    }
  }

  async function deleteIssue(id: string) {
    try {
      await api.delete(`/api/issues/${id}`);
      issues.value = issues.value.filter((i) => i.id !== id);
    } catch (err: any) {
      error.value = err.message;
      throw err;
    }
  }

  async function approveIssue(id: string) {
    try {
      await api.post(`/api/issues/${id}/approve`);
      const issue = issues.value.find((i) => i.id === id);
      if (issue) issue.status = "Approved" as any; // Cast as any because of enum string mismatch if any
    } catch (err: any) {
      error.value = err.message;
      throw err;
    }
  }

  async function rejectIssue(id: string) {
    try {
      await api.post(`/api/issues/${id}/reject`);
      const issue = issues.value.find((i) => i.id === id);
      if (issue) issue.status = "Rejected" as any;
    } catch (err: any) {
      error.value = err.message;
      throw err;
    }
  }

  return {
    issues,
    loading,
    error,
    fetchIssuesForRepository,
    createIssue,
    updateIssue,
    deleteIssue,
    approveIssue,
    rejectIssue,
  };
});
