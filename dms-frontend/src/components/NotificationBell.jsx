import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { notificationsApi } from "../api/notificationsApi";

export function NotificationBell() {
    const [notifications, setNotifications] = useState([]);
    const [unreadCount, setUnreadCount] = useState(0);
    const [open, setOpen] = useState(false);
    const dropdownRef = useRef(null);
    const navigate = useNavigate();

    async function loadUnreadCount() {
        try {
            const count = await notificationsApi.getUnreadCount();
            setUnreadCount(count);
        } catch {
            // silently ignore — a failed poll shouldn't disrupt the rest of the app
        }
    }

    useEffect(() => {
        loadUnreadCount();
        const interval = setInterval(loadUnreadCount, 30000);
        return () => clearInterval(interval);
    }, []);

    useEffect(() => {
        function handleClickOutside(e) {
            if (dropdownRef.current && !dropdownRef.current.contains(e.target)) {
                setOpen(false);
            }
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    async function handleOpen() {
        setOpen((prev) => !prev);
        if (!open) {
            try {
                const list = await notificationsApi.getAll();
                setNotifications(list);
            } catch {
                // ignore
            }
        }
    }

    async function handleNotificationClick(n) {
        if (!n.isRead) {
            await notificationsApi.markRead(n.id);
            setUnreadCount((prev) => Math.max(0, prev - 1));
        }
        setOpen(false);
        if (n.documentId) navigate(`/documents/${n.documentId}`);
    }

    async function handleMarkAllRead() {
        await notificationsApi.markAllRead();
        setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
        setUnreadCount(0);
    }

    return (
        <div className="notification-bell" ref={dropdownRef}>
            <button className="bell-button" onClick={handleOpen}>
                🔔
                {unreadCount > 0 && <span className="bell-badge">{unreadCount}</span>}
            </button>

            {open && (
                <div className="notification-dropdown">
                    <div className="notification-dropdown-header">
                        <span>Notifications</span>
                        {unreadCount > 0 && (
                            <button onClick={handleMarkAllRead}>Mark all read</button>
                        )}
                    </div>
                    {notifications.length === 0 ? (
                        <p className="notification-empty">No notifications yet.</p>
                    ) : (
                        <ul className="notification-list">
                            {notifications.map((n) => (
                                <li
                                    key={n.id}
                                    className={`notification-item ${n.isRead ? "" : "notification-unread"}`}
                                    onClick={() => handleNotificationClick(n)}
                                >
                                    <span>{n.message}</span>
                                    <span className="notification-time">
                                        {new Date(n.createdAt).toLocaleDateString()}
                                    </span>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>
            )}
        </div>
    );
}