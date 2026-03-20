export interface NoteRelation {
    id: string;
    connection: string[]; // Two note IDs
    name: string;
}

export interface Note {
    id: string;
    title?: string;
    content?: string;
    x: number; // position relative to center
    y: number;
    relations: NoteRelation[];
}
