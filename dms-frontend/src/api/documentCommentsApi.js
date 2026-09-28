import { apiClient } from "./client";

export const documentCommentsApi = {
    getForDocument: (documentId) => apiClient.get(`/documents/${documentId}/comments`),
    add: (documentId, content) => apiClient.post(`/documents/${documentId}/comments`, { content }),
    delete: (documentId, commentId) =>
        apiClient.delete(`/documents/${documentId}/comments/${commentId}`),
};