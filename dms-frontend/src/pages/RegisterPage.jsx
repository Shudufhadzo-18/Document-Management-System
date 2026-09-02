import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export function RegisterPage() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [submitting, setSubmitting] = useState(false);
    const { register } = useAuth();
    const navigate = useNavigate();

    async function handleSubmit(e) {
        e.preventDefault();
        setError("");
        setSubmitting(true);
        try {
            await register(email, password);
            navigate("/documents");
        } catch (err) {
            // your Identity CreateAsync returns validation errors (password too
            // weak, email taken, etc.) — err.message comes from apiClient's
            // errorBody.message/title parsing in client.js
            setError(err.message || "Registration failed. Try a stronger password.");
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div className="auth-page">
            <h1>Register</h1>
            <form onSubmit={handleSubmit}>
                <label>
                    Email
                    <input
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                    />
                </label>
                <label>
                    Password
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </label>
                {error && <p className="error-text">{error}</p>}
                <button type="submit" disabled={submitting}>
                    {submitting ? "Registering..." : "Register"}
                </button>
            </form>
            <p>
                Already have an account? <Link to="/login">Log In</Link>
            </p>
        </div>
    );
}