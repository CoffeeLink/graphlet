import {NavLink, useNavigate} from "react-router-dom";

export default function SettingsLeft(){ //left side of settings page
    const navigate = useNavigate()
    function goBack(){
        navigate("/workspaces")
    }

    return(
        <section className="settings-left">
            <button className={"backbutton"} id={"back-to-dashboard"} onClick={goBack}>Go back</button>
            <h1>Settings</h1>
            <nav className="settings-nav">
                <NavLink to={"appearance"} className={({isActive}) => isActive ? 'nav-button active' : 'nav-button'}><button id="settings-nav-appearance">Appearance</button></NavLink>
                <NavLink to={"profile"} className={({isActive}) => isActive ? 'nav-button active' : 'nav-button'}><button id="settings-nav-profile">Profile</button></NavLink>
                <NavLink to={"shared"} className={({isActive}) => isActive ? 'nav-button active' : 'nav-button'}><button id="settings-nav-shared">Shared items</button></NavLink>
                <NavLink to={"subscription"} className={({isActive}) => isActive ? 'nav-button active' : 'nav-button'}><button id="settings-nav-subscription">Subscription</button></NavLink>
            </nav>

        </section>
    )
}
