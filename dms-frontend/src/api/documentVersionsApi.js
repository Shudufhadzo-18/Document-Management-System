import { apiClient } from "./client";

// export const documentVersionsApi = {
//     getVersions: (documentId) => apiClient.get(`/documents/${documentId}/versions`),

//     upload: (documentId, file, notes) => {
//         const formData = new FormData();
//         formData.append("file", file);
//         formData.append("notes", notes || "");
//         documentId also needs to be in the form body since your
//         DocumentVersionUploadDto binds via [FromForm]
//         formData.append("documentId", documentId);

//         return apiClient.post(`/documents/${documentId}/versions`, formData);
//     },
// };

export const documentVersionsApi = {
    getVersions: (documentId) => apiClient.get(`/documents/${documentId}/versions`),

    upload: (documentId, file, notes) => {
        const formData = new FormData();
        formData.append("file", file);
        formData.append("notes", notes || "");
        formData.append("documentId", documentId);
        return apiClient.post(`/documents/${documentId}/versions`, formData);
    },

    // Returns the direct URL — browsers handle file downloads best via a
    // real navigation/anchor click, not a fetch() call, so this isn't
    // routed through apiClient like the others
    getDownloadUrl: (documentId, versionId) =>
        `https://localhost:7071/api/documents/${documentId}/versions/${versionId}/download`,
};