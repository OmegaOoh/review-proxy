import { api } from "./client";
import type { User } from "../types";

const userCache = new Map<string, User>();

export const IdentityService = {
  async getMe(): Promise<User> {
    const user = await api.get<User>("/api/identities/me");
    userCache.set(user.id, user);
    return user;
  },

  async getUser(id: string): Promise<User | null> {
    if (userCache.has(id)) return userCache.get(id)!;
    try {
      const user = await api.get<User>(`/api/identities/${id}`);
      userCache.set(id, user);
      return user;
    } catch (err) {
      console.error(`Failed to fetch user ${id}`, err);
      return null;
    }
  },

  async searchUsers(query: string): Promise<User[]> {
    if (!query || query.length < 2) return [];
    const users = await api.get<User[]>(
      `/api/identities?query=${encodeURIComponent(query)}`,
    );
    users.forEach((u) => userCache.set(u.id, u));
    return users;
  },

  cacheUser(user: User) {
    userCache.set(user.id, user);
  },

  clearCache() {
    userCache.clear();
  },
};
