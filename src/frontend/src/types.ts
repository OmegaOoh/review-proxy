export const IssueStatus = {
  Draft: "Draft",
  SubmitForReview: "SubmitForReview",
  Approved: "Approved",
  Rejected: "Rejected",
} as const;

export type IssueStatus = (typeof IssueStatus)[keyof typeof IssueStatus];

export interface User {
  id: string;
  gitHubUsername: string;
  gitHubAvatarUrl?: string;
  gitHubID: string;
}

export interface Repository {
  id: string;
  gitHubRepoId: string;
  ownerId: string;
  description: string;
  auditorsIds: string[];
  createdAt: string;
  // Client-side extensions for convenience
  owner?: User;
  auditors?: User[];
}
export interface Issue {
  id: string;
  title: string;
  body: string;
  status: IssueStatus;
  ownerId: string;
  repositoryId: string;
  createdAt: string;
  updatedAt: string;
  // Client-side extension
  owner?: User;
}

export interface DepositRequest {
  githubRepoId: string;
  description: string;
  gitHubToken?: string;
  auditors: string[];
}

export interface CreateIssueRequest {
  title: string;
  body: string;
  repositoryId: string;
  status: IssueStatus;
}

export interface UpdateIssueRequest {
  title: string;
  body: string;
  status: IssueStatus;
}
