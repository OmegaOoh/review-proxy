import { defineStore } from "pinia";
import { ref } from "vue";
import type { Repository, User, DepositRequest } from "../types";
import { RepositoryService } from "../api/repositories";
import { IdentityService } from "../api/identities";

export const useRepoStore = defineStore("repositories", () => {
  const repositories = ref<Repository[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchAllRepositories() {
    loading.value = true;
    error.value = null;
    try {
      repositories.value = await RepositoryService.fetchAll();
      await enrichRepositories();
    } catch (err: any) {
      console.error("Failed to fetch repositories", err);
      error.value = err.message;
    } finally {
      loading.value = false;
    }
  }

  async function enrichRepositories() {
    await Promise.all(
      repositories.value.map(async (repo) => {
        const [owner, auditors] = await Promise.all([
          IdentityService.getUser(repo.ownerId),
          RepositoryService.getAuditorsWithFallback(repo.id, repo.ownerId),
        ]);
        repo.owner = owner || undefined;
        repo.auditors = auditors;
      }),
    );
  }

  async function depositRepository(request: DepositRequest) {
    loading.value = true;
    try {
      const newRepo = await RepositoryService.deposit(request);
      repositories.value.push(newRepo);
      await enrichRepositories(); // Ensure new repo is enriched
      return newRepo;
    } catch (err: any) {
      error.value = err.message;
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function updateRepositoryDescription(
    repoId: string,
    description: string,
  ) {
    try {
      await RepositoryService.updateDescription(repoId, description);
      const repo = repositories.value.find((r) => r.id === repoId);
      if (repo) repo.description = description;
    } catch (err: any) {
      error.value = err.message;
      throw err;
    }
  }

  async function addAuditors(repoId: string, userIds: string[]) {
    try {
      await RepositoryService.addAuditors(repoId, userIds);
      const repo = repositories.value.find((r) => r.id === repoId);
      if (repo) {
        const auditors = await RepositoryService.getAuditorsWithFallback(
          repo.id,
          repo.ownerId,
        );
        repo.auditors = auditors;
      }
    } catch (err: any) {
      error.value = err.message;
      throw err;
    }
  }

  async function removeAuditor(repoId: string, userId: string) {
    try {
      await RepositoryService.removeAuditors(repoId, [userId]);
      const repo = repositories.value.find((r) => r.id === repoId);
      if (repo) {
        repo.auditors = repo.auditors?.filter((a) => a.id !== userId);
      }
    } catch (err: any) {
      error.value = err.message;
      throw err;
    }
  }

  async function deleteRepository(repoId: string) {
    loading.value = true;
    try {
      await RepositoryService.delete(repoId);
      repositories.value = repositories.value.filter((r) => r.id !== repoId);
    } catch (err: any) {
      error.value = err.message;
      throw err;
    } finally {
      loading.value = false;
    }
  }

  return {
    repositories,
    loading,
    error,
    fetchAllRepositories,
    depositRepository,
    updateRepositoryDescription,
    addAuditors,
    removeAuditor,
    deleteRepository,
  };
});
