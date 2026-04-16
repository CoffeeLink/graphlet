import {useEffect, useRef, useState, useLayoutEffect} from "react";
import "./workspace.css";
import {type RelationDisplay} from "../classes/relationPin"; // Import RelationPin
import {NoteCard} from "./NoteCard.tsx";
import type {Note} from "./types.ts";

export default function Workspace({workspaceId}: { workspaceId?: string }) {
    const [errorText, setErrorText] = useState("");
    const [notes, setNotes] = useState<Note[]>([]);

    // Auto-hide error messages after a few seconds
    useEffect(() => {
        if (!errorText) return;
        const t = window.setTimeout(() => setErrorText(""), 4000);
        return () => window.clearTimeout(t);
    }, [errorText]);

    const FORBIDDEN_MESSAGE = 'Forbidden: try to log in, or ask for higher priviliges!';

    function handleForbidden(resp: Response): boolean {
        if (resp.status === 403) {
            setErrorText(FORBIDDEN_MESSAGE);
            return true;
        }
        return false;
    }

    // Linking mode state
    const [linkingSourceId, setLinkingSourceId] = useState<string | null>(null);

    // Toolbar search (by note title)
    const [searchQuery, setSearchQuery] = useState("");
    const [isSearchOpen, setIsSearchOpen] = useState(false);
    const searchRef = useRef<HTMLDivElement | null>(null);

    const normalizedQuery = searchQuery.trim().toLowerCase();
    const searchResults = normalizedQuery.length === 0
        ? []
        : notes
            .filter(n => (n.title ?? "").toLowerCase().includes(normalizedQuery))
            .slice(0, 10);

    function centerOnNote(note: Note) {
        // Notes are positioned relative to canvas center; offset is added to all notes.
        // To bring a note to the center, offset must cancel out note.x/note.y.
        setOffset({x: -note.x, y: -note.y});
    }

    useEffect(() => {
        function handleClickOutside(e: MouseEvent) {
            if (!searchRef.current) return;
            if (!searchRef.current.contains(e.target as Node)) setIsSearchOpen(false);
        }

        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    // Pan offset applied to all notes. Notes positions are relative to center.
    const [offset, setOffset] = useState({x: 0, y: 0});
    const cvRef = useRef<HTMLDivElement | null>(null);

    // Drag state for panning the canvas
    const draggingRef = useRef(false);
    const lastPosRef = useRef<{ x: number, y: number } | null>(null);
    const [isDragging, setIsDragging] = useState(false);

    // Viewport size for centering
    const [viewportSize, setViewportSize] = useState({w: 0, h: 0});

    useLayoutEffect(() => {
        function updateSize() {
            if (cvRef.current) {
                setViewportSize({
                    w: cvRef.current.clientWidth,
                    h: cvRef.current.clientHeight
                });
            }
        }

        updateSize();
        window.addEventListener('resize', updateSize);
        return () => window.removeEventListener('resize', updateSize);
    }, []);

    useEffect(() => {
        async function getNotes() {
            // If we don't have a workspaceId we can't call the workspace-scoped API from apimap.json
            if (!workspaceId) {
                setErrorText("No workspace selected — using sample notes for testing.");
                setNotes([
                    {id: 's1', title: 'Sample 1', content: 'Hello from center', x: 0, y: 0, relations: []},
                    {id: 's2', title: 'Left', content: 'To the left', x: -200, y: -50, relations: []},
                    {id: 's3', title: 'Right', content: 'To the right', x: 220, y: 80, relations: []}
                ]);
                return;
            }

            try {
                const url = `http://localhost:5188/api/workspace/${encodeURIComponent(workspaceId)}/note`;
                const raw = await fetch(url, {
                    headers: {
                        "Content-Type": "application/json",
                        Authorization: `Bearer ${localStorage.getItem("token")}`
                    }
                });

                if (handleForbidden(raw)) {
                    setNotes([]);
                    return;
                }

                if (raw.status !== 200) {
                    setErrorText("Couldn't fetch notes: " + raw.status + " — using sample notes for testing.");
                    setNotes([
                        {id: 's1', title: 'Sample 1', content: 'Hello from center', x: 0, y: 0, relations: []},
                        {id: 's2', title: 'Left', content: 'To the left', x: -200, y: -50, relations: []},
                        {id: 's3', title: 'Right', content: 'To the right', x: 220, y: 80, relations: []}
                    ]);
                    return;
                }
                const res = await raw.json();
                // API returns an array of notes (or possibly a single item) in the API note shape.
                const data = Array.isArray(res) ? res : (res ? [res] : []);
                if (data.length === 0) {
                    //setNotes([{ id: 'test-1', title: 'Test note', content: 'This is a test note', x: 0, y: 0 }]);
                } else {
                    // Map API note shape -> local Note
                    /* eslint-disable  @typescript-eslint/no-explicit-any */
                    const mapped: Note[] = data.map((n: any) => ({
                        id: n.id ?? n.noteId ?? String(Math.random()),
                        title: n.name ?? n.title ?? 'Untitled',
                        content: n.value ?? n.content ?? '',
                        x: Number(n.positionX ?? n.x ?? 0) || 0,
                        y: Number(n.positionY ?? n.y ?? 0) || 0,
                        relations: (n.relations || []).map((r: any) => ({
                            id: r.id,
                            connection: r.connection || [],
                            name: r.name || 'Relation'
                        }))
                    }));
                    setNotes(mapped);
                }
            } catch (err: unknown) {
                const msg = err instanceof Error ? err.message : String(err);
                setErrorText("Couldn't fetch notes: " + msg + " — using sample notes for testing.");
                setNotes([
                    {id: 's1', title: 'Sample 1', content: 'Hello from center', x: 0, y: 0, relations: []},
                    {id: 's2', title: 'Left', content: 'To the left', x: -200, y: -50, relations: []},
                    {id: 's3', title: 'Right', content: 'To the right', x: 220, y: 80, relations: []}
                ]);
            }
        }

        getNotes();
    }, [workspaceId]);

    // Save changes to an existing note via the workspace-scoped API
    async function saveNoteToApi(noteId: string, patch: Partial<Note>) {
        if (!workspaceId) {
            setErrorText('No workspace selected — cannot save to server.');
            return;
        }

        // Build API NoteUpdate shape based on provided patch
        /* eslint-disable  @typescript-eslint/no-explicit-any */
        const body: any = {};
        if (patch.title !== undefined) body.name = patch.title;
        if (patch.content !== undefined) body.value = patch.content;
        if (patch.x !== undefined) body.positionX = patch.x;
        if (patch.y !== undefined) body.positionY = patch.y;

        try {
            const resp = await fetch(`http://localhost:5188/api/workspace/${encodeURIComponent(workspaceId)}/note/${encodeURIComponent(noteId)}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                },
                body: JSON.stringify(body)
            });

            if (handleForbidden(resp)) return;

            if (!resp.ok) {
                let txt = `${resp.status} ${resp.statusText}`;
                try {
                    const b = await resp.json();
                    if (b?.message) txt = b.message;
                } catch {
                    void 0;
                }
                setErrorText("Failed to save note: " + txt);
            }
        } catch (e) {
            const msg = e instanceof Error ? e.message : String(e);
            setErrorText("Failed to save note: " + msg);
        }
    }

    async function deleteNote(id: string) {
        if (!workspaceId) {
            // just remove locally
            setNotes(n => n.filter(note => note.id !== id));
            return;
        }
        const res = await fetch(`http://localhost:5188/api/workspace/${encodeURIComponent(workspaceId)}/note/${encodeURIComponent(id)}`, {
            method: "DELETE",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${localStorage.getItem("token")}`
            }
        });

        if (handleForbidden(res)) return;

        if (res.status !== 204) {
            setErrorText("Couldn't delete note:" + res.status);
        } else {
            setNotes(n => n.filter(note => note.id !== id));
        }
    }

    // Simple mouse handlers for canvas panning
    function handleCanvasMouseDown(e: React.MouseEvent<HTMLDivElement>) {
        if (e.button !== 0) return;
        draggingRef.current = true;
        setIsDragging(true);
        lastPosRef.current = {x: e.clientX, y: e.clientY};
    }

    function handleCanvasMouseMove(e: React.MouseEvent<HTMLDivElement>) {
        if (!draggingRef.current || !lastPosRef.current) return;
        const dx = e.clientX - lastPosRef.current.x;
        const dy = e.clientY - lastPosRef.current.y;
        lastPosRef.current = {x: e.clientX, y: e.clientY};
        setOffset(o => ({x: o.x + dx, y: o.y + dy}));
    }

    function handleCanvasMouseUp() {
        draggingRef.current = false;
        setIsDragging(false);
        lastPosRef.current = null;
    }

    function updateNoteLocally(id: string, patch: Partial<Note>) {
        setNotes(prev => prev.map(n => n.id === id ? {...n, ...patch} : n));
    }

    function notesAlreadyRelated(aId: string, bId: string): boolean {
        const a = notes.find(n => n.id === aId);
        const b = notes.find(n => n.id === bId);

        const hasIn = (n?: Note) => (n?.relations ?? []).some(r => {
            const conn = r.connection || [];
            return conn.includes(aId) && conn.includes(bId);
        });

        return hasIn(a) || hasIn(b);
    }

    async function createRelation(sourceId: string, targetId: string) {
        if (sourceId === targetId) return;

        if (notesAlreadyRelated(sourceId, targetId)) {
            setErrorText('These notes are already related.');
            return;
        }

        if (!workspaceId) {
            // Local simulation
            // eslint-disable-next-line react-hooks/purity
            const newRelId = globalThis.crypto?.randomUUID ? globalThis.crypto.randomUUID() : `rel-${Date.now()}`;
            const targetNote = notes.find(n => n.id === targetId);
            const name = `Relation to ${targetNote?.title || 'Unknown'}`;

            setNotes(prev => prev.map(n => {
                // Update both source and target notes locally
                if (n.id === sourceId || n.id === targetId) {
                    // extra safety: avoid duplicates if state changed between click and setState
                    const already = n.relations.some(r => {
                        const conn = r.connection || [];
                        return conn.includes(sourceId) && conn.includes(targetId);
                    });
                    if (already) return n;

                    return {...n, relations: [...n.relations, {id: newRelId, connection: [sourceId, targetId], name}]};
                }
                return n;
            }));
            return;
        }

        const targetNote = notes.find(n => n.id === targetId);
        const name = `Relation to ${targetNote?.title || 'Unknown'}`;

        try {
            const resp = await fetch(`http://localhost:5188/api/workspace/${encodeURIComponent(workspaceId)}/note/${encodeURIComponent(sourceId)}/relation`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                },
                body: JSON.stringify({otherId: targetId, name})
            });

            if (handleForbidden(resp)) return;

            if (!resp.ok) {
                setErrorText(`Failed to create relation: ${resp.status}`);
                return;
            }
            const created = await resp.json();

            // Update all notes involved in the relation
            setNotes(prev => prev.map(n => {
                const conn = created.connection || [sourceId, targetId];
                if (conn.includes(n.id)) {
                    // Prevent duplicates
                    if (n.relations.find(r => r.id === created.id)) return n;

                    return {...n, relations: [...n.relations, {id: created.id, connection: conn, name: created.name}]};
                }
                return n;
            }));
        } catch (e) {
            setErrorText('Failed to create relation: ' + String(e));
        }
    }

    async function deleteRelation(noteId: string, relationId: string) {
        if (!workspaceId) {
            // Remove relation from all notes locally
            setNotes(prev => prev.map(n => ({
                ...n,
                relations: n.relations.filter(r => r.id !== relationId)
            })));
            return;
        }

        try {
            const resp = await fetch(`http://localhost:5188/api/workspace/${encodeURIComponent(workspaceId)}/note/${encodeURIComponent(noteId)}/relation/${encodeURIComponent(relationId)}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                }
            });

            if (handleForbidden(resp)) return;

            if (!resp.ok) {
                setErrorText(`Failed to delete relation: ${resp.status}`);
                return;
            }

            // Remove relation from all notes in state
            setNotes(prev => prev.map(n => ({
                ...n,
                relations: n.relations.filter(r => r.id !== relationId)
            })));
        } catch (e) {
            setErrorText("Failed to delete relation: " + String(e));
        }
    }

    // Add a test note at center for manual testing
    async function addTestNote() {
        const id = `test-${Date.now()}`;
        const note: Note = {id, title: 'New note', content: 'New note text', x: 0, y: 0, relations: []};

        if (!workspaceId) {
            setNotes(prev => [...prev, note]);
            return;
        }

        // Create on server using NoteCreate shape
        const body = {
            name: note.title,
            kind: 'text',
            value: note.content,
            positionX: note.x,
            positionY: note.y,
            tags: [] as string[]
        };

        try {
            const resp = await fetch(`http://localhost:5188/api/workspace/${encodeURIComponent(workspaceId)}/note`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                },
                body: JSON.stringify(body)
            });

            if (handleForbidden(resp)) return;

            if (!resp.ok) {
                setErrorText(`Failed to create note: ${resp.status} ${resp.statusText}`);
                // fallback to local
                setNotes(prev => [...prev, note]);
                return;
            }
            const created = await resp.json();
            const mapped: Note = {
                id: created.id ?? id,
                title: created.name ?? note.title,
                content: created.value ?? note.content,
                x: Number(created.positionX ?? note.x) || 0
                ,
                y: Number(created.positionY ?? note.y) || 0,
                relations: []
            };
            setNotes(prev => [...prev, mapped]);
        } catch (e) {
            const msg = e instanceof Error ? e.message : String(e);
            setErrorText("Failed to create note: " + msg);
            setNotes(prev => [...prev, note]);
        }
    }

    function handleWorkspaceClose() {
        window.close();
    }

    // Calculate lines to draw
    const drawnRelationIds = new Set<string>();
    const linesToDraw: Array<{ id: string, x1: number, y1: number, x2: number, y2: number }> = [];

    notes.forEach(note => {
        note.relations.forEach(rel => {
            if (drawnRelationIds.has(rel.id)) return;
            drawnRelationIds.add(rel.id);

            // Find connected notes
            // The connection array has [id1, id2]. One is note.id.
            const otherId = rel.connection.find(id => id !== note.id) || rel.connection[0]; // fallback if self-ref (unlikely but safe)
            if (!otherId) return;

            // If relation is somehow stored but other note missing, skip
            const otherNote = notes.find(n => n.id === otherId);
            if (!otherNote) return;

            // Calculate centers
            // Note positions (x,y) are relative to center (0,0)
            // Screen X = note.x + viewportW/2 + offset.x
            // Screen Y = note.y + viewportH/2 + offset.y

            // Fixed dimensions as per CSS/NoteCard (since resizing isn't in this version)
            const noteHeight = 140;
            const yOffset = noteHeight / 2;

            const x1 = note.x + viewportSize.w / 2 + offset.x;
            const y1 = (note.y + viewportSize.h / 2 + offset.y) - yOffset; // Top edge (pin position)

            const x2 = otherNote.x + viewportSize.w / 2 + offset.x;
            const y2 = (otherNote.y + viewportSize.h / 2 + offset.y) - yOffset; // Top edge (pin position)

            linesToDraw.push({
                id: rel.id,
                x1, y1, x2, y2
            });
        });
    });

    return (
        <section style={{height: '100%', display: 'flex', flexDirection: 'column'}}>
            {/* toolbar */}
            <div id="workspace-toolbar" className="workspace-toolbar">
                <div className="workspace-toolbar-left">
                    <button id="add-new-note-btn" onClick={addTestNote} className="add-test-note-btn">
                        Add new note
                    </button>

                    {/* search moved to the left */}
                    <div ref={searchRef} className="workspace-search">
                        <input
                            id="workspace-search-input"
                            className="workspace-search-input"
                            type="text"
                            value={searchQuery}
                            placeholder="Search notes by title..."
                            onFocus={() => setIsSearchOpen(true)}
                            onChange={(e) => {
                                setSearchQuery(e.target.value);
                                setIsSearchOpen(true);
                            }}
                        />

                        {isSearchOpen && normalizedQuery.length > 0 && (
                            <div id="workspace-search-results" className="workspace-search-results">
                                {searchResults.length === 0 ? (
                                    <div className="workspace-search-empty">No matches</div>
                                ) : (
                                    searchResults.map(n => (
                                        <button
                                            key={n.id}
                                            id={`workspace-search-result-${n.id}`}
                                            type="button"
                                            className="workspace-search-result"
                                            onClick={() => {
                                                centerOnNote(n);
                                                setLinkingSourceId(null);
                                                setSearchQuery('');
                                                setIsSearchOpen(false);
                                            }}
                                        >
                                            {n.title ?? 'Untitled'}
                                        </button>
                                    ))
                                )}
                            </div>
                        )}
                    </div>

                    {/* error moved to the right of search */}
                    <div id="workspace-error" className="workspace-error">{errorText}</div>
                    {linkingSourceId && (
                        <div className="workspace-message" style={{color: 'blue'}}>
                            Select a note to link... (Click background to cancel)
                        </div>
                    )}
                </div>

                {/* actions */}
                <div className="floating-toolbar">
                    <button id="workspace-close-btn" onClick={handleWorkspaceClose} className="workspace-close-btn">
                        Close
                    </button>
                </div>
            </div>

            {/* canvas area */}
            <div
                id={"cv"}
                ref={cvRef}
                onMouseDown={(e) => {
                    handleCanvasMouseDown(e);
                    if (linkingSourceId) setLinkingSourceId(null); // Cancel linking on canvas click
                }}
                onMouseMove={handleCanvasMouseMove}
                onMouseUp={handleCanvasMouseUp}
                onMouseLeave={handleCanvasMouseUp}
                className={`workspace-canvas ${isDragging ? 'cursor-grabbing' : 'cursor-grab'}`}
            >
                {/* Lines Layer */}
                <svg className="workspace-lines-layer">
                    {linesToDraw.map(line => {
                        const midX = (line.x1 + line.x2) / 2;
                        const midY = (line.y1 + line.y2) / 2;
                        const dist = Math.sqrt(Math.pow(line.x2 - line.x1, 2) + Math.pow(line.y2 - line.y1, 2));
                        // Droop factor
                        const droop = Math.min(dist * 0.2, 100);
                        const cY = midY + droop;

                        return (
                            <path
                                key={line.id}
                                d={`M ${line.x1} ${line.y1} Q ${midX} ${cY} ${line.x2} ${line.y2}`}
                                stroke="#555"
                                strokeWidth="2"
                                fill="none"
                            />
                        );
                    })}
                </svg>


                {/* render notes positioned relative to center + offset */}
                {notes.map(note => {
                    // Prepare relations for display
                    const displayRelations: RelationDisplay[] = note.relations.map(r => {
                        // Find the other ID. The connection array has 2 IDs.
                        // However, sometimes it might not be strictly 2 or order guaranteed.
                        // Assuming [source, target] or similar.
                        // If I am 'note.id', the other is the one that != note.id
                        const otherId = r.connection.find(id => id !== note.id) || r.connection[0]; // Fallback
                        const otherNote = notes.find(n => n.id === otherId);
                        return {
                            id: r.id,
                            name: otherNote?.title || r.name || 'Unknown Note'
                        };
                    });

                    return (
                        <NoteCard key={note.id}
                                  note={note}
                                  relations={displayRelations}
                                  isLinking={!!linkingSourceId}
                                  onSelect={() => {
                                      if (linkingSourceId && linkingSourceId !== note.id) {
                                          createRelation(linkingSourceId, note.id);
                                          setLinkingSourceId(null);
                                      }
                                  }}
                                  onStartLinking={() => setLinkingSourceId(note.id)}
                                  onDeleteRelation={(relId) => deleteRelation(note.id, relId)}
                                  parentRef={cvRef}
                                  offset={offset}
                                  onDelete={() => deleteNote(note.id)}
                                  onMove={(x: number, y: number) => {
                                      updateNoteLocally(note.id, {x, y});
                                  }}
                                  onMoveEnd={(x: number, y: number) => {
                                      updateNoteLocally(note.id, {x, y});
                                      void saveNoteToApi(note.id, {x, y});
                                  }}
                                  onUpdate={(patch: Partial<Note>) => {
                                      updateNoteLocally(note.id, patch);
                                      // map patch to API fields and save
                                      void saveNoteToApi(note.id, patch);
                                  }}/>
                    );
                })}
            </div>
        </section>
    );
}

