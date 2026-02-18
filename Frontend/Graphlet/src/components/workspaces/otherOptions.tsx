import "./otherOptions.css"
import {Link} from "react-router-dom";

export default function OtherOptions() {
    function Logout() {
        if (localStorage.getItem("token")) {
            localStorage.removeItem("token");
        window.location.href = "/login";
        }
        else {
            console.log("nincs token");
        }
    }


    return (
        <section className="other-options fg">
            <div>
                <Link to={"/settings"}>
                    <button>Settings</button>
                </Link>
                <button onClick={Logout}>Logout</button>

            </div>
        </section>
    )
}