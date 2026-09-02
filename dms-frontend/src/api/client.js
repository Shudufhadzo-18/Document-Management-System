const BASE_URL = "https://localhost:7071/api";

async function request(endpoint, options = {}) {
    const response = await fetch(`${BASE_URL}${endpoint}`, {
        ...options,
        credentials: "include", // sends the ASP.NET Identity auth cookie on every request
        headers: {
            ...(options.body instanceof FormData ? {} : { "Content-Type": "application/json" }),
            ...options.headers,
        },
    });

    if (!response.ok) {
        let message = `Request failed with status ${response.status}`;
        try {
            const errorBody = await response.json();
            message = errorBody.message || errorBody.title || message;
        } catch {
            // response wasn't JSON — keep the generic message
        }
        throw new Error(message); 
    }

    // 204 No Content has no body to parse
    if (response.status === 204) return null;

    return response.json();
}

// export const apiClient = {
//     get: (endpoint) => request(endpoint, { method: "GET" }),
//     post: (endpoint, body) =>
//         request(endpoint, {
//             method: "POST",
//             body: body instanceof FormData ? body : JSON.stringify(body),
//         }),
//     delete: (endpoint) => request(endpoint, { method: "DELETE" }),
// };

export const apiClient = {
    get: (endpoint) => request(endpoint, { method: "GET" }),
    post: (endpoint, body) =>
        request(endpoint, {
            method: "POST",
            body: body instanceof FormData ? body : JSON.stringify(body),
        }),
    patch: (endpoint, body) =>
        request(endpoint, { method: "PATCH", body: JSON.stringify(body) }),
    delete: (endpoint) => request(endpoint, { method: "DELETE" }),
};