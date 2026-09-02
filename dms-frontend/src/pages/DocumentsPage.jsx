import { useState, useEffect, useMemo } from "react";
import { Link } from "react-router-dom";
import { documentsApi } from "../api/documentsApi";
import { categoriesApi } from "../api/categoriesApi";

export function DocumentsPage() {
    const [documents, setDocuments] = useState([]);
    const [categories, setCategories] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");



    // create-document form state
    const [title, setTitle] = useState("");
    const [categoryId, setCategoryId] = useState("");
    const [creating, setCreating] = useState(false);

    // search/filter state
    const [searchText, setSearchText] = useState("");
    const [filterCategoryId, setFilterCategoryId] = useState("");
    <div className="filter-bar">
        <input
            type="text"
            placeholder="Search by title..."
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
        />
        <select
            value={filterCategoryId}
            onChange={(e) => setFilterCategoryId(e.target.value)}
        >
            <option value="">All categories</option>
            {categories.map((c) => (
                <option key={c.id} value={c.id}>
                    {c.name}
                </option>
            ))}
        </select>
    </div>

    useEffect(() => {
        loadData();
    }, []);

    async function loadData() {
        setLoading(true);
        setError("");
        try {
            const [docs, cats] = await Promise.all([
                documentsApi.getAll(),
                categoriesApi.getAll(),
            ]);
            setDocuments(docs);
            setCategories(cats);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }

    async function handleCreate(e) {
        e.preventDefault();
        if (!categoryId) {
            setError("Choose a category first.");
            return;
        }
        setCreating(true);
        setError("");
        try {
            await documentsApi.create({ title, categoryId: Number(categoryId) });
            setTitle("");
            setCategoryId("");
            await loadData();
        } catch (err) {
            setError(err.message);
        } finally {
            setCreating(false);
        }
    }

    async function handleDelete(id) {
        if (!confirm("Delete this document?")) return;
        try {
            await documentsApi.delete(id);
            setDocuments((prev) => prev.filter((d) => d.id !== id));
        } catch (err) {
            setError(err.message);
        }
    }

    // DocumentSummaryDto only has Id/Title/Status — no CategoryId — so
    // category filtering needs the category name to match against each
    // document's title-adjacent data isn't available client-side yet.
    // We filter by what's actually on the summary DTO: title + status.
    // const filteredDocuments = useMemo(() => {
    //     return documents.filter((doc) => {
    //         const matchesText = doc.title
    //             .toLowerCase()
    //             .includes(searchText.toLowerCase());
    //         return matchesText;
    //     });
    // }, [documents, searchText]);

    const filteredDocuments = useMemo(() => {
        return documents.filter((doc) => {
            const matchesText = doc.title
                .toLowerCase()
                .includes(searchText.toLowerCase());
            const matchesCategory =
                !filterCategoryId || doc.categoryId === Number(filterCategoryId);
            return matchesText && matchesCategory;
        });
    }, [documents, searchText, filterCategoryId]);
    

    if (loading) return <p className="page-message">Loading documents...</p>;

    return (
        <div className="documents-page">
            <h1>Documents</h1>

            {error && <p className="error-text">{error}</p>}

            <form onSubmit={handleCreate} className="create-document-form">
                <input
                    type="text"
                    placeholder="Document title"
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                    required
                />
                <select
                    value={categoryId}
                    onChange={(e) => setCategoryId(e.target.value)}
                    required
                >
                    <option value="">Select category</option>
                    {categories.map((c) => (
                        <option key={c.id} value={c.id}>
                            {c.name}
                        </option>
                    ))}
                </select>
                <button type="submit" disabled={creating}>
                    {creating ? "Creating..." : "New Document"}
                </button>
            </form>

            <div className="filter-bar">
                <input
                    type="text"
                    placeholder="Search by title..."
                    value={searchText}
                    onChange={(e) => setSearchText(e.target.value)}
                />
            </div>

            {filteredDocuments.length === 0 ? (
                <p>No documents match your search.</p>
            ) : (
                <ul className="document-list">
                    {filteredDocuments.map((doc) => (
                        <li key={doc.id}>
                            <Link to={`/documents/${doc.id}`}>{doc.title}</Link>
                            <span className={`status status-${doc.status?.toLowerCase()}`}>
                                {doc.status}
                            </span>
                            <button onClick={() => handleDelete(doc.id)}>Delete</button>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}