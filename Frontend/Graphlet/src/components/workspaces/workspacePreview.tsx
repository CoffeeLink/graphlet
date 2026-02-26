import "./workspacePreview.css"
import { useState } from "react";
import ConfirmDialog from "../confirmdialog/confirmDialog";

interface WorkspacePreviewProps {
    id: string;
    name: string;
    onRename?: (id: string, name: string) => void;
    onDelete?: (id: string) => void;
    onOpen?: () => void; // <-- Add onOpen prop type
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
                try {
                    const body = await resp.json(); if (body?.message) errText = body.message;
                } catch (err)
                {
                    console.error("Failed to parse error response:", err);
                    setError((errText)?? "Failed to rename workspace");
                }
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
                try {
                    const body = await resp.json(); if (body?.message) errText = body.message;
                }
                catch (err) {
                    console.error("Failed to parse error response:", err);
                    setError((errText)?? "Failed to rename workspace");
                }
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
            <div className="workspace-preview" id={`workspace-card-${props.id}`} onClick={props.onOpen}>
                <p style={{ color: "black" }}>{renameValue}</p>
                <div style={{ position: "relative" }}>
                    <button id={`workspace-menu-button-${props.id}`} aria-haspopup="menu" onClick={(e) => { e.stopPropagation(); setShowMenu(s => !s); }}>...</button>
                    {showMenu && (
                        <div className="workspace-preview-menu" role="menu" onClick={(e) => e.stopPropagation()}>
                            <button id={`workspace-rename-button-${props.id}`} role="menuitem" onClick={(e) => { e.stopPropagation(); setConfirmMode("rename"); setShowMenu(false); setRenameValue(props.name); }}>Rename</button>
                            <button id={`workspace-delete-button-${props.id}`} role="menuitem" onClick={(e) => { e.stopPropagation(); setConfirmMode("delete"); setShowMenu(false); }}>Delete</button>
                        </div>
                    )}
                </div>
            </div>

            {confirmMode === "rename" && (
                <ConfirmDialog
                    title="Renaming workspace"
                    showInput
                    inputValue={renameValue}
                    onInputChange={(v) => setRenameValue(v)}
                    onConfirm={handleRenameConfirm}
                    onCancel={() => { setConfirmMode(null); setError(null); }}
                    confirmLabel="Rename"
                    cancelLabel="Cancel"
                    isSubmitting={isSubmitting}
                    error={error}
                    confirmVariant={"primary"}
                />
            )}

            {confirmMode === "delete" && (
                <ConfirmDialog
                    title="Deleting workspace"
                    message="Are you sure you want to delete this workspace? This action cannot be undone."
                    onConfirm={handleDeleteConfirm}
                    onCancel={() => { setConfirmMode(null); setError(null); }}
                    confirmLabel="Delete"
                    cancelLabel="Cancel"
                    isSubmitting={isSubmitting}
                    error={error}
                    confirmVariant={"danger"}
                />
            )}
        </div>
    )
}