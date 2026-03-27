import Login from '../../pages/login.tsx';
import Register from '../../pages/register.tsx';
import Settings from '../../pages/settings.tsx';
import Workspaces from "../../pages/workspaces.tsx";
import SettingsRightOrganization from "../settings/rightsidePages/settingsRightOrganization.tsx";
import SettingsRightInvites from "../settings/rightsidePages/settingsRightInvites.tsx";
import SettingsRightProfileSettings from "../settings/rightsidePages/settingsRightProfileSettings.tsx";
import SettingsRightSharedItems from "../settings/rightsidePages/settingsRightSharedItems.tsx";
import SettingsRightSubscription from "../settings/rightsidePages/settingsRightSubscription.tsx";
import {Navigate} from 'react-router-dom';


export const ROUTING = [
    {
        path: "/",
        element: <Navigate to="/login" replace/>
    },
    {
        path: "/login",
        element: <Login/>
    },
    {
        path: "/register",
        element: <Register/>
    },
    {
        path: "/workspaces",
        element: <Workspaces/>
    },
    {
        path: "/settings",
        element: <Settings/>,
        children: [
            {
                index: true,
                element: <Navigate to="/settings/organization" replace/>
            },
            {
                path: "organization",
                element: <SettingsRightOrganization/>
            },
            {
                path: "invites",
                element: <SettingsRightInvites/>
            },
            {
                path: "profile",
                element: <SettingsRightProfileSettings/>
            },
            {
                path: "shared",
                element: <SettingsRightSharedItems/>
            },
            {
                path: "subscription",
                element: <SettingsRightSubscription/>
            }
        ]
    },
]
