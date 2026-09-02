import { createContext, useContext, useState, useEffect } from "react";
import { authApi } from "../api/authApi";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    // On first mount, check if the auth cookie from a previous session is
    // still valid — this is what keeps someone logged in after a page refresh
    useEffect(() => {
        async function checkSession() {
            try {
                const currentUser = await authApi.me();
                setUser(currentUser);
            } catch {
                // /Auth/me returns 401 if not logged in — that's expected, not an error to show
                setUser(null);
            } finally {
                setLoading(false);
            }
        }

        checkSession();
    }, []);

    async function login(email, password) {
        await authApi.login(email, password);
        const currentUser = await authApi.me();
        setUser(currentUser);
    }

    async function register(email, password) {
        await authApi.register(email, password);
        const currentUser = await authApi.me();
        setUser(currentUser);
    }

    async function logout() {
        await authApi.logout();
        setUser(null);
    }

    const value = {
        user,
        isAuthenticated: !!user,
        loading,
        login,
        register,
        logout,

    };

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error("useAuth must be used within an AuthProvider");
    }
    return context;
}