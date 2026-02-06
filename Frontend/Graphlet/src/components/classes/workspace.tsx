export class Workspace {
    id: string | null = null;
    name: string;
    createdAt: Date;

    constructor(id: string | null, name: string, createdAt: Date) {
        this.id = id;
        this.name = name;
        this.createdAt = createdAt;
    }
}

