import { apiClient } from "./client";

export const documentPermissionsApi = {
    getForDocument: (documentId) => apiClient.get(`/documents/${documentId}/permissions`),
    grant: (documentId, dto) => apiClient.post(`/documents/${documentId}/permissions`, dto),
    revoke: (documentId, userId) =>
        apiClient.delete(`/documents/${documentId}/permissions/${userId}`),
};