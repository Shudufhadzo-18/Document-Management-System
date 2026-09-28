import { useState, useEffect } from "react";
import { usersApi } from "../api/usersApi";

export function UsersPage() {
    const [users, setUsers] = useState([]);
    const [availableRoles, setAvailableRoles] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [selectedRole, setSelectedRole] = useState({}); // per-user dropdown selection, keyed by user id

    useEffect(() => {
        loadData();
    }, []);

    async function loadData() {
        setLoading(true);
        setError("");
        try {
            const [userList, roles] = await Promise.all([
                usersApi.getAll(),
                usersApi.getAllRoles(),
            ]);
            setUsers(userList);
            setAvailableRoles(roles);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }

    async function handleAssign(userId) {
        const role = selectedRole[userId];
        if (!role) return;
        setError("");
        try {
            await usersApi.assignRole(userId, role);
            await loadData();
        } catch (err) {
            setError(err.message);
        }
    }

    async function handleRemove(userId, role) {
        if (!confirm(`Remove "${role}" from this user?`)) return;
        setError("");
        try {
            await usersApi.removeRole(userId, role);
            await loadData();
        } catch (err) {
            setError(err.message);
        }
    }

    if (loading) return <p className="page-message">Loading users...</p>;

    return (
        <div className="categories-page">
            <h1>Users &amp; Roles</h1>

            {error && <p className="error-text">{error}</p>}

            <div className="category-groups">
                {users.map((user) => {
                    // roles this user doesn't already have — that's what the dropdown should offer
                    const assignableRoles = availableRoles.filter(
                        (r) => !user.roles.includes(r)
                    );

                    return (
                        <div key={user.id} className="category-group">
                            <div className="category-row category-parent">
                                <span>{user.email}</span>
                            </div>
                            <div className="category-row category-child user-roles-row">
                                <div className="role-chips">
                                    {user.roles.length === 0 ? (
                                        <span className="no-roles-text">No roles assigned</span>
                                    ) : (
                                        user.roles.map((role) => (
                                            <span key={role} className="role-chip">
                                                {role}
                                                <button
                                                    className="role-chip-remove"
                                                    onClick={() => handleRemove(user.id, role)}
                                                    title={`Remove ${role}`}
                                                >
                                                    &times;
                                                </button>
                                            </span>
                                        ))
                                    )}
                                </div>

                                {assignableRoles.length > 0 && (
                                    <div className="assign-role-controls">
                                        <select
                                            value={selectedRole[user.id] || ""}
                                            onChange={(e) =>
                                                setSelectedRole((prev) => ({
                                                    ...prev,
                                                    [user.id]: e.target.value,
                                                }))
                                            }
                                        >
                                            <option value="">Add role...</option>
                                            {assignableRoles.map((r) => (
                                                <option key={r} value={r}>
                                                    {r}
                                                </option>
                                            ))}
                                        </select>
                                        <button onClick={() => handleAssign(user.id)}>Add</button>
                                    </div>
                                )}
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}