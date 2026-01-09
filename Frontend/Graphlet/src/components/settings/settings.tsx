import SettingsLeft from "./settingsLeft";
import { Outlet } from 'react-router-dom';
import "./settings.css"

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