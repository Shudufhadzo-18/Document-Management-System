import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { documentsApi } from "../api/documentsApi";

export function ReviewDashboardPage() {
    const [reviewDue, setReviewDue] = useState([]);
    const [expired, setExpired] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        loadData();
    }, []);

    async function loadData() {
        setLoading(true);
        setError("");
        try {
            const [due, exp] = await Promise.all([
                documentsApi.getReviewDue(),
                documentsApi.getExpired(),
            ]);
            setReviewDue(due);
            setExpired(exp);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }

    if (loading) return <p className="page-message">Loading...</p>;

    return (
        <div className="documents-page">
            <h1>Review &amp; Expiry</h1>
            {error && <p className="error-text">{error}</p>}

            <h2 className="review-section-heading">
                Overdue ({expired.length})
            </h2>
            {expired.length === 0 ? (
                <p>Nothing overdue.</p>
            ) : (
                <ul className="document-list">
                    {expired.map((doc) => (
                        <li key={doc.id}>
                            <Link to={`/documents/${doc.id}`}>{doc.title}</Link>
                            <span className="review-due-badge">
                                Was due {new Date(doc.reviewDate).toLocaleDateString()}
                            </span>
                            <span className={`status status-${doc.status?.toLowerCase()}`}>
                                {doc.status}
                            </span>
                        </li>
                    ))}
                </ul>
            )}

            <h2 className="review-section-heading">
                Due Within 30 Days ({reviewDue.length})
            </h2>
            {reviewDue.length === 0 ? (
                <p>Nothing due soon.</p>
            ) : (
                <ul className="document-list">
                    {reviewDue.map((doc) => (
                        <li key={doc.id}>
                            <Link to={`/documents/${doc.id}`}>{doc.title}</Link>
                            <span className="doc-type-badge">
                                Due {new Date(doc.reviewDate).toLocaleDateString()}
                            </span>
                            <span className={`status status-${doc.status?.toLowerCase()}`}>
                                {doc.status}
                            </span>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}