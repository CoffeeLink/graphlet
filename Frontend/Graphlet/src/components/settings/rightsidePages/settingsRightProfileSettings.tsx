import "./settingsRightComponents.css";
import { useEffect, useState } from "react";

type MeUser = {
    id: string;
    username: string;
    email: string;
    profilePicUrl?: string;
    lastSeen?: string;
};

type PublicUser = {
    id: string;
    username: string;
    profilePicUrl?: string;
};

export default function SettingsRightProfileSettings() {
    const [user, setUser] = useState<MeUser | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Copy button state
    const [copyState, setCopyState] = useState<"idle" | "copied" | "failed">("idle");

    // PUT /api/user/me
    const [editUsername, setEditUsername] = useState("");
    const [editEmail, setEditEmail] = useState("");
    const [editProfilePicUrl, setEditProfilePicUrl] = useState("");
    const [saveLoading, setSaveLoading] = useState(false);
    const [saveMessage, setSaveMessage] = useState<string | null>(null);

    // GET /api/user/{id}
    const [lookupId, setLookupId] = useState("");
    const [publicUser, setPublicUser] = useState<PublicUser | null>(null);
    const [lookupLoading, setLookupLoading] = useState(false);
    const [lookupError, setLookupError] = useState<string | null>(null);

    async function getProfile() {
        try {
            setLoading(true);
            setError(null);
            setSaveMessage(null);

            const token = localStorage.getItem("token");
            const response = await fetch("http://localhost:5188/api/user/me", {
                headers: {
                    Authorization: `Bearer ${token}`
                }
            });

            if (!response.ok) {
                const msg = await response.text().catch(() => "");
                setError(msg || "Failed to fetch profile");
                setUser(null);
                return;
            }

            const data = await response.json();
            const me: MeUser = {
                id: data.id,
                username: data.username,
                email: data.email,
                profilePicUrl: data.profilePicUrl,
                lastSeen: data.lastSeen
            };

            setUser(me);
            setEditUsername(me.username ?? "");
            setEditEmail(me.email ?? "");
            setEditProfilePicUrl(me.profilePicUrl ?? "");
        } catch (e) {
            console.error("Failed to fetch profile", e);
            setError("Error connecting to server");
            setUser(null);
        } finally {
            setLoading(false);
        }
    }

    async function copyText(text: string): Promise<boolean> {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch {
            // Fallback for older/locked-down environments
            try {
                const el = document.createElement("textarea");
                el.value = text;
                el.style.position = "fixed";
                el.style.left = "-9999px";
                document.body.appendChild(el);
                el.focus();
                el.select();
                const ok = document.execCommand("copy");
                document.body.removeChild(el);
                return ok;
            } catch {
                return false;
            }
        }
    }

    async function onCopyUserId() {
        if (!user?.id) return;
        const ok = await copyText(user.id);
        setCopyState(ok ? "copied" : "failed");
        window.setTimeout(() => setCopyState("idle"), 1200);
    }

    async function updateMe() {
        if (!user) return;

        try {
            setSaveLoading(true);
            setError(null);
            setSaveMessage(null);

            const token = localStorage.getItem("token");
            if (!token) {
                setError("Missing auth token");
                return;
            }

            const payload: Record<string, string> = {};
            const u = editUsername.trim();
            const e = editEmail.trim();
            const p = editProfilePicUrl.trim();

            if (u && u !== user.username) payload.username = u;
            if (e && e !== user.email) payload.email = e;
            if (p && p !== (user.profilePicUrl ?? "")) payload.profilePicUrl = p;

            if (Object.keys(payload).length === 0) {
                setSaveMessage("No changes to save.");
                return;
            }

            const response = await fetch("http://localhost:5188/api/user/me", {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const msg = await response.text().catch(() => "");
                setError(msg || "Failed to update profile");
                return;
            }

            const updated = await response.json();
            const me: MeUser = {
                id: updated.id,
                username: updated.username,
                email: updated.email,
                profilePicUrl: updated.profilePicUrl,
                lastSeen: updated.lastSeen
            };

            setUser(me);
            setSaveMessage("Saved.");
        } catch (e) {
            console.error("Failed to update profile", e);
            setError("Error connecting to server");
        } finally {
            setSaveLoading(false);
        }
    }

    async function fetchPublicUser() {
        const id = lookupId.trim();
        if (!id) return;

        try {
            setLookupLoading(true);
            setLookupError(null);
            setPublicUser(null);

            const response = await fetch(`http://localhost:5188/api/user/${encodeURIComponent(id)}`);
            if (!response.ok) {
                const msg = await response.text().catch(() => "");
                setLookupError(msg || "Failed to fetch user");
                return;
            }

            const data = await response.json();
            setPublicUser({
                id: data.id,
                username: data.username,
                profilePicUrl: data.profilePicUrl
            });
        } catch (e) {
            console.error("Failed to fetch public user", e);
            setLookupError("Error connecting to server");
        } finally {
            setLookupLoading(false);
        }
    }

    useEffect(() => {
        const timeoutId = window.setTimeout(() => {
            void getProfile();
        }, 0);

        return () => {
            window.clearTimeout(timeoutId);
        };
    }, []);

    return (
        <section className="settings-right-component">
            <h1>Profile Settings</h1>
            <h2>My profile</h2>

            {loading && <div>Loading...</div>}
            {error && <div className="error">{error}</div>}
            {saveMessage && <div>{saveMessage}</div>}

            {user && (
                <div className="profile-details">
                    <p>
                        <strong>User ID:</strong> {user.id}{" "}
                        <button id="copy-userid" type="button" onClick={onCopyUserId}>
                            {copyState === "copied" ? "Copied" : copyState === "failed" ? "Copy failed" : "Copy"}
                        </button>
                    </p>

                    {user.profilePicUrl && (
                        <p>
                            <strong>Profile picture:</strong>{" "}
                            <a href={user.profilePicUrl} target="_blank" rel="noreferrer">
                                {user.profilePicUrl}
                            </a>
                        </p>
                    )}

                    <p>
                        <strong>Username:</strong> {user.username}
                    </p>
                    <p>
                        <strong>Email:</strong> {user.email}
                    </p>

                    {user.lastSeen && (
                        <p>
                            <strong>Last seen:</strong> {new Date(user.lastSeen).toLocaleString()}
                        </p>
                    )}

                    <div className="section">
                        <h3>Update my profile</h3>

                        <table className="edit-table">
                            <tbody>
                                <tr>
                                    <th scope="row">Username</th>
                                    <td>
                                        <input
                                            id="profile-edit-username"
                                            type="text"
                                            value={editUsername}
                                            onChange={(e) => setEditUsername(e.target.value)}
                                            placeholder="Username"
                                        />
                                    </td>
                                </tr>
                                <tr>
                                    <th scope="row">Email</th>
                                    <td>
                                        <input
                                            id="profile-edit-email"
                                            type="email"
                                            value={editEmail}
                                            onChange={(e) => setEditEmail(e.target.value)}
                                            placeholder="Email"
                                        />
                                    </td>
                                </tr>
                                <tr>
                                    <th scope="row">Profile picture URL</th>
                                    <td>
                                        <input
                                            id="profile-edit-profilepicurl"
                                            type="text"
                                            value={editProfilePicUrl}
                                            onChange={(e) => setEditProfilePicUrl(e.target.value)}
                                            placeholder="https://..."
                                        />
                                    </td>
                                </tr>
                                <tr>
                                    <th scope="row"></th>
                                    <td className="edit-table-actions">
                                        <button id="profile-save" type="button" onClick={updateMe} disabled={saveLoading}>
                                            {saveLoading ? "Saving..." : "Save"}
                                        </button>
                                        <button id="profile-refresh" type="button" onClick={getProfile} disabled={saveLoading}>
                                            Refresh
                                        </button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <div className="section">
                        <h3>Lookup public user (by id)</h3>

                        <div className="public-user-search">
                            {lookupError && <div className="error">{lookupError}</div>}

                            <div className="add-workspace">
                                <input
                                    id="public-user-id"
                                    type="text"
                                    value={lookupId}
                                    onChange={(e) => setLookupId(e.target.value)}
                                    placeholder="User ID (uuid)"
                                />
                                <button id="public-user-fetch" type="button" onClick={fetchPublicUser} disabled={lookupLoading}>
                                    {lookupLoading ? "Loading..." : "Fetch"}
                                </button>
                            </div>

                            {publicUser && (
                                <div style={{ marginTop: 10 }}>
                                    <p>
                                        <strong>ID:</strong> {publicUser.id}
                                    </p>
                                    <p>
                                        <strong>Username:</strong> {publicUser.username}
                                    </p>
                                    {publicUser.profilePicUrl && (
                                        <p>
                                            <strong>Profile picture:</strong>{" "}
                                            <a href={publicUser.profilePicUrl} target="_blank" rel="noreferrer">
                                                {publicUser.profilePicUrl}
                                            </a>
                                        </p>
                                    )}
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            )}
        </section>
    );
}

