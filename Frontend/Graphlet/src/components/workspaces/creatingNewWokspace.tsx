import "./createNewWorkspace.css"
import {useState} from "react";
import {Tag} from "./tag.tsx";
import {CreateNewTag} from "./createNewTag.tsx";

interface CreatingNewProps {
    onClose?: () => void;
}

export default function CreatingNewWokspace({ onClose }: CreatingNewProps) {

    const tag = <Tag text="Aasd" color="blue" />;
    console.log(tag);

    const [workspaceName, setWorkspaceName] = useState('')
    const [creating, setCreating] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Return true on success, false on failure
    async function handleCreateWorkspace(): Promise<boolean> {
        setCreating(true);
        setError(null);
        try {
            const res = await fetch("http://localhost:5188/api/workspace", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                },
                body: JSON.stringify({
                    name: workspaceName
                })
            });

            if (!res.ok) {
                let msg = `Failed to create workspace: ${res.status} ${res.statusText}`;
                try {
                    const body = await res.json();
                    if (body && body.message) msg = body.message;
                } catch {
                    // ignore
                }
                setError(msg);
                return false;
            }

            return true;
        } catch (e) {
            setError((e as Error)?.message ?? 'Network error');
            return false;
        } finally {
            setCreating(false);
        }
    }

    function handleClose(){
        if (onClose) onClose();
    }

    const [showCreateTag, setShowCreateTag] = useState(false);
    function handleCreateNewTag() {
        setShowCreateTag(perv => !perv);
    }
    function onCloseCreate() {
        setShowCreateTag(false);
        getTags();
    }

    async function getTags(){
       //kell endpoint
       //  const rawres = await fetch("http://localhost:5188/api/userTags", {
       //      method: "GET",
       //      headers: {
       //
       //      }
       //  })

    }

    return (
        <>
            <div className={"creating-new-workspace fg"}>
                <div className="header-row">
                    <div className="header-title text">Create new workspace</div>
                    <button className="header-close close-button" aria-label="Close" onClick={handleClose}>X</button>
                </div>
                <table>
                    <tbody>
                    <tr >
                        <td >Workspace name:</td>
                        <td><input type="text" id={"nameInput"} value={workspaceName} onChange={(e) => setWorkspaceName(e.target.value)} /></td>
                    </tr>
                    <tr>
                        <td>Tags:</td>
                        <td className={"tag-container"}>  </td>
                        <td><button onClick={handleCreateNewTag}>Create new tag</button></td>
                    </tr>
                    </tbody>
                </table>
                {error && <div className="create-error">{error}</div>}
                <button onClick={async ()=>{ const ok = await handleCreateWorkspace(); if (ok) handleClose(); }} id={"create-new-workspace-button"} disabled={creating || workspaceName.trim() === ''}>{creating ? 'Creating...' : 'Create new Workspace'}</button>
                {showCreateTag && <CreateNewTag onClose={onCloseCreate}/>}
            </div>
        </>
    );
}