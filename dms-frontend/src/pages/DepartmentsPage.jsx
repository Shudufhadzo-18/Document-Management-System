import { useState, useEffect } from "react";
import { departmentsApi } from "../api/departmentsApi";

export function DepartmentsPage() {
    const [departments, setDepartments] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [creating, setCreating] = useState(false);

    useEffect(() => {
        loadDepartments();
    }, []);

    async function loadDepartments() {
        setLoading(true);
        setError("");
        try {
            const data = await departmentsApi.getAll();
            setDepartments(data);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }

    async function handleCreate(e) {
        e.preventDefault();
        setCreating(true);
        setError("");
        try {
            await departmentsApi.create({ name, description });
            setName("");
            setDescription("");
            await loadDepartments();
        } catch (err) {
            setError(err.message);
        } finally {
            setCreating(false);
        }
    }

    async function handleDelete(id) {
        if (!confirm("Delete this department? Documents inside it will be unassigned, not deleted.")) return;
        try {
            await departmentsApi.delete(id);
            setDepartments((prev) => prev.filter((d) => d.id !== id));
        } catch (err) {
            setError(err.message);
        }
    }

    if (loading) return <p className="page-message">Loading departments...</p>;

    return (
        <div className="categories-page">
            <h1>Departments</h1>

            {error && <p className="error-text">{error}</p>}

            <form onSubmit={handleCreate} className="create-category-form">
                <input
                    type="text"
                    placeholder="Department name"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    required
                />
                <input
                    type="text"
                    placeholder="Description (optional)"
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                />
                <button type="submit" disabled={creating}>
                    {creating ? "Creating..." : "New Department"}
                </button>
            </form>

            {departments.length === 0 ? (
                <p>No departments yet.</p>
            ) : (
                <div className="category-groups">
                    {departments.map((dept) => (
                        <div key={dept.id} className="category-group">
                            <div className="category-row category-parent">
                                <span>
                                    {dept.name}{" "}
                                    <span className="category-doc-count">{dept.documentCount}</span>
                                </span>
                                <button onClick={() => handleDelete(dept.id)}>Delete</button>
                            </div>
                            {dept.description && (
                                <div className="category-row category-child">
                                    <span>{dept.description}</span>
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}