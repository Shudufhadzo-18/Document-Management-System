import { useState, useEffect } from "react";
import { Document, Page, pdfjs } from "react-pdf";
import "react-pdf/dist/Page/AnnotationLayer.css";
import "react-pdf/dist/Page/TextLayer.css";

// Load the PDF.js worker from a CDN matching the installed pdfjs version —
// avoids needing to configure Vite to bundle the worker file separately
pdfjs.GlobalWorkerOptions.workerSrc = `https://unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.mjs`;

export function PdfPreviewModal({ fileUrl, fileName, onClose }) {
    const [blobUrl, setBlobUrl] = useState(null);
    const [numPages, setNumPages] = useState(null);
    const [pageNumber, setPageNumber] = useState(1);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let objectUrl;

        async function loadFile() {
            setLoading(true);
            setError("");
            try {
                // fetch with credentials so the auth cookie is sent — react-pdf's
                // own internal fetch won't include cookies, so we can't just pass
                // fileUrl straight to <Document>
                const response = await fetch(fileUrl, { credentials: "include" });
                if (!response.ok) throw new Error("Failed to load file for preview.");

                const blob = await response.blob();
                objectUrl = URL.createObjectURL(blob);
                setBlobUrl(objectUrl);
            } catch (err) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        }

        loadFile();

        // revoke the object URL when the modal closes or the file changes,
        // otherwise each preview leaks memory
        return () => {
            if (objectUrl) URL.revokeObjectURL(objectUrl);
        };
    }, [fileUrl]);

    function handleLoadSuccess({ numPages }) {
        setNumPages(numPages);
        setPageNumber(1);
    }

    return (
        <div className="pdf-modal-overlay" onClick={onClose}>
            <div className="pdf-modal" onClick={(e) => e.stopPropagation()}>
                <div className="pdf-modal-header">
                    <span>{fileName}</span>
                    <button onClick={onClose} className="pdf-modal-close">
                        &times;
                    </button>
                </div>

                <div className="pdf-modal-body">
                    {loading && <p>Loading preview...</p>}
                    {error && <p className="error-text">{error}</p>}

                    {blobUrl && !error && (
                        <Document
                            file={blobUrl}
                            onLoadSuccess={handleLoadSuccess}
                            onLoadError={() => setError("Could not render this PDF.")}
                            loading="Rendering..."
                        >
                            <Page pageNumber={pageNumber} width={700} />
                        </Document>
                    )}
                </div>

                {numPages && (
                    <div className="pdf-modal-footer">
                        <button
                            onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                            disabled={pageNumber <= 1}
                        >
                            Previous
                        </button>
                        <span>
                            Page {pageNumber} of {numPages}
                        </span>
                        <button
                            onClick={() => setPageNumber((p) => Math.min(numPages, p + 1))}
                            disabled={pageNumber >= numPages}
                        >
                            Next
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
}