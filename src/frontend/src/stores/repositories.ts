import { defineStore } from "pinia";
import { ref } from "vue";
import type { Repository, User, DepositRequest } from "../types";
import { api } from "../api/client";

export const useRepoStore = defineStore("repositories", () => {
  const repositories = ref<Repository[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchAllRepositories() {
    loading.value = true;
    error.value = null;
    try {
      const data = await api.get<Repository[]>("/api/repositories");
      repositories.value = data;
      // Fetch owner and auditor details
      await enrichRepositories();
    } catch (err: any) {
      console.error("Failed to fetch repositories", err);
      error.value = err.message;
    } finally {
      loading.value = false;
    }
  }

  async function enrichRepositories() {
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

    const promises = repositories.value.map(async (repo) => {
      // Fetch Owner
      repo.owner = (await fetchUserDetails(repo.ownerId)) || undefined;

      // Fetch Auditors
      try {
        // Try detailed endpoint first
        const auditors = await api.get<User[]>(
          `/api/repositories/${repo.id}/auditors/details`,
        );
        repo.auditors = auditors.filter((a) => a.id !== repo.ownerId);
        // Pre-warm cache with these auditors
        auditors.forEach((a) => userCache.set(a.id, a));
      } catch (err) {
        // Fallback: fetch IDs then details
        try {
          const auditorIds = await api.get<string[]>(
            `/api/repositories/${repo.id}/auditors`,
          );
          const filteredIds = auditorIds.filter((id) => id !== repo.ownerId);
          const auditorPromises = filteredIds.map((id) => fetchUserDetails(id));
          const results = await Promise.all(auditorPromises);
          repo.auditors = results.filter((u): u is User => !!u);
        } catch (fallbackErr) {
          console.error(
            `Failed to fetch auditors for repo ${repo.id}`,
            fallbackErr,
          );
          repo.auditors = [];
        }
      }
    });
    await Promise.all(promises);
  }

  async function depositRepository(request: DepositRequest) {
    loading.value = true;
    try {
      const newRepo = await api.post<Repository>("/api/repositories", request);
      repositories.value.push(newRepo);
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
      await api.patch(`/api/repositories/${repoId}`, { description });
      const repo = repositories.value.find((r) => r.id === repoId);
      if (repo) repo.description = description;
    } catch (err: any) {
      error.value = err.message;
      throw err;
    }
  }

  async function addAuditors(repoId: string, userIds: string[]) {
    try {
      await api.post(`/api/repositories/${repoId}/auditors`, userIds);
      // Refresh auditors for this repo
      const repo = repositories.value.find((r) => r.id === repoId);
      if (repo) {
        // Filter out owner just in case
        const filteredIds = userIds.filter((id) => id !== repo.ownerId);
        const detailsPromises = filteredIds.map((id) =>
          api.get<User>(`/api/identities/${id}`),
        );
        const newAuditors = await Promise.all(detailsPromises);
        repo.auditors = [...(repo.auditors || []), ...newAuditors];
      }
    } catch (err: any) {
      error.value = err.message;
      throw err;
    }
  }

  async function removeAuditor(repoId: string, userId: string) {
    try {
      await api.delete(`/api/repositories/${repoId}/auditors`, [userId]);
      const repo = repositories.value.find((r) => r.id === repoId);
      if (repo) {
        repo.auditors = repo.auditors?.filter((a) => a.id !== userId);
      }
    } catch (err: any) {
      error.value = err.message;
      throw err;
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
  };
});
