
import { apiClient } from "./client";

export const authApi = {
    register: (email, password) =>
        apiClient.post("/Auth/register", { email, password }),

    login: (email, password) =>
        apiClient.post("/Auth/login", { email, password }),

    logout: () => apiClient.post("/Auth/logout", {}),

    // used on app load to check if the cookie is still valid and who's logged in
    me: () => apiClient.get("/Auth/me"),
};