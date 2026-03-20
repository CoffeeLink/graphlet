import { useState, useEffect, useRef } from 'react';
import PinImg from "../../assets/drawing-pin-146214_1280.png";
import "./relationPin.css";
import * as React from "react";

export interface RelationDisplay {
    id: string; // The relation ID
    name: string; // The name of the relation or the target note
}

interface RelationPinProps {
    relations: RelationDisplay[];
    onStartLinking: () => void;
    onDeleteRelation: (relationId: string) => void;
}

export function RelationPin({ relations, onStartLinking, onDeleteRelation }: RelationPinProps){
    const [isOpen, setIsOpen] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        }

        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, []);

    function handleClick(e: React.MouseEvent){
        e.stopPropagation();
        setIsOpen(!isOpen);
    }

    return(
        <div className="relation-pin-container" ref={containerRef}>
            <img 
                onClick={handleClick} 
                alt={"Pin"} 
                src={PinImg} 
                className="relation-pin-img"
            />
            {isOpen && (
                <div className="relation-pin-menu">
                     <button 
                        className="relation-add-btn"
                        onClick={(e) => { e.stopPropagation(); setIsOpen(false); onStartLinking(); }}
                    >
                        + Add Relation
                    </button>
                    {relations.length > 0 && <hr className="relation-separator" />}
                    <div className="relation-list">
                        {relations.map(rel => (
                            <div key={rel.id} className="relation-item">
                                <span className="relation-item-name" title={rel.name}>{rel.name}</span>
                                <button 
                                    onClick={(e) => { e.stopPropagation(); onDeleteRelation(rel.id); }}
                                    className="relation-delete-btn"
                                >
                                    ✕
                                </button>
                            </div>
                        ))}
                        {relations.length === 0 && <div className="relation-empty">No relations</div>}
                    </div>
                </div>
            )}
        </div>
    )
}