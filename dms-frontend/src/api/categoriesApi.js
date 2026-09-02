import { apiClient } from "./client";

export const categoriesApi = {
    getAll: () => apiClient.get("/Categories"),
    getById: (id) => apiClient.get(`/Categories/${id}`),
    create: (dto) => apiClient.post("/Categories", dto),
    delete: (id) => apiClient.delete(`/Categories/${id}`),
};