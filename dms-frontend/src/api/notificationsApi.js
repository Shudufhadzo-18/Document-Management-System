import { apiClient } from "./client";

export const notificationsApi = {
    getAll: () => apiClient.get("/Notifications"),
    getUnreadCount: () => apiClient.get("/Notifications/unread-count"),
    markRead: (id) => apiClient.patch(`/Notifications/${id}/read`, {}),
    markAllRead: () => apiClient.patch("/Notifications/read-all", {}),
};