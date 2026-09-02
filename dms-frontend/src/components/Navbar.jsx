import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export function Navbar() {
    const { user, isAuthenticated, logout } = useAuth();
    const navigate = useNavigate();

    async function handleLogout() {
        await logout();
        navigate("/login");
    }

    // Don't show the navbar on login/register pages — nothing to navigate to yet
    if (!isAuthenticated) return null;

    return (
        <nav className="navbar">
            <div className="navbar-brand">
                <Link to="/documents">SITA DMS</Link>
            </div>
            <div className="navbar-links">
                <Link to="/documents">Documents</Link>
                    <Link to="/categories">Categories</Link>
               
            </div>
            <div className="navbar-user">
                <span>{user?.email}</span>
                <button onClick={handleLogout}>Log Out</button>
            </div>
        </nav>
    );
}