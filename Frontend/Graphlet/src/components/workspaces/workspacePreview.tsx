import "./workspacePreview.css"
import { useState } from "react";
import ConfirmDialog from "../confirmdialog/confirmDialog";

interface WorkspacePreviewProps {
    id: string;
    name: string;
    onRename?: (id: string, name: string) => void;
    onDelete?: (id: string) => void;
}

export default function WorkspacePreview(props: WorkspacePreviewProps) {
    const [showMenu, setShowMenu] = useState(false);
    const [confirmMode, setConfirmMode] = useState<"rename"|"delete"|null>(null);
    const [renameValue, setRenameValue] = useState(props.name);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    async function handleRenameConfirm(value?: string) {
        const newName = (value ?? renameValue).trim();
        if (!newName) { setError("Name cannot be empty"); return; }
        setIsSubmitting(true); setError(null);
        try {
            const resp = await fetch(`http://localhost:5188/api/workspace/${props.id}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                },
                body: JSON.stringify({ name: newName })
            });
            if (!resp.ok) {
                let errText = `${resp.status} ${resp.statusText}`;
                try { const body = await resp.json(); if (body?.message) errText = body.message; } catch {}
                throw new Error(errText);
            }
            // success - inform parent
            if (props.onRename) props.onRename(props.id, newName);
            setRenameValue(newName);
            setConfirmMode(null);
            setShowMenu(false);
        } catch (err) {
            setError((err as Error)?.message ?? "Failed to rename workspace");
        } finally {
            setIsSubmitting(false);
        }
    }

    async function handleDeleteConfirm() {
        setIsSubmitting(true); setError(null);
        try {
            const resp = await fetch(`http://localhost:5188/api/workspace/${props.id}`, {
                method: "DELETE",
                headers: {
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                }
            });
            if (!resp.ok) {
                let errText = `${resp.status} ${resp.statusText}`;
                try { const body = await resp.json(); if (body?.message) errText = body.message; } catch {}
                throw new Error(errText);
            }
            if (props.onDelete) props.onDelete(props.id);
            setConfirmMode(null);
            setShowMenu(false);
        } catch (err) {
            setError((err as Error)?.message ?? "Failed to delete workspace");
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <div className="workspacePreview">
            <div className="workspace-preview">
                <p style={{ color: "black" }}>{renameValue}</p>
                <div style={{ position: "relative" }}>
                    <button aria-haspopup="menu" onClick={() => setShowMenu(s => !s)}>...</button>
                    {showMenu && (
                        <div className="workspace-preview-menu" role="menu">
                            <button role="menuitem" onClick={() => { setConfirmMode("rename"); setShowMenu(false); setRenameValue(props.name); }}>Rename</button>
                            <button role="menuitem" onClick={() => { setConfirmMode("delete"); setShowMenu(false); }}>Delete</button>
                        </div>
                    )}
                </div>
            </div>

            {confirmMode === "rename" && (
                <ConfirmDialog
                    title="Rename workspace"
                    showInput
                    inputValue={renameValue}
                    onInputChange={(v) => setRenameValue(v)}
                    onConfirm={handleRenameConfirm}
                    onCancel={() => { setConfirmMode(null); setError(null); }}
                    confirmLabel="Rename"
                    cancelLabel="Cancel"
                    isSubmitting={isSubmitting}
                    error={error}
                />
            )}

            {confirmMode === "delete" && (
                <ConfirmDialog
                    title="Delete workspace"
                    message="Are you sure you want to delete this workspace? This action cannot be undone."
                    onConfirm={handleDeleteConfirm}
                    onCancel={() => { setConfirmMode(null); setError(null); }}
                    confirmLabel="Delete"
                    cancelLabel="Cancel"
                    isSubmitting={isSubmitting}
                    error={error}
                />
            )}
        </div>
    )
}