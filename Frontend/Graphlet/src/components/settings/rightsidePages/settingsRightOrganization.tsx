import React, { useCallback, useEffect, useMemo, useState } from 'react';
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

interface PublicUser {
    id: string;
    username: string;
    profilePicUrl?: string | null;
}

type OrgMemberEdits = Record<string, Record<string, AccessLevel>>; // orgId -> userId -> editedLevel

type LoadingMap = Record<string, boolean>; // orgId -> isLoading

type ErrorMap = Record<string, string | null>; // orgId -> error

export default function SettingsRightOrganization() {
    const [organizations, setOrganizations] = useState<Organization[]>([]);
    const [userWorkspaces, setUserWorkspaces] = useState<Workspace[]>([]);
    const [orgWorkspaces, setOrgWorkspaces] = useState<Record<string, string[]>>({});
    const [orgAccessLevel, setOrgAccessLevel] = useState<Record<string, AccessLevel>>({});

    // Member management (Owner-only UI)
    const [meId, setMeId] = useState<string | null>(null);
    const [orgMembers, setOrgMembers] = useState<Record<string, OrgAccess[]>>({});
    const [orgMembersLoading, setOrgMembersLoading] = useState<LoadingMap>({});
    const [orgMembersError, setOrgMembersError] = useState<ErrorMap>({});
    const [memberEdits, setMemberEdits] = useState<OrgMemberEdits>({});
    const [publicUsers, setPublicUsers] = useState<Record<string, PublicUser>>({});

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [createName, setCreateName] = useState('');
    const [selectedWorkspace, setSelectedWorkspace] = useState<Record<string, string>>({});
    const [inviteTarget, setInviteTarget] = useState<Record<string, string>>({});
    const [inviteAccessLevel, setInviteAccessLevel] = useState<Record<string, string>>({});

    const authHeader = useMemo(() => ({
        'Authorization': `Bearer ${localStorage.getItem('token')}`
    }), []);

    const fetchMe = useCallback(async () => {
        try {
            const response = await fetch('http://localhost:5188/api/user/me', {
                headers: authHeader
            });

            if (!response.ok) {
                return;
            }

            const data = await response.json();
            if (data?.id) {
                setMeId(String(data.id));
            }
        } catch (err) {
            console.error('Failed to fetch current user', err);
        }
    }, [authHeader]);

    const fetchOrgWorkspaces = useCallback(async (orgId: string) => {
        try {
            const response = await fetch(`http://localhost:5188/api/organization/${orgId}/workspaces`, {
                headers: authHeader
            });

            if (!response.ok) {
                return;
            }

            const workspaceIds = await response.json();
            setOrgWorkspaces(prev => ({ ...prev, [orgId]: workspaceIds }));
        } catch (err) {
            console.error(`Failed to fetch workspaces for org ${orgId}`, err);
        }
    }, [authHeader]);

    const fetchOrgAccessLevel = useCallback(async (orgId: string) => {
        try {
            const response = await fetch(`http://localhost:5188/api/access/organization/${orgId}`, {
                headers: authHeader
            });

            if (!response.ok) {
                return;
            }

            const access: OrgAccess = await response.json();
            setOrgAccessLevel(prev => ({ ...prev, [orgId]: access.accessLevel }));
        } catch (err) {
            console.error(`Failed to fetch access for org ${orgId}`, err);
        }
    }, [authHeader]);

    const fetchPublicUsers = useCallback(async (userIds: string[]) => {
        const missing = userIds.filter(id => !!id && !publicUsers[id]);
        if (missing.length === 0) return;

        try {
            const results = await Promise.all(
                missing.map(async (id) => {
                    try {
                        const resp = await fetch(`http://localhost:5188/api/user/${encodeURIComponent(id)}`);
                        if (!resp.ok) return null;
                        const data = await resp.json();
                        return {
                            id: String(data.id ?? id),
                            username: String(data.username ?? id),
                            profilePicUrl: data.profilePicUrl ?? null
                        } satisfies PublicUser;
                    } catch {
                        return null;
                    }
                })
            );

            const next: Record<string, PublicUser> = {};
            for (const u of results) {
                if (!u) continue;
                next[u.id] = u;
            }

            if (Object.keys(next).length > 0) {
                setPublicUsers(prev => ({ ...prev, ...next }));
            }
        } catch (err) {
            console.error('Failed to prefetch public users', err);
        }
    }, [publicUsers]);

    const fetchOrgMembers = useCallback(async (orgId: string) => {
        setOrgMembersLoading(prev => ({ ...prev, [orgId]: true }));
        setOrgMembersError(prev => ({ ...prev, [orgId]: null }));

        try {
            const response = await fetch(`http://localhost:5188/api/access/organization/${orgId}/list`, {
                headers: authHeader
            });

            if (!response.ok) {
                const msg = await response.text().catch(() => '');
                setOrgMembersError(prev => ({ ...prev, [orgId]: msg || 'Failed to fetch organization members' }));
                return;
            }

            const list: OrgAccess[] = await response.json();
            setOrgMembers(prev => ({ ...prev, [orgId]: list }));

            // Fetch usernames for display
            void fetchPublicUsers(list.map(m => String(m.userId)));

            // Initialize edit values to current values (only for users not already edited)
            setMemberEdits(prev => {
                const orgPrev = prev[orgId] ?? {};
                const nextOrg = { ...orgPrev };
                for (const entry of list) {
                    const uid = String(entry.userId);
                    if (!nextOrg[uid]) {
                        nextOrg[uid] = entry.accessLevel;
                    }
                }
                return { ...prev, [orgId]: nextOrg };
            });
        } catch (err) {
            console.error(`Failed to fetch members for org ${orgId}`, err);
            setOrgMembersError(prev => ({ ...prev, [orgId]: 'Failed to fetch organization members' }));
        } finally {
            setOrgMembersLoading(prev => ({ ...prev, [orgId]: false }));
        }
    }, [authHeader, fetchPublicUsers]);

    const fetchOrganizations = useCallback(async () => {
        const response = await fetch('http://localhost:5188/api/organization', {
            headers: authHeader
        });

        if (!response.ok) {
            setError('Failed to fetch organizations');
            return;
        }

        const data = await response.json();
        setOrganizations(data);
        await Promise.all(
            data.map((org: Organization) =>
                Promise.all([fetchOrgWorkspaces(org.id), fetchOrgAccessLevel(org.id)])
            )
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

        const data = await response.json();
        setUserWorkspaces(data);
    }, [authHeader]);

    const fetchData = useCallback(async () => {
        setLoading(true);
        setError(null);

        try {
            await Promise.all([fetchOrganizations(), fetchUserWorkspaces(), fetchMe()]);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        } finally {
            setLoading(false);
        }
    }, [fetchMe, fetchOrganizations, fetchUserWorkspaces]);

    useEffect(() => {
        void fetchData();
    }, [fetchData]);

    // Auto-load member lists for orgs where the user is Owner.
    useEffect(() => {
        for (const org of organizations) {
            const isOwner = orgAccessLevel[org.id] === 'Owner';
            if (!isOwner) continue;

            if (orgMembers[org.id] == null && !orgMembersLoading[org.id]) {
                void fetchOrgMembers(org.id);
            }
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [organizations, orgAccessLevel]);

    const handleCreate = async (e: React.FormEvent) => {
        e.preventDefault();

        try {
            const response = await fetch('http://localhost:5188/api/organization', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...authHeader
                },
                body: JSON.stringify({ name: createName })
            });

            if (!response.ok) {
                setError('Failed to create organization');
                return;
            }

            setCreateName('');
            await fetchOrganizations();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        }
    };

    const handleDelete = async (id: string) => {
        if (!confirm('Are you sure you want to delete this organization?')) return;

        try {
            const response = await fetch(`http://localhost:5188/api/organization/${id}`, {
                method: 'DELETE',
                headers: authHeader
            });

            if (!response.ok) {
                setError('Failed to delete organization');
                return;
            }

            setOrganizations(prev => prev.filter(o => o.id !== id));
            setOrgWorkspaces(prev => {
                const copy = { ...prev };
                delete copy[id];
                return copy;
            });
            setOrgAccessLevel(prev => {
                const copy = { ...prev };
                delete copy[id];
                return copy;
            });
            setOrgMembers(prev => {
                const copy = { ...prev };
                delete copy[id];
                return copy;
            });
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
                headers: authHeader
            });

            if (!response.ok) {
                setError('Failed to add workspace to organization');
                return;
            }

            await fetchOrgWorkspaces(orgId);
            setSelectedWorkspace(prev => ({ ...prev, [orgId]: '' }));
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        }
    };

    const handleInviteUser = async (orgId: string) => {
        const targetUserId = inviteTarget[orgId];
        const accessLevel = inviteAccessLevel[orgId] || 'Write';
        if (!targetUserId) return;

        try {
            const response = await fetch(`http://localhost:5188/api/access/organization/${orgId}/invite`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...authHeader
                },
                body: JSON.stringify({ targetUserId, accessLevel })
            });

            if (!response.ok) {
                setError('Failed to invite user to organization');
                return;
            }

            setInviteTarget(prev => ({ ...prev, [orgId]: '' }));
            setInviteAccessLevel(prev => ({ ...prev, [orgId]: 'Write' }));

            // Refresh members so the newly invited user appears in the list once accepted/granted.
            if (orgAccessLevel[orgId] === 'Owner') {
                await fetchOrgMembers(orgId);
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        }
    };

    const handleRemoveWorkspace = async (orgId: string, workspaceId: string) => {
        if (!confirm('Remove workspace from organization?')) return;

        try {
            const response = await fetch(`http://localhost:5188/api/access/organization/${orgId}/workspace/${workspaceId}`, {
                method: 'DELETE',
                headers: authHeader
            });

            if (!response.ok) {
                setError('Failed to remove workspace from organization');
                return;
            }

            await fetchOrgWorkspaces(orgId);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        }
    };

    const handleUpdateMemberAccess = async (orgId: string, targetUserId: string) => {
        const desiredLevel = memberEdits[orgId]?.[targetUserId];
        if (!desiredLevel) return;

        try {
            const response = await fetch(`http://localhost:5188/api/access/organization/${orgId}/grant`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...authHeader
                },
                body: JSON.stringify({ targetUserId, accessLevel: desiredLevel })
            });

            if (!response.ok) {
                const msg = await response.text().catch(() => '');
                setError(msg || 'Failed to update member access');
                return;
            }

            await fetchOrgMembers(orgId);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        }
    };

    const handleRemoveMember = async (orgId: string, targetUserId: string) => {
        if (!confirm('Remove this user from the organization?')) return;

        try {
            const response = await fetch(`http://localhost:5188/api/access/organization/${orgId}/revoke/${targetUserId}`, {
                method: 'DELETE',
                headers: authHeader
            });

            if (!response.ok) {
                const msg = await response.text().catch(() => '');
                setError(msg || 'Failed to remove member');
                return;
            }

            setOrgMembers(prev => {
                const existing = prev[orgId] ?? [];
                return { ...prev, [orgId]: existing.filter(m => String(m.userId) !== targetUserId) };
            });
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        }
    };

    const getMemberColor = (level: AccessLevel): React.CSSProperties['color'] => {
        if (level === 'Owner') return 'red';
        if (level === 'Write') return 'green';
        // Read -> default (no color), Admin -> default (no color)
        return undefined;
    };

    const getMemberLabel = (userId: string): string => {
        const u = publicUsers[userId];
        return u?.username ? u.username : userId;
    };

    const columnDividerStyle: React.CSSProperties = {
        borderLeft: '1px solid rgba(0, 0, 0, 0.12)',
        paddingLeft: 10,
        paddingRight: 10
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
                        {organizations.map(org => {
                            const isOwner = orgAccessLevel[org.id] === 'Owner';
                            const ownerOnlyTitle = isOwner ? undefined : 'Only organization owners can perform this action.';

                            return (
                                <li key={org.id} className="org-item">
                                    <div className="org-header">
                                        <span className="org-name">{org.name}</span>
                                        <button
                                            onClick={() => handleDelete(org.id)}
                                            className="delete-btn"
                                            disabled={!isOwner}
                                            title={ownerOnlyTitle}
                                        >
                                            Delete Organization
                                        </button>
                                    </div>

                                    <div className="org-workspaces">
                                        <h4>Workspaces:</h4>
                                        <ul className="sub-list">
                                            {(orgWorkspaces[org.id] || []).map(wsId => (
                                                <li key={wsId} className="sub-list-item">
                                                    <span>{getWorkspaceName(wsId)}</span>
                                                    <button
                                                        onClick={() => handleRemoveWorkspace(org.id, wsId)}
                                                        className="remove-ws-btn"
                                                        disabled={!isOwner}
                                                        title={ownerOnlyTitle}
                                                    >
                                                        Remove
                                                    </button>
                                                </li>
                                            ))}
                                        </ul>

                                        <div className="add-workspace">
                                            <select
                                                value={selectedWorkspace[org.id] || ''}
                                                onChange={(e) => setSelectedWorkspace({ ...selectedWorkspace, [org.id]: e.target.value })}
                                                disabled={!isOwner}
                                                title={ownerOnlyTitle}
                                            >
                                                <option className={"option-item"} value="">Select workspace to add...</option>
                                                {userWorkspaces
                                                    .filter(ws => !(orgWorkspaces[org.id] || []).includes(ws.id))
                                                    .map(ws => (
                                                        <option className={"option-item"} key={ws.id} value={ws.id}>
                                                            {ws.name}
                                                        </option>
                                                    ))}
                                            </select>
                                            <button
                                                onClick={() => handleAddWorkspace(org.id)}
                                                disabled={!isOwner || !selectedWorkspace[org.id]}
                                                title={ownerOnlyTitle}
                                            >
                                                Add Workspace
                                            </button>
                                        </div>

                                        <div className="add-workspace">
                                            <input
                                                type="text"
                                                value={inviteTarget[org.id] || ''}
                                                onChange={(e) => setInviteTarget({ ...inviteTarget, [org.id]: e.target.value })}
                                                placeholder="Target user ID"
                                                disabled={!isOwner}
                                                title={ownerOnlyTitle}
                                            />
                                            <select
                                                value={inviteAccessLevel[org.id] || 'Write'}
                                                onChange={(e) => setInviteAccessLevel({ ...inviteAccessLevel, [org.id]: e.target.value })}
                                                disabled={!isOwner}
                                                title={ownerOnlyTitle}
                                            >
                                                <option className={"option-item"} value="Read">Read</option>
                                                <option className={"option-item"} value="Write">Write</option>
                                                <option className={"option-item"} value="Admin">Admin</option>
                                                <option className={"option-item"} value="Owner">Owner</option>
                                            </select>
                                            <button
                                                onClick={() => handleInviteUser(org.id)}
                                                disabled={!isOwner || !inviteTarget[org.id]}
                                                title={ownerOnlyTitle}
                                            >
                                                Invite User
                                            </button>
                                        </div>

                                        {isOwner && (
                                            <div className="section" style={{ marginTop: 12 }}>
                                                <h4>Organization members</h4>

                                                {orgMembersError[org.id] && <div className="error">{orgMembersError[org.id]}</div>}

                                                <div style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 8 }}>
                                                    <button
                                                        type="button"
                                                        onClick={() => fetchOrgMembers(org.id)}
                                                        disabled={orgMembersLoading[org.id]}
                                                    >
                                                        {orgMembersLoading[org.id] ? 'Loading...' : 'Refresh member list'}
                                                    </button>
                                                </div>

                                                {(orgMembers[org.id] ?? []).length === 0 ? (
                                                    <p>No members found.</p>
                                                ) : (
                                                    <ul className="sub-list">
                                                        <li
                                                            className="sub-list-item"
                                                            style={{
                                                                display: 'flex',
                                                                gap: 0,
                                                                alignItems: 'center',
                                                                fontWeight: 600,
                                                                opacity: 0.85
                                                            }}
                                                        >
                                                            <span style={{ flex: 1, paddingRight: 10 }}>User</span>
                                                            <span style={{ ...columnDividerStyle, minWidth: 110 }}>Access</span>
                                                            <span style={{ ...columnDividerStyle, minWidth: 60 }}>Save</span>
                                                            <span style={{ ...columnDividerStyle, minWidth: 70 }}>Remove</span>
                                                        </li>

                                                        {(orgMembers[org.id] ?? []).map(member => {
                                                            const memberId = String(member.userId);
                                                            const isMe = meId != null && memberId === meId;
                                                            const editLevel = memberEdits[org.id]?.[memberId] ?? member.accessLevel;
                                                            const selfTitle = isMe ? 'You cannot modify your own access here.' : undefined;
                                                            const name = getMemberLabel(memberId);
                                                            const color = getMemberColor(member.accessLevel);

                                                            return (
                                                                <li key={memberId} className="sub-list-item" style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
                                                                    <span
                                                                        style={{ flex: 1, wordBreak: 'break-all', color }}
                                                                        title={name !== memberId ? memberId : undefined}
                                                                    >
                                                                        {name}{name !== memberId ? ` (${memberId})` : ''}
                                                                    </span>

                                                                    <select
                                                                        value={editLevel}
                                                                        onChange={(e) => {
                                                                            const nextLevel = e.target.value as AccessLevel;
                                                                            setMemberEdits(prev => ({
                                                                                ...prev,
                                                                                [org.id]: { ...(prev[org.id] ?? {}), [memberId]: nextLevel }
                                                                            }));
                                                                        }}
                                                                        disabled={isMe}
                                                                        title={selfTitle}
                                                                    >
                                                                        <option className={"option-item"} value="Read">Read</option>
                                                                        <option className={"option-item"} value="Write">Write</option>
                                                                        {/*<option className={"option-item"} value="Admin">Admin</option>*/}
                                                                        <option className={"option-item"} value="Owner">Owner</option>
                                                                    </select>

                                                                    <button
                                                                        type="button"
                                                                        onClick={() => handleUpdateMemberAccess(org.id, memberId)}
                                                                        disabled={isMe}
                                                                        title={selfTitle}
                                                                    >
                                                                        Save
                                                                    </button>

                                                                    <button
                                                                        type="button"
                                                                        onClick={() => handleRemoveMember(org.id, memberId)}
                                                                        disabled={isMe}
                                                                        title={selfTitle}
                                                                    >
                                                                        Remove
                                                                    </button>
                                                                </li>
                                                            );
                                                        })}
                                                    </ul>
                                                )}
                                            </div>
                                        )}
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
