import Login from '../../pages/login.tsx';
import Register from '../../pages/register.tsx';
import Settings from '../../pages/settings.tsx';
import Workspaces from "../../pages/workspaces.tsx";
import SettingsRightAppearance from "../settings/rightsidePages/settingsRightAppearance.tsx";
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
                element: <Navigate to="/settings/appearance" replace/>
            },
            {
                path: "appearance",
                element: <SettingsRightAppearance/>
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
