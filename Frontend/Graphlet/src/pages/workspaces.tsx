import "../components/workspaces/workspaces.css"
import OtherOptions from "../components/workspaces/otherOptions.tsx";
import { useState, useEffect } from "react";
import WorkspacePreview from "../components/workspaces/workspacePreview.tsx";
import CreatingNewWorkspace from "../components/workspaces/creatingNewWokspace.tsx";
import { Workspace } from "../components/classes/workspace.tsx"
import WorkspaceComponent from "../components/workspace/workspace.tsx";

// Fetch workspaces from the API. Returns an array of Workspace objects (empty array on unexpected responses).
async function getWorkspaces(): Promise<Workspace[]> {
    const response = await fetch("http://localhost:5188/api/workspace", {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${localStorage.getItem("token")}`
        }
    });

    if (response.ok) {
        const data = await response.json();
        // Ensure we always return an array
        if (Array.isArray(data)) return data as Workspace[];
        if (data == null) return [];
        return [data as Workspace];
    }

    // Non-OK responses: try to extract message or return empty
    let errText = `Failed to fetch workspaces: ${response.status} ${response.statusText}`;
    try {
        const errBody = await response.json();
        if (errBody && errBody.message) errText = errBody.message;
    } catch {
        /* ignore JSON parse errors */
    }
    throw new Error(errText);
}

export default function Workspaces() {
    // Workspaces list (array) rather than single Workspace
    const [workspaces, setWorkspaces] = useState<Workspace[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    // UI state
    const [showOtherOptions, setShowOtherOptions] = useState(false);
    const [showCreatingNew, setShowCreatingNew] = useState(false);
    const [searchTerm, setSearchTerm] = useState("");

    // Opened workspace ID for editing notes — initialize from URL query param so opening in new tab works
    const params = typeof window !== 'undefined' ? new URLSearchParams(window.location.search) : new URLSearchParams();
    const initialWorkspaceId = params.get('workspaceId');
    const isInitialFullscreen = params.get('fullscreen') === '1';
    // These values are only read (the UI opens workspaces in a new tab). Keep them as constants.
    const openedWorkspaceId: string | null = initialWorkspaceId;
    const isFullscreenMode: boolean = isInitialFullscreen;

    useEffect(() => {
        let mounted = true;

        async function load() {
            if (!mounted) return;
            setLoading(true);
            setError(null);

            try {
                const data = await getWorkspaces();
                if (!mounted) return;
                setWorkspaces(data);
            } catch (err) {
                if (!mounted) return;
                setError((err as Error)?.message ?? "Failed to load workspaces");
            } finally {
                if (mounted) setLoading(false);
            }
        }

        load();

        return () => { mounted = false; };
    }, []);

    function handleOtherOptionsClick() { //opening options when button clicked
        setShowOtherOptions(prev => !prev);
    }

    function handleCreateNewClick() {
        setShowCreatingNew(prev => !prev);
    }

    function handleCloseCreatingNew() {
        setShowCreatingNew(false);
    }

    const filteredWorkspaces = workspaces.filter(w => {
        if (!searchTerm) return true;
        return (w?.name ?? "").toLowerCase().includes(searchTerm.toLowerCase());
    });

    function handleRename(id: string, name: string) {
        setWorkspaces(prev => prev.map(w => w.id === id ? { ...w, name } : w));
    }

    function handleDelete(id: string) {
        setWorkspaces(prev => prev.filter(w => w.id !== id));
    }

    function openWorkspaceInNewTab(id: string) {
        // include fullscreen flag to render only the workspace in the new tab
        const url = window.location.origin + window.location.pathname + `?workspaceId=${encodeURIComponent(id)}&fullscreen=1`;
        window.open(url, '_blank');
    }

    // If the page was opened in fullscreen mode, render only the workspace component to occupy the whole tab
    if (isFullscreenMode && openedWorkspaceId) {
        return (
            <div style={{height: '100vh', width: '100vw', margin: 0}}>
                <WorkspaceComponent workspaceId={openedWorkspaceId} />
            </div>
        );
    }

    return (
        <>
            <div className="workspaces-page popup-bg">
                <header className={"workspaces-header"}>
                    <h2>My workspaces</h2>
                    <button type="button" onClick={handleCreateNewClick}>Create new</button>
                    <p>Keresés <input type="text" value={searchTerm} onChange={e => setSearchTerm(e.target.value)} placeholder="Search workspaces..."/></p>
                    <button onClick={handleOtherOptionsClick}>...</button>
                    {showOtherOptions && <OtherOptions/>}
                </header>
                <main>
                    <div className="workspaces-container">
                        {loading && <p>Loading workspaces...</p>}
                        {error && <div className="workspaces-error">{error}</div>}

                        {!loading && !error && filteredWorkspaces.length === 0 && (
                            <p>No workspaces found.</p>
                        )}

                        {!loading && !error && filteredWorkspaces.map((w, idx) => (
                            <WorkspacePreview key={w.id ?? `ws-${idx}`} id={w.id ?? `ws-${idx}`} name={w.name} onRename={handleRename} onDelete={handleDelete} onOpen={() => openWorkspaceInNewTab(w.id ?? `ws-${idx}`)} />
                        ))}

                    </div>

                    {showCreatingNew && <CreatingNewWorkspace onClose={handleCloseCreatingNew}  />}
                </main>
            </div>
        </>
    );
}