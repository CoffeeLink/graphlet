import {useEffect, useRef, useState, useLayoutEffect} from "react";
import "./workspace.css";

// note data shape
interface Note {
    id: string;
    title?: string;
    content?: string;
    x: number; // position relative to center
    y: number;
}

export default function Workspace({ workspaceId }: { workspaceId?: string }) {
    const [errorText, setErrorText] = useState("");
    const [notes, setNotes] = useState<Note[]>([]);

    // Pan offset applied to all notes. Notes positions are relative to center.
    const [offset, setOffset] = useState({x: 0, y: 0});
    const cvRef = useRef<HTMLDivElement | null>(null);

    // Drag state for panning the canvas
    const draggingRef = useRef(false);
    const lastPosRef = useRef<{x:number,y:number} | null>(null);
    const [isDragging, setIsDragging] = useState(false);

    useEffect(()=>{
        async function getNotes(){
            // If we don't have a workspaceId we can't call the workspace-scoped API from apimap.json
            if (!workspaceId) {
                setErrorText("No workspace selected — using sample notes for testing.");
                setNotes([
                    {id: 's1', title: 'Sample 1', content: 'Hello from center', x: 0, y: 0},
                    {id: 's2', title: 'Left', content: 'To the left', x: -200, y: -50},
                    {id: 's3', title: 'Right', content: 'To the right', x: 220, y: 80}
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
                if(raw.status !== 200){
                    setErrorText("Couldn't fetch notes: " + raw.status + " — using sample notes for testing.");
                    setNotes([
                        {id: 's1', title: 'Sample 1', content: 'Hello from center', x: 0, y: 0},
                        {id: 's2', title: 'Left', content: 'To the left', x: -200, y: -50},
                        {id: 's3', title: 'Right', content: 'To the right', x: 220, y: 80}
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
                        y: Number(n.positionY ?? n.y ?? 0) || 0
                    }));
                    setNotes(mapped);
                }
            } catch (err: unknown) {
                const msg = err instanceof Error ? err.message : String(err);
                setErrorText("Couldn't fetch notes: " + msg + " — using sample notes for testing.");
                setNotes([
                    {id: 's1', title: 'Sample 1', content: 'Hello from center', x: 0, y: 0},
                    {id: 's2', title: 'Left', content: 'To the left', x: -200, y: -50},
                    {id: 's3', title: 'Right', content: 'To the right', x: 220, y: 80}
                ]);
            }
        }
        getNotes();
    },[workspaceId]);

    // Save changes to an existing note via the workspace-scoped API
    async function saveNoteToApi(noteId: string, patch: Partial<Note>){
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

        try{
            const resp = await fetch(`http://localhost:5188/api/workspace/${encodeURIComponent(workspaceId)}/note/${encodeURIComponent(noteId)}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                },
                body: JSON.stringify(body)
            });
            if(!resp.ok){
                let txt = `${resp.status} ${resp.statusText}`;
                try{ const b = await resp.json(); if(b?.message) txt = b.message; } catch { void 0; }
                setErrorText("Failed to save note: " + txt);
            }
        }catch(e){
            const msg = e instanceof Error ? e.message : String(e);
            setErrorText("Failed to save note: " + msg);
        }
    }

    async function deleteNote(id:string){
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
        if(res.status !== 200){
            setErrorText("Couldn't delete note:" + res.status);
        } else {
            setNotes(n => n.filter(note => note.id !== id));
        }
    }

    // Simple mouse handlers for canvas panning
    function handleCanvasMouseDown(e: React.MouseEvent<HTMLDivElement>){
        if (e.button !== 0) return;
        draggingRef.current = true;
        setIsDragging(true);
        lastPosRef.current = { x: e.clientX, y: e.clientY };
    }
    function handleCanvasMouseMove(e: React.MouseEvent<HTMLDivElement>){
        if(!draggingRef.current || !lastPosRef.current) return;
        const dx = e.clientX - lastPosRef.current.x;
        const dy = e.clientY - lastPosRef.current.y;
        lastPosRef.current = { x: e.clientX, y: e.clientY };
        setOffset(o => ({ x: o.x + dx, y: o.y + dy }));
    }
    function handleCanvasMouseUp(){
        draggingRef.current = false;
        setIsDragging(false);
        lastPosRef.current = null;
    }

    function updateNoteLocally(id: string, patch: Partial<Note>){
        setNotes(prev => prev.map(n => n.id === id ? { ...n, ...patch } : n));
    }

    // Add a test note at center for manual testing
    async function addTestNote() {
        const id = `test-${Date.now()}`;
        const note: Note = { id, title: 'New note', content: 'New note text', x: 0, y: 0 };

        if (!workspaceId) {
            setNotes(prev => [ ...prev, note ]);
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

        try{
            const resp = await fetch(`http://localhost:5188/api/workspace/${encodeURIComponent(workspaceId)}/note`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                },
                body: JSON.stringify(body)
            });
            if(!resp.ok){
                setErrorText(`Failed to create note: ${resp.status} ${resp.statusText}`);
                // fallback to local
                setNotes(prev => [ ...prev, note ]);
                return;
            }
            const created = await resp.json();
            const mapped: Note = {
                id: created.id ?? id,
                title: created.name ?? note.title,
                content: created.value ?? note.content,
                x: Number(created.positionX ?? note.x) || 0,
                y: Number(created.positionY ?? note.y) || 0
            };
            setNotes(prev => [ ...prev, mapped ]);
        }catch(e){
            const msg = e instanceof Error ? e.message : String(e);
            setErrorText("Failed to create note: " + msg);
            setNotes(prev => [ ...prev, note ]);
        }
    }

    return (
        <section style={{height: '100%', display: 'flex', flexDirection: 'column'}}>
            {/* toolbar */}
            <div className="workspace-toolbar">
                <div className="workspace-error">{errorText}</div>
                {/* floating test toolbar on the canvas */}
                <div className="floating-toolbar">
                    <button onClick={addTestNote} className="add-test-note-btn">Add new note</button>
                </div>
            </div>

            {/* canvas area */}
            <div
                id={"cv"}
                ref={cvRef}
                onMouseDown={handleCanvasMouseDown}
                onMouseMove={handleCanvasMouseMove}
                onMouseUp={handleCanvasMouseUp}
                onMouseLeave={handleCanvasMouseUp}
                className={`workspace-canvas ${isDragging ? 'cursor-grabbing' : 'cursor-grab'}`}
            >



                {/* render notes positioned relative to center + offset */}
                {notes.map(note => (
                    <NoteCard key={note.id}
                              note={note}
                              parentRef={cvRef}
                              offset={offset}
                              onDelete={() => deleteNote(note.id)}
                              onMove={(x:number,y:number)=>{
                                  updateNoteLocally(note.id, { x, y });
                              }}
                              onMoveEnd={(x:number,y:number)=>{
                                  updateNoteLocally(note.id, { x, y });
                                  void saveNoteToApi(note.id, { x, y });
                              }}
                              onUpdate={(patch: Partial<Note>)=>{
                                  updateNoteLocally(note.id, patch);
                                  // map patch to API fields and save
                                  void saveNoteToApi(note.id, patch);
                              }} />
                ))}
             </div>
         </section>
     );
}

type NoteCardProps = {
    note: Note;
    parentRef: React.RefObject<HTMLDivElement | null>;
    offset: {x:number,y:number};
    onDelete: () => void;
    onMove?: (x:number,y:number) => void;
    onMoveEnd?: (x:number,y:number) => void;
    onUpdate?: (patch: Partial<Note>) => void;
}

function NoteCard({note, parentRef, offset, onDelete, onMove, onMoveEnd, onUpdate}: NoteCardProps){
    const [rect, setRect] = useState<DOMRect | null>(null);
    // get parent rect and update on resize
    useLayoutEffect(()=>{
        function update(){
            if(parentRef?.current) setRect(parentRef.current.getBoundingClientRect());
        }
        update();
        window.addEventListener('resize', update);
        return () => window.removeEventListener('resize', update);
    }, [parentRef]);

    // Dragging state for the note itself
    const draggingNoteRef = useRef(false);
    const startMouseRef = useRef<{x:number,y:number} | null>(null);
    const startPosRef = useRef<{x:number,y:number} | null>(null);
    const [, setTick] = useState(0); // to force render during drag

    function handleNoteMouseDown(e: React.MouseEvent){
        e.stopPropagation();
        if (e.button !== 0) return;
        // If we're in edit mode or clicking an interactive element, don't start dragging.
        // This allows selecting text inside inputs/textareas when editing.
        const target = e.target as HTMLElement | null;
        if (isEditing) return;
        if (target && target.closest && target.closest('input,textarea,select,button,a')) return;

        draggingNoteRef.current = true;
        startMouseRef.current = { x: e.clientX, y: e.clientY };
        startPosRef.current = { x: note.x, y: note.y };
    }
    function handleNoteMouseMove(e: React.MouseEvent){
        if(!draggingNoteRef.current || !startMouseRef.current || !startPosRef.current) return;
        e.stopPropagation();
        const dx = e.clientX - startMouseRef.current.x;
        const dy = e.clientY - startMouseRef.current.y;
        const newX = startPosRef.current.x + dx;
        const newY = startPosRef.current.y + dy;
        if (onMove) onMove(newX, newY);
        setTick(t => t+1);
    }
    function handleNoteMouseUp(e: React.MouseEvent){
        if(!draggingNoteRef.current) return;
        draggingNoteRef.current = false;
        e.stopPropagation();
        if(!startMouseRef.current || !startPosRef.current) return;
        const dx = e.clientX - startMouseRef.current.x;
        const dy = e.clientY - startMouseRef.current.y;
        const newX = startPosRef.current.x + dx;
        const newY = startPosRef.current.y + dy;
        if (onMoveEnd) onMoveEnd(newX, newY);
        startMouseRef.current = null;
        startPosRef.current = null;
    }

    // Simple edit mode
    const [isEditing, setIsEditing] = useState(false);
    const [editTitle, setEditTitle] = useState(note.title ?? '');
    const [editContent, setEditContent] = useState(note.content ?? '');

    useEffect(()=>{
        // sync edits when external note changes
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setEditTitle(note.title ?? '');
        setEditContent(note.content ?? '');
    }, [note.title, note.content]);

    function handleDoubleClick(e: React.MouseEvent){
        e.stopPropagation();
        setIsEditing(true);
        setEditTitle(note.title ?? '');
        setEditContent(note.content ?? '');
    }

    function submitEdit(){
        setIsEditing(false);
        if (onUpdate) { onUpdate({ title: editTitle, content: editContent }); }
    }

    // compute note position based on x/y/offset (note size comes from CSS)
    const noteWidth = 200;
    const noteHeight = 140;
    const nx = (note?.x ?? 0) + (rect ? rect.width/2 : 0) + offset.x - noteWidth/2;
    const ny = (note?.y ?? 0) + (rect ? rect.height/2 : 0) + offset.y - noteHeight/2;

    return (
        <div
            className="note"
            style={{ left: nx, top: ny }}
            onMouseDown={handleNoteMouseDown}
            onMouseMove={handleNoteMouseMove}
            onMouseUp={handleNoteMouseUp}
            onMouseLeave={handleNoteMouseUp}
            onDoubleClick={handleDoubleClick}
        >
            <div className="note-header">
                <strong className="note-title">{note.title ?? 'Untitled'}</strong>
                <button onClick={(e)=>{ e.stopPropagation(); onDelete(); }} className="note-delete-button">✕</button>
            </div>

            {!isEditing ? (
                <div className="note-content">
                    {note.content ?? ''}
                </div>
            ) : (
                <div className="note-edit">
                    <input value={editTitle} onChange={e=>setEditTitle(e.target.value)} />
                    <textarea value={editContent} onChange={e=>setEditContent(e.target.value)} />
                    <div className="controls">
                        <button onClick={submitEdit}>Save</button>
                        <button onClick={(e)=>{ e.stopPropagation(); setIsEditing(false); }}>Cancel</button>
                    </div>
                </div>
            )}
        </div>
    );
}
