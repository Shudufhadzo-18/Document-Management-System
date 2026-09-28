/* eslint-disable no-unused-vars */
import { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import { documentsApi } from "../api/documentsApi";
import { documentVersionsApi } from "../api/documentVersionsApi";
import { apiClient } from "../api/client";
import { PdfPreviewModal } from "../components/PdfPreviewModal";
import { documentPermissionsApi } from "../api/documentPermissionsApi";
import { usersApi } from "../api/usersApi";
import { documentCommentsApi } from "../api/documentCommentsApi";
import { useAuth } from "../context/AuthContext";


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

    // inside the component:
    const { user } = useAuth();
    const [comments, setComments] = useState([]);
    const [newComment, setNewComment] = useState("");
    const [postingComment, setPostingComment] = useState(false);

    // with your other state:
    const [permissions, setPermissions] = useState([]);
    const [allUsers, setAllUsers] = useState([]);
    const [grantUserId, setGrantUserId] = useState("");
    const [grantRights, setGrantRights] = useState({
        canView: true,
        canDownload: false,
        canEdit: false,
        canDelete: false,
        canApprove: false,
    });

    useEffect(() => {
        loadData();
    }, [id]);

    async function loadData() {
        const commentList = await documentCommentsApi.getForDocument(id);
        setComments(commentList);
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
            }


            catch {
                setAuditLogs(null); // null = not permitted to view, distinct from [] = no entries yet
            }
        }

        catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }

        try {
            const perms = await documentPermissionsApi.getForDocument(id);
            setPermissions(perms);
            const users = await usersApi.getAll();
            setAllUsers(users);
        } catch {
            setPermissions(null); // not an Admin — hide the section entirely
        }
    }

    const [statusUpdating, setStatusUpdating] = useState(false);

    const statusOptions = {
        Draft: ["Submitted"],
        Submitted: ["UnderReview"],
        UnderReview: ["Approved", "Rejected"],
        Approved: ["Published"],
        Rejected: ["Draft"],
        Published: ["Archived", "Expired"],
        Expired: ["Archived"],
        Archived: ["Published"],
    };
    async function handlePostComment(e) {
        e.preventDefault();
        if (!newComment.trim()) return;
        setPostingComment(true);
        setError("");
        try {
            await documentCommentsApi.add(id, newComment);
            setNewComment("");
            const commentList = await documentCommentsApi.getForDocument(id);
            setComments(commentList);
        } catch (err) {
            setError(err.message);
        } finally {
            setPostingComment(false);
        }
    }

    async function handleDeleteComment(commentId) {
        if (!confirm("Delete this comment?")) return;
        try {
            await documentCommentsApi.delete(id, commentId);
            setComments((prev) => prev.filter((c) => c.id !== commentId));
        } catch (err) {
            setError(err.message);
        }
    }

    async function handleStatusChange(newStatus) {
        let reason = null;

        if (newStatus === "Rejected") {
            reason = prompt("Reason for rejection (optional):");
            if (reason === null) return; // user cancelled the prompt
        }

        const label = newStatus === "Published" && document.status === "Approved"
            ? "publish"
            : newStatus.toLowerCase();

        if (!confirm(`Change status to "${newStatus}"?`)) return;

        setStatusUpdating(true);
        setError("");
        try {
            await documentsApi.updateStatus(id, newStatus, reason);
            await loadData();
        } catch (err) {
            setError(err.message);
        } finally {
            setStatusUpdating(false);
        }
    }

    async function handleGrantPermission(e) {
        e.preventDefault();
        if (!grantUserId) return;
        try {
            await documentPermissionsApi.grant(id, {
                userId: grantUserId,
                ...grantRights,
            });
            setGrantUserId("");
            setGrantRights({ canView: true, canDownload: false, canEdit: false, canDelete: false, canApprove: false });
            await loadData();
        } catch (err) {
            setError(err.message);
        }
    }

    async function handleRevokePermission(userId) {
        if (!confirm("Revoke this user's access?")) return;
        try {
            await documentPermissionsApi.revoke(id, userId);
            await loadData();
        } catch (err) {
            setError(err.message);
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
            {document.description && <p className="doc-description">{document.description}</p>}

            <div className="detail-meta">
                <span className={`status status-${document.status?.toLowerCase()}`}>
                    {document.status}
                </span>
                {document.documentType && <span>Type: {document.documentType}</span>}
                <span>Category: {document.categoryName}</span>
                {document.departmentName && <span>Department: {document.departmentName}</span>}
                <span>Owner: {document.ownerName}</span>
                <span>Current version: {document.currentVersionNumber ?? "None"}</span>
                {document.effectiveDate && (
                    <span>Effective: {new Date(document.effectiveDate).toLocaleDateString()}</span>
                )}
                {document.reviewDate && (
                    <span>Review due: {new Date(document.reviewDate).toLocaleDateString()}</span>
                )}
            </div>

            {document.tags && (
                <div className="tag-list">
                    {document.tags.split(",").map((tag) => (
                        <span key={tag.trim()} className="tag-chip">{tag.trim()}</span>
                    ))}
                </div>
            )}

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

            <section className="comments-section">
                <h2>Comments</h2>

                {comments.length === 0 ? (
                    <p>No comments yet.</p>
                ) : (
                    <ul className="comment-list">
                        {comments.map((c) => (
                            <li key={c.id} className="comment-item">
                                <div className="comment-header">
                                    <span className="comment-author">{c.userEmail}</span>
                                    <span className="comment-date">
                                        {new Date(c.createdAt).toLocaleString()}
                                    </span>
                                    {(c.userId === user?.id || user?.roles?.includes("Admin")) && (
                                        <button
                                            className="comment-delete"
                                            onClick={() => handleDeleteComment(c.id)}
                                        >
                                            Delete
                                        </button>
                                    )}
                                </div>
                                <p className="comment-content">{c.content}</p>
                            </li>
                        ))}
                    </ul>
                )}

                <form onSubmit={handlePostComment} className="comment-form">
                    <textarea
                        placeholder="Add a comment..."
                        value={newComment}
                        onChange={(e) => setNewComment(e.target.value)}
                        rows={2}
                        required
                    />
                    <button type="submit" disabled={postingComment}>
                        {postingComment ? "Posting..." : "Post Comment"}
                    </button>
                </form>
            </section>

            {permissions && (
                <section className="permissions-section">
                    <h2>Document Permissions</h2>

                    {permissions.length === 0 ? (
                        <p>No specific permissions granted — access follows role-based rules only.</p>
                    ) : (
                        <table className="versions-table">
                            <thead>
                                <tr>
                                    <th>User</th>
                                    <th>View</th>
                                    <th>Download</th>
                                    <th>Edit</th>
                                    <th>Delete</th>
                                    <th>Approve</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                {permissions.map((p) => (
                                    <tr key={p.id}>
                                        <td>{p.userEmail}</td>
                                        <td>{p.canView ? "✓" : ""}</td>
                                        <td>{p.canDownload ? "✓" : ""}</td>
                                        <td>{p.canEdit ? "✓" : ""}</td>
                                        <td>{p.canDelete ? "✓" : ""}</td>
                                        <td>{p.canApprove ? "✓" : ""}</td>
                                        <td>
                                            <button onClick={() => handleRevokePermission(p.userId)}>Revoke</button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}

                    <form onSubmit={handleGrantPermission} className="grant-permission-form">
                        <select value={grantUserId} onChange={(e) => setGrantUserId(e.target.value)} required>
                            <option value="">Select user</option>
                            {allUsers.map((u) => (
                                <option key={u.id} value={u.id}>{u.email}</option>
                            ))}
                        </select>
                        {["canView", "canDownload", "canEdit", "canDelete", "canApprove"].map((right) => (
                            <label key={right} className="permission-checkbox">
                                <input
                                    type="checkbox"
                                    checked={grantRights[right]}
                                    onChange={(e) =>
                                        setGrantRights((prev) => ({ ...prev, [right]: e.target.checked }))
                                    }
                                />
                                {right.replace("can", "")}
                            </label>
                        ))}
                        <button type="submit">Grant</button>
                    </form>
                </section>
            )}

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