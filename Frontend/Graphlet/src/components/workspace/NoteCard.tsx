import React, {useEffect, useRef, useState, useLayoutEffect} from "react";
import "./workspace.css";
import { RelationPin, type RelationDisplay } from "../classes/relationPin";
import type {Note} from "./types";

export interface NoteCardProps {
    note: Note;
    relations: RelationDisplay[];
    isLinking: boolean;
    onSelect: () => void;
    onStartLinking: () => void;
    onDeleteRelation: (id: string) => void;
    parentRef: React.RefObject<HTMLDivElement | null>;
    offset: {x:number,y:number};
    onDelete: () => void;
    onMove?: (x:number,y:number) => void;
    onMoveEnd?: (x:number,y:number) => void;
    onUpdate?: (patch: Partial<Note>) => void;
}

export function NoteCard({note, relations, isLinking, onSelect, onStartLinking, onDeleteRelation, parentRef, offset, onDelete, onMove, onMoveEnd, onUpdate}: NoteCardProps){
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

        // Linking Interception
        if (isLinking) {
            onSelect();
            return;
        }

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
            id={`note-card-${note.id}`}
            className={`note ${isLinking ? 'cursor-pointer hover:ring-2 hover:ring-blue-500' : ''}`}
            style={{ left: nx, top: ny }}
            onMouseDown={handleNoteMouseDown}
            onMouseMove={handleNoteMouseMove}
            onMouseUp={handleNoteMouseUp}
            onMouseLeave={handleNoteMouseUp}
            onDoubleClick={handleDoubleClick}
        >
            <RelationPin 
                relations={relations} 
                onStartLinking={onStartLinking}
                onDeleteRelation={onDeleteRelation}
            />
            <div className="note-header">
                <strong className="note-title">{note.title ?? 'Untitled'}</strong>
                <button id={`note-delete-btn-${note.id}`} onClick={(e)=>{ e.stopPropagation(); onDelete(); }} className="note-delete-button">✕</button>
            </div>

            {!isEditing ? (
                <div className="note-content">
                    {note.content ?? ''}
                </div>
            ) : (
                <div className="note-edit">
                    <input id={`note-title-input-${note.id}`} value={editTitle} onChange={e=>setEditTitle(e.target.value)} />
                    <textarea id={`note-content-input-${note.id}`} value={editContent} onChange={e=>setEditContent(e.target.value)} />
                    <div className="controls">
                        <button id={`note-save-btn-${note.id}`} onClick={submitEdit}>Save</button>
                        <button id={`note-cancel-btn-${note.id}`} onClick={(e)=>{ e.stopPropagation(); setIsEditing(false); }}>Cancel</button>
                    </div>
                </div>
            )}
        </div>
    );
}

