 export class Tag {
    id: string;
    name: string;
    color: number[];
    workspaceId: string;

    constructor(id: string, name: string, color: number[], workspaceId: string) {
        this.id = id;
        this.name = name;
        this.color = color;
        this.workspaceId = workspaceId;
    }

    static async getAllTagForWorkspace(workspaceId: string): Promise<Tag[]> {
        const response = await fetch(`/api/workspace/${workspaceId}/tags`);
        if (!response.ok) {
            throw new Error(`Error fetching tags: ${response.statusText}`);
        }
        const data: { id: string, name: string, color: number[] }[] = await response.json();
        return data.map((t) => new Tag(t.id, t.name, t.color, workspaceId));
    }

    static async getTagByTagId(workspaceId: string, tagId: string): Promise<Tag> {
        const response = await fetch(`/api/workspace/${workspaceId}/tags/${tagId}`);
        if (!response.ok) {
            throw new Error(`Error fetching tag: ${response.statusText}`);
        }
        const data: { id: string, name: string, color: number[] } = await response.json();
        return new Tag(data.id, data.name, data.color, workspaceId);
    }

    static async postNewTag(workspaceId: string, name: string, color: number[]): Promise<void> {
        const response = await fetch(`/api/workspace/${workspaceId}/tags`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ name, color })
        });
        if (!response.ok) {
            throw new Error(`Error creating tag: ${response.statusText}`);
        }
    }

    static async putTag(workspaceId: string, tagId: string, name: string, color: number[]): Promise<void> {
        const response = await fetch(`/api/workspace/${workspaceId}/tags/${tagId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ name, color })
        });
        if (!response.ok) {
            throw new Error(`Error updating tag: ${response.statusText}`);
        }
    }

    static async deleteTag(workspaceId: string, tagId: string): Promise<void> {
        const response = await fetch(`/api/workspace/${workspaceId}/tags/${tagId}`, {
            method: 'DELETE'
        });
        if (!response.ok) {
            throw new Error(`Error deleting tag: ${response.statusText}`);
        }
    }

    // Helper to convert array [r, g, b] to hex string #RRGGBB
    get hexColor(): string {
        if (!this.color || this.color.length < 3) return "#000000";
        const [r, g, b] = this.color;
        return "#" + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1).toUpperCase();
    }
}
