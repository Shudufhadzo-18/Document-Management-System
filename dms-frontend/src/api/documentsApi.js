import { apiClient } from "./client";

export const documentsApi = {
    getAll: () => apiClient.get("/Documents"),
    getById: (id) => apiClient.get(`/Documents/${id}`),
    create: (dto) => apiClient.post("/Documents", dto),
    delete: (id) => apiClient.delete(`/Documents/${id}`),
    updateStatus: (id, newStatus, reason = null) =>
        apiClient.patch(`/Documents/${id}/status`, { newStatus, reason }),
    getExpiring: (withinDays = 30) =>
        apiClient.get(`/Documents/expiring?withinDays=${withinDays}`),
    getExpired: () => apiClient.get("/Documents/expired"),
    getReviewDue: () => apiClient.get("/Documents/review-due"),
};