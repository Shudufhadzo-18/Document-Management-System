import { apiClient } from "./client";

export const usersApi = {
    getAll: () => apiClient.get("/Users"),
    getAllRoles: () => apiClient.get("/Users/roles"),
    assignRole: (userId, role) => apiClient.post(`/Users/${userId}/roles`, { role }),
    removeRole: (userId, role) =>
        apiClient.delete(`/Users/${userId}/roles/${role}`),
};