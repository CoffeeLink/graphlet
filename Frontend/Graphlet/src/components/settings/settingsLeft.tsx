import {NavLink} from "react-router-dom";

export default function SettingsLeft(){ //left side of settings page
    return(
        <section className="settings-left">
            {/*TODO backbutton here*/}
            <h1>Settings</h1>
            <nav className="settings-nav">
                <NavLink to={"appearance"} className={({isActive}) => isActive ? 'nav-button active' : 'nav-button'}><button>Appearance</button></NavLink>
                <NavLink to={"profile"} className={({isActive}) => isActive ? 'nav-button active' : 'nav-button'}><button>Profile</button></NavLink>
                <NavLink to={"shared"} className={({isActive}) => isActive ? 'nav-button active' : 'nav-button'}><button>Shared items</button></NavLink>
                <NavLink to={"subscription"} className={({isActive}) => isActive ? 'nav-button active' : 'nav-button'}><button>Subscription</button></NavLink>
            </nav>

        </section>
    )
}
