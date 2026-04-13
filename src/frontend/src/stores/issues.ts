import { defineStore } from "pinia";
import { ref } from "vue";
import type { Issue, CreateIssueRequest, UpdateIssueRequest } from "../types";
import { IssueService } from "../api/issues";
import { IdentityService } from "../api/identities";

export const useIssueStore = defineStore("issues", () => {
  const issues = ref<Issue[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchIssuesForRepository(repoId: string) {
    loading.value = true;
    error.value = null;
    try {
      const allIssues = await IssueService.fetchAll();
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
    const promises = issues.value.map(async (issue) => {
      issue.owner = (await IdentityService.getUser(issue.ownerId)) || undefined;
    });
    await Promise.all(promises);
  }

  async function createIssue(request: CreateIssueRequest) {
    try {
      const newIssue = await IssueService.create(request);
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
      const updatedIssue = await IssueService.update(id, request);
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
      await IssueService.delete(id);
      issues.value = issues.value.filter((i) => i.id !== id);
    } catch (err: any) {
      error.value = err.message;
      throw err;
    }
  }

  async function approveIssue(id: string) {
    try {
      await IssueService.approve(id);
      const issue = issues.value.find((i) => i.id === id);
      if (issue) issue.status = "Approved" as any;
    } catch (err: any) {
      error.value = err.message;
      throw err;
    }
  }

  async function rejectIssue(id: string) {
    try {
      await IssueService.reject(id);
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
