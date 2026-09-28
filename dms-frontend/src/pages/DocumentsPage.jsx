import { useState, useEffect, useMemo } from "react";
import { Link } from "react-router-dom";
import { documentsApi } from "../api/documentsApi";
import { categoriesApi } from "../api/categoriesApi";
import { departmentsApi } from "../api/departmentsApi";



export function DocumentsPage() {
    const [documents, setDocuments] = useState([]);
    const [categories, setCategories] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    // with your other state:
    const [departments, setDepartments] = useState([]);
    const [departmentId, setDepartmentId] = useState("");

    const [description, setDescription] = useState("");
    const [documentType, setDocumentType] = useState("");
    const [tags, setTags] = useState("");
    const [effectiveDate, setEffectiveDate] = useState("");
    const [reviewDate, setReviewDate] = useState("");



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
            const [docs, cats, depts] = await Promise.all([
                documentsApi.getAll(),
                categoriesApi.getAll(),
                departmentsApi.getAll(),
            ]);
            setDocuments(docs);
            setCategories(cats);
            setDepartments(depts);
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
            await documentsApi.create({
                title,
                categoryId: Number(categoryId),
                departmentId: departmentId ? Number(departmentId) : null,
                description: description || null,
                documentType: documentType || null,
                tags: tags || null,
                effectiveDate: effectiveDate || null,
                reviewDate: reviewDate || null,
            });
            setTitle("");
            setCategoryId("");
            setDepartmentId("");
            setDescription("");
            setDocumentType("");
            setTags("");
            setEffectiveDate("");
            setReviewDate("");
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
    function isReviewDueSoon(reviewDate) {
        if (!reviewDate) return false;
        const daysUntil = (new Date(reviewDate) - new Date()) / (1000 * 60 * 60 * 24);
        return daysUntil <= 30 && daysUntil >= 0;
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

            <form onSubmit={handleCreate} className="create-document-form-expanded">
                <div className="form-row">
                    <input
                        type="text"
                        placeholder="Document title"
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                        required
                    />
                    <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)} required>
                        <option value="">Select category</option>
                        {categories.map((c) => (
                            <option key={c.id} value={c.id}>{c.name}</option>
                        ))}
                    </select>
                    <select value={departmentId} onChange={(e) => setDepartmentId(e.target.value)}>
                        <option value="">No department</option>
                        {departments.map((d) => (
                            <option key={d.id} value={d.id}>{d.name}</option>
                        ))}
                    </select>
                </div>

                <div className="form-row">
                    <select value={documentType} onChange={(e) => setDocumentType(e.target.value)}>
                        <option value="">Document type</option>
                        <option value="Policy">Policy</option>
                        <option value="Report">Report</option>
                        <option value="Form">Form</option>
                        <option value="Contract">Contract</option>
                        <option value="Certificate">Certificate</option>
                        <option value="Other">Other</option>
                    </select>
                    <input
                        type="text"
                        placeholder="Tags (comma-separated)"
                        value={tags}
                        onChange={(e) => setTags(e.target.value)}
                    />
                </div>

                <div className="form-row">
                    <label className="date-field">
                        Effective Date
                        <input
                            type="date"
                            value={effectiveDate}
                            onChange={(e) => setEffectiveDate(e.target.value)}
                        />
                    </label>
                    <label className="date-field">
                        Review Date
                        <input
                            type="date"
                            value={reviewDate}
                            onChange={(e) => setReviewDate(e.target.value)}
                        />
                    </label>
                </div>

                <textarea
                    placeholder="Description (optional)"
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    rows={2}
                />

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
                            {doc.documentType && <span className="doc-type-badge">{doc.documentType}</span>}
                            {isReviewDueSoon(doc.reviewDate) && (
                                <span className="review-due-badge">Review due</span>
                            )}
                            <span className={`status status-${doc.status?.toLowerCase()}`}>{doc.status}</span>
                            {<button onClick={() => handleDelete(doc.id)}>Delete</button>}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
 }
// canDelete && 