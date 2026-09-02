/* eslint-disable no-unused-vars */
import { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import { documentsApi } from "../api/documentsApi";
import { documentVersionsApi } from "../api/documentVersionsApi";
import { apiClient } from "../api/client";
import { PdfPreviewModal } from "../components/PdfPreviewModal";


export function DocumentDetailPage() {
    const { id } = useParams();
    const [document, setDocument] = useState(null);
    const [versions, setVersions] = useState([]);
    const [auditLogs, setAuditLogs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const [previewVersion, setPreviewVersion] = useState(null);
   

    // upload form state
    const [file, setFile] = useState(null);
    const [notes, setNotes] = useState("");
    const [uploading, setUploading] = useState(false);

    useEffect(() => {
        loadData();
    }, [id]);

    async function loadData() {
        setLoading(true);
        setError("");
        try {
            const doc = await documentsApi.getById(id);
            const vers = await documentVersionsApi.getVersions(id);
            setDocument(doc);
            setVersions(vers);

            // audit logs are role-restricted (Admin/ComplianceOfficer) — a
            // regular user will get a 403 here, which we treat as "just don't show it"
            try {
                const logs = await apiClient.get(`/documents/${id}/audit-logs`);
                setAuditLogs(logs);
            } catch {
                setAuditLogs(null); // null = not permitted to view, distinct from [] = no entries yet
            }
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }

    const [statusUpdating, setStatusUpdating] = useState(false);

    const statusOptions = {
        Draft: ["Active"],
        Active: ["Archived", "Draft"],
        Archived: ["Active"],
    };

    async function handleStatusChange(newStatus) {
        if (!confirm(`Change status to "${newStatus}"?`)) return;
        setStatusUpdating(true);
        setError("");
        try {
            await documentsApi.updateStatus(id, newStatus);
            await loadData(); // refresh to show the new status + new audit entry
        } catch (err) {
            setError(err.message);
        } finally {
            setStatusUpdating(false);
        }
    }

    async function handleUpload(e) {
        e.preventDefault();
        if (!file) {
            setError("Choose a file first.");
            return;
        }
        setUploading(true);
        setError("");
        try {
            await documentVersionsApi.upload(id, file, notes);
            setFile(null);
            setNotes("");
            await loadData(); // refresh version list + document's current version number
        } catch (err) {
            setError(err.message);
        } finally {
            setUploading(false);
        }
    }

    if (loading) return <p className="page-message">Loading document...</p>;
    if (!document) return <p className="page-message">Document not found.</p>;

    return (
        <div className="detail-page">
            <Link to="/documents" className="back-link">
                &larr; Back to documents
            </Link>

            <h1>{document.title}</h1>

            <div className="detail-meta">
                <span className={`status status-${document.status?.toLowerCase()}`}>
                    {document.status}
                </span>
                <span>Category: {document.categoryName}</span>
                <span>Owner: {document.ownerName}</span>
                <span>Current version: {document.currentVersionNumber ?? "None"}</span>
            </div>

            <div className="status-actions">
                {statusOptions[document.status]?.map((nextStatus) => (
                    <button
                        key={nextStatus}
                        onClick={() => handleStatusChange(nextStatus)}
                        disabled={statusUpdating}
                        className={`status-action-btn status-action-${nextStatus.toLowerCase()}`}
                    >
                        Move to {nextStatus}
                    </button>
                ))}
            </div>

            {error && <p className="error-text">{error}</p>}

            <section className="upload-section">
                <h2>Upload New Version</h2>
                <form onSubmit={handleUpload} className="upload-form">
                    <input
                        type="file"
                        onChange={(e) => setFile(e.target.files[0])}
                        required
                    />
                    <input
                        type="text"
                        placeholder="Notes (what changed?)"
                        value={notes}
                        onChange={(e) => setNotes(e.target.value)}
                    />
                    <button type="submit" disabled={uploading}>
                        {uploading ? "Uploading..." : "Upload"}
                    </button>
                </form>
            </section>

            <section className="versions-section">
                <h2>Version History</h2>
                {versions.length === 0 ? (
                    <p>No versions uploaded yet.</p>
                ) : (
                    <table className="versions-table">
                        <thead>
                            <tr>
                                <th>Version</th>
                                <th>File</th>
                                <th>Uploaded By</th>
                                <th>Date</th>
                                <th>Notes</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {versions.map((v) => (
                                <tr key={v.id}>
                                    <td>{v.versionNumber}</td>
                                    <td>{v.fileName}</td>
                                    <td>{v.uploadedByName}</td>
                                    <td>{new Date(v.uploadedAt).toLocaleString()}</td>
                                    <td>{v.notes}</td>
                                    <td>
                                        {v.mimeType === "application/pdf" ? (
                                            <button
                                                className="preview-link"
                                                onClick={() => setPreviewVersion(v)}
                                            >
                                                Preview
                                            </button>
                                        ) : null}
                                        {" "}

                                       <a href={documentVersionsApi.getDownloadUrl(id, v.id)}
                                        className="download-link"
                                        target="_blank"
                                        rel="noopener noreferrer"
  >
                                        Download
                                    </a>
                                </td>
      </tr>
    ))}
                    </tbody>
</table>
                )}
            </section>

            {auditLogs && (
                <section className="audit-section">
                    <h2>Audit Trail</h2>
                    {auditLogs.length === 0 ? (
                        <p>No audit entries yet.</p>
                    ) : (
                        <ul className="audit-list">
                            {auditLogs.map((log) => (
                                <li key={log.id}>
                                    <strong>{log.action}</strong> by {log.userName} —{" "}
                                    {new Date(log.timestamp).toLocaleString()}
                                </li>
                            ))}
                        </ul>
                    )}
                </section>
            )}

            {previewVersion && (
                <PdfPreviewModal
                    fileUrl={documentVersionsApi.getDownloadUrl(id, previewVersion.id)}
                    fileName={previewVersion.fileName}
                    onClose={() => setPreviewVersion(null)}
                />
            )}
        </div>
    );
}