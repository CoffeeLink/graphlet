import { useCallback, useEffect, useMemo, useState } from 'react';
import './settingsRightComponents.css';

interface Organization {
    id: string;
    name: string;
}

interface Workspace {
    id: string;
    name: string;
}

type AccessLevel = 'Read' | 'Write' | 'Admin' | 'Owner';

interface OrgAccess {
    userId: string;
    orgId: string;
    accessLevel: AccessLevel;
    invitedBy?: string | null;
}

export default function SettingsRightSharedItems() {
    const [organizations, setOrganizations] = useState<Organization[]>([]);
    const [userWorkspaces, setUserWorkspaces] = useState<Workspace[]>([]);
    const [orgWorkspaces, setOrgWorkspaces] = useState<Record<string, string[]>>({});
    const [orgAccessLevel, setOrgAccessLevel] = useState<Record<string, AccessLevel>>({});

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const authHeader = useMemo(() => ({
        Authorization: `Bearer ${localStorage.getItem('token')}`
    }), []);

    const fetchOrgWorkspaces = useCallback(async (orgId: string) => {
        const response = await fetch(`http://localhost:5188/api/organization/${orgId}/workspaces`, {
            headers: authHeader
        });

        if (!response.ok) return;

        const workspaceIds: string[] = await response.json();
        setOrgWorkspaces(prev => ({ ...prev, [orgId]: workspaceIds }));
    }, [authHeader]);

    const fetchOrgAccessLevel = useCallback(async (orgId: string) => {
        const response = await fetch(`http://localhost:5188/api/access/organization/${orgId}`, {
            headers: authHeader
        });

        if (!response.ok) return;

        const access: OrgAccess = await response.json();
        setOrgAccessLevel(prev => ({ ...prev, [orgId]: access.accessLevel }));
    }, [authHeader]);

    const fetchOrganizations = useCallback(async () => {
        const response = await fetch('http://localhost:5188/api/organization', {
            headers: authHeader
        });

        if (!response.ok) {
            setError('Failed to fetch organizations');
            return;
        }

        const data: Organization[] = await response.json();
        setOrganizations(data);

        await Promise.all(
            data.map(org => Promise.all([fetchOrgWorkspaces(org.id), fetchOrgAccessLevel(org.id)]))
        );
    }, [authHeader, fetchOrgAccessLevel, fetchOrgWorkspaces]);

    const fetchUserWorkspaces = useCallback(async () => {
        const response = await fetch('http://localhost:5188/api/workspace', {
            headers: authHeader
        });

        if (!response.ok) {
            setError('Failed to fetch workspaces');
            return;
        }

        const data: Workspace[] = await response.json();
        setUserWorkspaces(data);
    }, [authHeader]);

    const fetchData = useCallback(async () => {
        setLoading(true);
        setError(null);

        try {
            await Promise.all([fetchOrganizations(), fetchUserWorkspaces()]);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        } finally {
            setLoading(false);
        }
    }, [fetchOrganizations, fetchUserWorkspaces]);

    useEffect(() => {
        void fetchData();
    }, [fetchData]);

    const getWorkspaceName = (id: string) => {
        const ws = userWorkspaces.find(w => w.id === id);
        return ws ? ws.name : id;
    };

    const handleUnshare = async (orgId: string, workspaceId: string) => {
        const isOwner = orgAccessLevel[orgId] === 'Owner';
        if (!isOwner) return;
        if (!confirm('Unshare this workspace from the organization?')) return;

        try {
            const response = await fetch(`http://localhost:5188/api/access/organization/${orgId}/workspace/${workspaceId}`, {
                method: 'DELETE',
                headers: authHeader
            });

            if (!response.ok) {
                const msg = await response.text().catch(() => '');
                setError(msg || 'Failed to unshare workspace');
                return;
            }

            setOrgWorkspaces(prev => {
                const current = prev[orgId] ?? [];
                return { ...prev, [orgId]: current.filter(id => id !== workspaceId) };
            });
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        }
    };

    const orgsWithSharedWorkspaces = useMemo(() => {
        // Only organization Owners should see/manage the organization's shared workspaces.
        return organizations
            .map(org => ({
                org,
                workspaceIds: orgWorkspaces[org.id] ?? [],
                isOwner: orgAccessLevel[org.id] === 'Owner'
            }))
            .filter(x => x.isOwner && x.workspaceIds.length > 0);
    }, [organizations, orgWorkspaces, orgAccessLevel]);

    if (loading && organizations.length === 0) return <div>Loading...</div>;

    return (
        <section className="settings-right-component">
            <h1>Shared Items</h1>
            {error && <div className="error">{error}</div>}

            <div className="section">
                <h3>Organization-shared workspaces</h3>

                {orgsWithSharedWorkspaces.length === 0 ? (
                    <p>No organization-shared workspaces found.</p>
                ) : (
                    <ul className="list">
                        {orgsWithSharedWorkspaces.map(({ org, workspaceIds }) => {
                            const isOwner = orgAccessLevel[org.id] === 'Owner';
                            const ownerOnlyTitle = isOwner ? undefined : 'Only organization owners can unshare workspaces.';

                            return (
                                <li key={org.id} className="org-item">
                                    <div className="org-header">
                                        <span className="org-name">{org.name}</span>
                                    </div>

                                    <div className="org-workspaces">
                                        <ul className="sub-list">
                                            {workspaceIds.map(wsId => (
                                                <li key={wsId} className="sub-list-item">
                                                    <span>{getWorkspaceName(wsId)}</span>
                                                    <button
                                                        type="button"
                                                        className="remove-ws-btn"
                                                        onClick={() => handleUnshare(org.id, wsId)}
                                                        disabled={!isOwner}
                                                        title={ownerOnlyTitle}
                                                    >
                                                        Unshare
                                                    </button>
                                                </li>
                                            ))}
                                        </ul>
                                    </div>
                                </li>
                            );
                        })}
                    </ul>
                )}
            </div>
        </section>
    );
}