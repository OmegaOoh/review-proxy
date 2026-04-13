import { api } from "./client";
import type { Repository, DepositRequest, User } from "../types";

export const RepositoryService = {
  async fetchAll(): Promise<Repository[]> {
    return api.get<Repository[]>("/api/repositories");
  },

  async deposit(request: DepositRequest): Promise<Repository> {
    return api.post<Repository>("/api/repositories", request);
  },

  async updateDescription(repoId: string, description: string): Promise<void> {
    return api.patch(`/api/repositories/${repoId}`, { description });
  },

  async getAuditors(repoId: string): Promise<string[]> {
    return api.get<string[]>(`/api/repositories/${repoId}/auditors`);
  },

  async getAuditorsDetails(repoId: string): Promise<User[]> {
    return api.get<User[]>(`/api/repositories/${repoId}/auditors/details`);
  },

  async addAuditors(repoId: string, userIds: string[]): Promise<void> {
    return api.post(`/api/repositories/${repoId}/auditors`, userIds);
  },

  async removeAuditors(repoId: string, userIds: string[]): Promise<void> {
    return api.delete(`/api/repositories/${repoId}/auditors`, userIds);
  },

  async delete(repoId: string): Promise<void> {
    return api.delete(`/api/repositories/${repoId}`);
  },

  async getSyncContext(): Promise<any> {
    return api.get<any>("/api/sync/context");
  },

  async getSyncRepositories(): Promise<any[]> {
    return api.get<any[]>("/api/sync/repositories");
  },
};
