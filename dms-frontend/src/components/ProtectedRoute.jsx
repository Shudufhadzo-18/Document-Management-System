import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

// Wraps any page that requires login. If the session check is still running,
// show nothing (avoids a flash of the login page for already-logged-in users).
// If it's done and there's no user, redirect to /login.
export function ProtectedRoute({ children }) {
    const { isAuthenticated, loading } = useAuth();

    if (loading) return null;

    if (!isAuthenticated) return <Navigate to="/login" replace />;

    return children;
}