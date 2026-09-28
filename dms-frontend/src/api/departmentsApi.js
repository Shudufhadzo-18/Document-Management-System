import { apiClient } from "./client";

export const departmentsApi = {
    getAll: () => apiClient.get("/Departments"),
    getById: (id) => apiClient.get(`/Departments/${id}`),
    create: (dto) => apiClient.post("/Departments", dto),
    delete: (id) => apiClient.delete(`/Departments/${id}`),
};