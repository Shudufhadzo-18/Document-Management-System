import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { NotificationBell } from "./NotificationBell";


export function Navbar() {
    const { user, isAuthenticated, logout } = useAuth();
    const navigate = useNavigate();
    1
    async function handleLogout() {
        await logout();
        navigate("/login");
    }

    // Don't show the navbar on login/register pages — nothing to navigate to yet
    if (!isAuthenticated) return null;

    return (
        <nav className="navbar">
            <div className="navbar-brand">
                <Link to="/documents">SETA DMS</Link>
            </div>
            <div className="navbar-links">
                <Link to="/documents">Documents</Link>
                <Link to="/categories">Categories</Link>
                <Link to="/departments">Departments</Link>
                <Link to="/users">Users</Link>
                <Link to="/review">Review</Link>
               
            </div>
            <div className="navbar-user">
                <NotificationBell />
                <span>{user?.email}</span>
                <button onClick={handleLogout}>Log Out</button>
            </div>
        </nav>
    );
}