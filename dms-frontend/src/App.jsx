import { Routes, Route, Navigate } from "react-router-dom";
import { Navbar } from "./components/Navbar";
import { LoginPage } from "./pages/LoginPage";
import { RegisterPage } from "./pages/RegisterPage";
import { DocumentsPage } from "./pages/DocumentsPage";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { DocumentDetailPage } from "./pages/DocumentDetailPage";
import { CategoriesPage } from "./pages/CategoriesPage";

function App() {
    return (
        <>
            <Navbar />
        <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route
                path="/documents"
                element={
                    <ProtectedRoute>
                        <DocumentsPage />
                    </ProtectedRoute>
                }
            />
            <Route path="/" element={<Navigate to="/documents" replace />} />

            <Route
                path="/documents/:id"
                element={
                    <ProtectedRoute>
                        <DocumentDetailPage />
                    </ProtectedRoute>
                }
                />
                <Route
                    path="/categories"
                    element={
                        <ProtectedRoute>
                            <CategoriesPage />
                        </ProtectedRoute>
                    }
                />
            </Routes>
        </>
    );
}

export default App;