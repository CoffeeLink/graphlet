import "./otherOptions.css"
import {Link} from "react-router-dom";
export default function OtherOptions(){
    function Logout(){
        localStorage.removeItem("token");
        window.location.href = "/login";
    }


    return(
        <section className="other-options fg">
            <div>
                <Link to={"/settings"}><button>Settings</button></Link>
                <button>Not defined</button>
                <button>Not defined</button>
                <button onClick={Logout}>Logout</button>

            </div>
        </section>
    )
}