import SettingsLeft from "../components/settings/settingsLeft.tsx";
import { Outlet } from 'react-router-dom';
import "../components/settings/settings.css"

export default function Settings() {

    return (

        <section className="settings-page">
            <SettingsLeft />

            <section className="settings-right">
                <Outlet />
            </section>

        </section>
    )
}