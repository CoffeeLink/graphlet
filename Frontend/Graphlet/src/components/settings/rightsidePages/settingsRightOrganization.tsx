import React, { useState, useEffect } from 'react';
import './settingsRightComponents.css';

interface Organization {
    id: string;
    name: string;
}

interface Workspace {
    id: string;
    name: string;
}

export default function SettingsRightOrganization() {
    const [organizations, setOrganizations] = useState<Organization[]>([]);
    const [userWorkspaces, setUserWorkspaces] = useState<Workspace[]>([]);
    const [orgWorkspaces, setOrgWorkspaces] = useState<Record<string, string[]>>({}); // OrgId -> WorkspaceIds
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [createName, setCreateName] = useState('');
    const [selectedWorkspace, setSelectedWorkspace] = useState<Record<string, string>>({}); // OrgId -> Selected WorkspaceId for adding

    useEffect(() => {
        fetchData();
    }, []);

    const fetchData = async () => {
        setLoading(true);
        try {
            await Promise.all([fetchOrganizations(), fetchUserWorkspaces()]);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        } finally {
            setLoading(false);
        }
    };

const fetchOrganizations = async () => {
    const response = await fetch('http://localhost:5188/api/organization', {
        headers: {
            'Authorization': `Bearer ${localStorage.getItem("token")}`
        }
    });

    if (!response.ok) throw new Error('Failed to fetch organizations');
    const data = await response.json();
    setOrganizations(data);
    
    // Fetch workspaces for each organization
    data.forEach((org: Organization) => fetchOrgWorkspaces(org.id));
};

const fetchUserWorkspaces = async () => {
    const response = await fetch('http://localhost:5188/api/workspace', {
        headers: {
            'Authorization': `Bearer ${localStorage.getItem("token")}`
        }
    });
    if (!response.ok) throw new Error('Failed to fetch workspaces');
    const data = await response.json();
    setUserWorkspaces(data);
};

const fetchOrgWorkspaces = async (orgId: string) => {
    try {
        const response = await fetch(`http://localhost:5188/api/organization/${orgId}/workspaces`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem("token")}`
            }
        });
        if (response.ok) {
            const workspaceIds = await response.json();
            setOrgWorkspaces(prev => ({ ...prev, [orgId]: workspaceIds }));
        }
    } catch (err) {
        console.error(`Failed to fetch workspaces for org ${orgId}`, err);
    }
};

const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
        const response = await fetch('http://localhost:5188/api/organization', {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem("token")}`
            },
            body: JSON.stringify({ name: createName })
        });
        if (!response.ok) throw new Error('Failed to create organization');
        setCreateName('');
        await fetchOrganizations();
    } catch (err) {
        setError(err instanceof Error ? err.message : 'Unknown error');
    }
};

const handleDelete = async (id: string) => {
    if(!confirm("Are you sure you want to delete this organization?")) return;
    try {
        const response = await fetch(`http://localhost:5188/api/organization/${id}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem("token")}`
            }
        });
        if (!response.ok) throw new Error('Failed to delete organization');
        // Remove from state immediately for better UX
        setOrganizations(prev => prev.filter(o => o.id !== id));
    } catch (err) {
        setError(err instanceof Error ? err.message : 'Unknown error');
    }
};

const handleAddWorkspace = async (orgId: string) => {
    const workspaceId = selectedWorkspace[orgId];
    if (!workspaceId) return;

    try {
        const response = await fetch(`http://localhost:5188/api/access/organization/${orgId}/workspace/${workspaceId}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem("token")}`
            }
        });
        if (!response.ok) throw new Error('Failed to add workspace to organization');
        
        await fetchOrgWorkspaces(orgId);
        // reset selection
        setSelectedWorkspace(prev => ({ ...prev, [orgId]: "" }));
    } catch (err) {
        setError(err instanceof Error ? err.message : 'Unknown error');
    }
};

const handleRemoveWorkspace = async (orgId: string, workspaceId: string) => {
    if(!confirm("Remove workspace from organization?")) return;
    try {
        const response = await fetch(`http://localhost:5188/api/access/organization/${orgId}/workspace/${workspaceId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem("token")}`
            }
        });
        if (!response.ok) throw new Error('Failed to remove workspace from organization');
        
        await fetchOrgWorkspaces(orgId);
    } catch (err) {
        setError(err instanceof Error ? err.message : 'Unknown error');
    }
};

    const getWorkspaceName = (id: string) => {
        const ws = userWorkspaces.find(w => w.id === id);
        return ws ? ws.name : id;
    };

    if (loading && organizations.length === 0) return <div>Loading...</div>;

    return (
        <section className="settings-right-component">
            <h1>Organization Settings</h1>
            {error && <div className="error">{error}</div>}
            
            <div className="section">
                <h3>Create Organization</h3>
                <form onSubmit={handleCreate}>
                    <input 
                        type="text" 
                        value={createName}
                        onChange={(e) => setCreateName(e.target.value)}
                        placeholder="Organization Name"
                        required
                    />
                    <button type="submit">Create</button>
                </form>
            </div>

            <div className="section">
                <h3>Your Organizations</h3>
                {organizations.length === 0 ? <p>No organizations found.</p> : (
                    <ul className="list">
                        {organizations.map(org => (
                            <li key={org.id} className="org-item">
                                <div className="org-header">
                                    <span className="org-name">{org.name}</span>
                                    <button onClick={() => handleDelete(org.id)} className="delete-btn">Delete Organization</button>
                                </div>
                                
                                <div className="org-workspaces">
                                    <h4>Workspaces:</h4>
                                    <ul className="sub-list">
                                        {(orgWorkspaces[org.id] || []).map(wsId => (
                                            <li key={wsId} className="sub-list-item">
                                                <span>{getWorkspaceName(wsId)}</span>
                                                <button onClick={() => handleRemoveWorkspace(org.id, wsId)} className="remove-ws-btn">Remove</button>
                                            </li>
                                        ))}
                                    </ul>

                                    <div className="add-workspace">
                                        <select 
                                            value={selectedWorkspace[org.id] || ""}
                                            onChange={(e) => setSelectedWorkspace({...selectedWorkspace, [org.id]: e.target.value})}
                                        >
                                            <option value="">Select workspace to add...</option>
                                            {userWorkspaces
                                                .filter(ws => !(orgWorkspaces[org.id] || []).includes(ws.id))
                                                .map(ws => (
                                                    <option key={ws.id} value={ws.id}>
                                                        {ws.name}
                                                    </option>
                                                ))
                                            }
                                        </select>
                                        <button onClick={() => handleAddWorkspace(org.id)} disabled={!selectedWorkspace[org.id]}>Add Workspace</button>
                                    </div>
                                </div>
                            </li>
                        ))}
                    </ul>
                )}
            </div>
        </section>
    );
}




