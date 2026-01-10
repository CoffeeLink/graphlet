import "./otherOptions.css"
import {Link} from "react-router-dom";
export default function OtherOptions(){
    
    return(
        <section className="other-options fg">
            <div>
                <Link to={"/settings"}><button>Settings</button></Link>
                <button>Not defined</button>
                <button>Not defined</button>
                <button>Logout</button>

            </div>
        </section>
    )
}