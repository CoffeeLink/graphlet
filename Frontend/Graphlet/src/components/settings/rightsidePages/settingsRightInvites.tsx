import { useState, useEffect } from 'react';
import './settingsRightComponents.css';

type InvitationKind = 'workspace' | 'organization';

interface WorkspaceInvitation {
    id: string;
    workspaceId: string;
    targetUserId: string;
    accessLevel: string;
    inviteMadeBy: string;
    created: string;
    expires: string;
}

interface OrganizationInvitation {
    id: string;
    orgId: string;
    organizationName: string;
    targetUserId: string;
    accessLevel: string;
    inviteMadeBy: string;
    created: string;
    expires: string;
}

interface InvitationItem {
    kind: InvitationKind;
    id: string;
    title: string;
    targetId: string;
    accessLevel: string;
    inviteMadeBy: string;
    created: string;
    expires: string;
}

export default function SettingsRightInvites() {
    const [invitations, setInvitations] = useState<InvitationItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchInvitations = async () => {
            try {
                const token = localStorage.getItem('token');
                const headers = { 'Authorization': `Bearer ${token}` };

                const [workspaceResult, organizationResult] = await Promise.allSettled([
                    fetch('http://localhost:5188/api/access/invitations', { headers }),
                    fetch('http://localhost:5188/api/access/organization-invitations', { headers })
                ]);

                const combined: InvitationItem[] = [];

                if (workspaceResult.status === 'fulfilled') {
                    if (workspaceResult.value.ok) {
                        const data: WorkspaceInvitation[] = await workspaceResult.value.json();
                        combined.push(...data.map(inv => ({
                            kind: 'workspace' as const,
                            id: inv.id,
                            title: `Workspace: ${inv.workspaceId}`,
                            targetId: inv.workspaceId,
                            accessLevel: inv.accessLevel,
                            inviteMadeBy: inv.inviteMadeBy,
                            created: inv.created,
                            expires: inv.expires
                        })));
                    } else {
                        setError(prev => prev ?? 'Failed to fetch workspace invitations');
                    }
                } else {
                    setError(prev => prev ?? 'Error connecting to server');
                }

                if (organizationResult.status === 'fulfilled') {
                    if (organizationResult.value.ok) {
                        const data: OrganizationInvitation[] = await organizationResult.value.json();
                        combined.push(...data.map(inv => ({
                            kind: 'organization' as const,
                            id: inv.id,
                            title: `Organization: ${inv.organizationName || inv.orgId}`,
                            targetId: inv.orgId,
                            accessLevel: inv.accessLevel,
                            inviteMadeBy: inv.inviteMadeBy,
                            created: inv.created,
                            expires: inv.expires
                        })));
                    } else {
                        setError(prev => prev ?? 'Failed to fetch organization invitations');
                    }
                } else {
                    setError(prev => prev ?? 'Error connecting to server');
                }

                setInvitations(combined.sort((a, b) => a.created.localeCompare(b.created)));
            } catch {
                setError('Error connecting to server');

            } finally {
                setLoading(false);
            }
        };

        fetchInvitations();
    }, []);

    const handleAction = async (invitation: InvitationItem, action: 'accept' | 'decline') => {
        try {
            const endpoint = invitation.kind === 'workspace'
                ? `http://localhost:5188/api/access/invitations/${invitation.id}/${action}`
                : `http://localhost:5188/api/access/organization-invitations/${invitation.id}/${action}`;

            const response = await fetch(endpoint, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                }
            });
            if (response.ok) {
                setInvitations(prev => prev.filter(inv => inv.id !== invitation.id));
            } else {
                setError('Action failed');
            }
        } catch {
            setError('Action failed');
        }
    };

    if (loading) return <div>Loading...</div>;

    return (
        <section className="settings-right-component">
            <h1>Invitations</h1>
            {error && <div className="error">{error}</div>}

            {invitations.length === 0 ? (
                <p>No pending invitations.</p>
            ) : (
                <ul className="list">
                    {invitations.map(invitation => (
                        <li key={invitation.id} className="list-item">
                            <span>{invitation.title} · Access: {invitation.accessLevel}</span>
                            <div className="actions">
                                <button onClick={() => handleAction(invitation, 'accept')}>Accept</button>
                                <button onClick={() => handleAction(invitation, 'decline')}>Decline</button>
                            </div>
                        </li>
                    ))}
                </ul>
            )}
        </section>
    );
}
