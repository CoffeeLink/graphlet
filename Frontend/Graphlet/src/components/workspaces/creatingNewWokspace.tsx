import "./createNewWorkspace.css"
import {useState, useRef, useEffect} from "react";
import {Input} from "@heroui/input";
import {Button} from "@heroui/button";

interface CreatingNewProps {
    onClose?: () => void;
}

export default function CreatingNewWokspace({ onClose }: CreatingNewProps) {

    const ref = useRef<HTMLDivElement>(null);

    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (ref.current && !ref.current.contains(event.target as Node)) {
                if (onClose) onClose();
            }
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, [onClose]);

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



    return (
        <>
            <div className={"creating-new-workspace fg"} ref={ref}>
                <div className="header-row">
                    <div className="header-title text">Create new workspace</div>
                    <button className="header-close close-button" aria-label="Close" onClick={handleClose}>X</button>
                </div>
                <div className="flex w-full flex-wrap md:flex-nowrap gap-4">
                    <Input label="Workspace name" placeholder="Enter workspace name"
                           value={workspaceName} onValueChange={setWorkspaceName} id={"nameInput"}/>
                </div>
                {error && <div className="create-error">{error}</div>}
                <Button id={"create-new-workspace-button"} onPress={async () => { const ok = await handleCreateWorkspace(); if (ok) handleClose(); }}
                        isDisabled={creating || workspaceName.trim() === ''}>
                    {creating ? 'Creating...' : 'Create new Workspace'}
                </Button>
            </div>
        </>
    );
}