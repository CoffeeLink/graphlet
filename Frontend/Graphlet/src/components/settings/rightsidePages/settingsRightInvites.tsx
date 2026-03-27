import { useState, useEffect } from 'react';
import './settingsRightComponents.css';

interface Invitation {
    id: string;
    workspaceId: string;
    inviterId: string;
    status: string;
}

export default function SettingsRightInvites() {
    const [invitations, setInvitations] = useState<Invitation[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchInvitations = async () => {
            try {
                const response = await fetch('http://localhost:5188/api/access/invitations', {
                    headers: {
                        'Authorization': `Bearer ${localStorage.getItem("token")}`
                    }
                });
                if (response.ok) {
                    const data = await response.json();
                    setInvitations(data);
                } else {
                    setError('Failed to fetch invitations');
                }
                // eslint-disable-next-line @typescript-eslint/no-unused-vars
            } catch (err) {
                setError('Error connecting to server');

            } finally {
                setLoading(false);
            }
        };

        fetchInvitations();
    }, []);

    const handleAction = async (id: string, action: 'accept' | 'decline') => {
        try {
            const response = await fetch(`http://localhost:5188/api/access/invitations/${id}/${action}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem("token")}`
                }
            });
            if (response.ok) {
                setInvitations(prev => prev.filter(inv => inv.id !== id));
            } else {
                setError('Action failed');
            }
        } catch (err) {
            console.error(err);
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
                            <span>Workspace Invite: {invitation.workspaceId}</span>
                            <div className="actions">
                                <button onClick={() => handleAction(invitation.id, 'accept')}>Accept</button>
                                <button onClick={() => handleAction(invitation.id, 'decline')}>Decline</button>
                            </div>
                        </li>
                    ))}
                </ul>
            )}
        </section>
    );
}
