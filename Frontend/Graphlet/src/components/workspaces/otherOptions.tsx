import "./otherOptions.css"
import {Link} from "react-router-dom";
import {useRef, useEffect} from "react";

interface OtherOptionsProps {
    onClose?: () => void;
}

export default function OtherOptions({ onClose }: OtherOptionsProps) {
    const ref = useRef<HTMLDivElement>(null);

    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (ref.current && !ref.current.contains(event.target as Node)) {
                if (onClose) onClose();
            }
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, [onClose]);

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
        <section className="other-options fg" ref={ref}>
            <div>
                <Link to={"/settings"}>
                    <button id="settings-button">Settings</button>
                </Link>
                <button id="logout-button" onClick={Logout}>Logout</button>

            </div>
        </section>
    )
}