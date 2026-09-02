import { apiClient } from "./client";

export const documentsApi = {
    getAll: () => apiClient.get("/Documents"),
    getById: (id) => apiClient.get(`/Documents/${id}`),
    create: (dto) => apiClient.post("/Documents", dto),
    delete: (id) => apiClient.delete(`/Documents/${id}`),
    updateStatus: (id, newStatus) =>
        apiClient.patch(`/Documents/${id}/status`, { newStatus }),
};