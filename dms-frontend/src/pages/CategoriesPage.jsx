import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { categoriesApi } from "../api/categoriesApi";
import { documentsApi } from "../api/documentsApi";

export function CategoriesPage() {
    const [categories, setCategories] = useState([]);
    const [documents, setDocuments] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const [name, setName] = useState("");
    const [parentId, setParentId] = useState("");
    const [creating, setCreating] = useState(false);

    const [expanded, setExpanded] = useState({});

    useEffect(() => {
        loadData();
    }, []);

    async function loadData() {
        setLoading(true);
        setError("");
        try {
            const [cats, docs] = await Promise.all([
                categoriesApi.getAll(),
                documentsApi.getAll(),
            ]);
            setCategories(cats);
            setDocuments(docs);
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
            await categoriesApi.create({
                name,
                parentId: parentId ? Number(parentId) : null,
            });
            setName("");
            setParentId("");
            await loadData();
        } catch (err) {
            setError(err.message);
        } finally {
            setCreating(false);
        }
    }

    async function handleDelete(id) {
        const docCount = documents.filter((d) => d.categoryId === id).length;
        const warning =
            docCount > 0
                ? `This category has ${docCount} document(s) in it. Delete anyway?`
                : "Delete this category?";
        if (!confirm(warning)) return;

        try {
            await categoriesApi.delete(id);
            setCategories((prev) => prev.filter((c) => c.id !== id));
        } catch (err) {
            setError(err.message);
        }
    }

    function toggleExpanded(categoryId) {
        setExpanded((prev) => ({ ...prev, [categoryId]: !prev[categoryId] }));
    }

    function documentsForCategory(categoryId) {
        return documents.filter((d) => d.categoryId === categoryId);
    }

    function getParentName(parentId) {
        if (!parentId) return null;
        const parent = categories.find((c) => c.id === parentId);
        return parent ? parent.name : "Unknown";
    }

    const topLevel = categories.filter((c) => !c.parentId);
    const subCategories = categories.filter((c) => c.parentId);

    if (loading) return <p className="page-message">Loading categories...</p>;

    return (
        <div className="categories-page">
            <h1>Categories</h1>

            {error && <p className="error-text">{error}</p>}

            <form onSubmit={handleCreate} className="create-category-form">
                <input
                    type="text"
                    placeholder="Category name"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    required
                />
                <select value={parentId} onChange={(e) => setParentId(e.target.value)}>
                    <option value="">No parent (top-level)</option>
                    {topLevel.map((c) => (
                        <option key={c.id} value={c.id}>
                            {c.name}
                        </option>
                    ))}
                </select>
                <button type="submit" disabled={creating}>
                    {creating ? "Creating..." : "New Category"}
                </button>
            </form>

            {categories.length === 0 ? (
                <p>No categories yet.</p>
            ) : (
                <div className="category-groups">
                    {topLevel.map((parent) => {
                        const parentDocs = documentsForCategory(parent.id);
                        return (
                            <div key={parent.id} className="category-group">
                                <div
                                    className="category-row category-parent"
                                    onClick={() => toggleExpanded(parent.id)}
                                >
                                    <span>
                                        {parentDocs.length > 0 && (
                                            <span className="category-toggle">
                                                {expanded[parent.id] ? "▾" : "▸"}
                                            </span>
                                        )}
                                        {parent.name}{" "}
                                        <span className="category-doc-count">
                                            {parentDocs.length}
                                        </span>
                                    </span>
                                    <button
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            handleDelete(parent.id);
                                        }}
                                    >
                                        Delete
                                    </button>
                                </div>

                                {expanded[parent.id] && parentDocs.length > 0 && (
                                    <ul className="category-documents">
                                        {parentDocs.map((doc) => (
                                            <li key={doc.id}>
                                                <Link to={`/documents/${doc.id}`}>{doc.title}</Link>
                                                <span className={`status status-${doc.status?.toLowerCase()}`}>
                                                    {doc.status}
                                                </span>
                                            </li>
                                        ))}
                                    </ul>
                                )}

                                {subCategories
                                    .filter((sub) => sub.parentId === parent.id)
                                    .map((sub) => {
                                        const subDocs = documentsForCategory(sub.id);
                                        return (
                                            <div key={sub.id}>
                                                <div
                                                    className="category-row category-child"
                                                    onClick={() => toggleExpanded(sub.id)}
                                                >
                                                    <span>
                                                        {subDocs.length > 0 && (
                                                            <span className="category-toggle">
                                                                {expanded[sub.id] ? "▾" : "▸"}
                                                            </span>
                                                        )}
                                                        {sub.name}{" "}
                                                        <span className="category-doc-count">
                                                            {subDocs.length}
                                                        </span>
                                                    </span>
                                                    <button
                                                        onClick={(e) => {
                                                            e.stopPropagation();
                                                            handleDelete(sub.id);
                                                        }}
                                                    >
                                                        Delete
                                                    </button>
                                                </div>
                                                {expanded[sub.id] && subDocs.length > 0 && (
                                                    <ul className="category-documents">
                                                        {subDocs.map((doc) => (
                                                            <li key={doc.id}>
                                                                <Link to={`/documents/${doc.id}`}>{doc.title}</Link>
                                                                <span className={`status status-${doc.status?.toLowerCase()}`}>
                                                                    {doc.status}
                                                                </span>
                                                            </li>
                                                        ))}
                                                    </ul>
                                                )}
                                            </div>
                                        );
                                    })}
                            </div>
                        );
                    })}

                    {subCategories
                        .filter((sub) => !topLevel.some((p) => p.id === sub.parentId))
                        .map((sub) => (
                            <div key={sub.id} className="category-group">
                                <div className="category-row category-child">
                                    <span>
                                        {sub.name} (parent: {getParentName(sub.parentId)})
                                    </span>
                                    <button onClick={() => handleDelete(sub.id)}>Delete</button>
                                </div>
                            </div>
                        ))}
                </div>
            )}
        </div>
    );
}