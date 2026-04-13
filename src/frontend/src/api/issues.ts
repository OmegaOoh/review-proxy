import { api } from "./client";
import type { Issue, CreateIssueRequest, UpdateIssueRequest } from "../types";

export const IssueService = {
  async fetchAll(): Promise<Issue[]> {
    return api.get<Issue[]>("/api/issues");
  },

  async create(request: CreateIssueRequest): Promise<Issue> {
    return api.post<Issue>("/api/issues", request);
  },

  async update(id: string, request: UpdateIssueRequest): Promise<Issue> {
    return api.put<Issue>(`/api/issues/${id}`, request);
  },

  async delete(id: string): Promise<void> {
    return api.delete(`/api/issues/${id}`);
  },

  async approve(id: string): Promise<void> {
    return api.post(`/api/issues/${id}/approve`);
  },

  async reject(id: string): Promise<void> {
    return api.post(`/api/issues/${id}/reject`);
  },
};
