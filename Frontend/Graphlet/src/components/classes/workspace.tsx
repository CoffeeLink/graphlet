export class Workspace {
    id: string | null = null;
    name: string;

    constructor(id: string | null, name: string) {
        this.id = id;
        this.name = name;
    }
}

