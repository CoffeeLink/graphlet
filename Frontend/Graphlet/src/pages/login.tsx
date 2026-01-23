import '../components/loginregister/login.css'
import {useNavigate} from "react-router-dom";
import { useState} from 'react';
import SuccessfulLogin from '../components/loginregister/successfulLogin.tsx';
import {ErrorComponent} from "../components/error/errorComponent.tsx";

export default function Login(){
    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState(false);
    const [error, setError] = useState(false);


    async function login(){
        const emailInput = document.querySelector(".usernameInput") as HTMLInputElement | null;
        const passwordInput = document.querySelector(".passwordInput") as HTMLInputElement | null;
        setError(false);
        if(!emailInput?.value || !passwordInput?.value ) {
            setError(true);
            console.log(error);
            return;
        }
        const rawRes = await fetch("http://localhost:5188/api/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                email: emailInput.value,
                password: passwordInput.value
            })
        })
            const res = await rawRes.json();
            console.log(res+"nice")

        //TODO if backend returns success
        console.log(emailInput.value, passwordInput.value);

         setLoading(true);
         setSuccess(true);

         setTimeout(() => {
             navigate("/workspaces");
         }, 1000);
    }

    return(
        <section className="login-section fg">
            <h1 >Login</h1>
            {success && <SuccessfulLogin />}
            <div className={"fg"}>
                <table className="loginForm">
                    <tbody>
                        <tr>
                            <td>Email: </td>
                            <td><input type="email" className="usernameInput" required /></td>
                        </tr>
                        <tr>
                            <td>Password: </td>
                            <td><input type="password" className="passwordInput" required /></td>
                        </tr>
                        {error && <ErrorComponent error={"Hiba van, itt valami nem jó!"}/>}
                        <tr>
                            <td colSpan={2}><button onClick={login} disabled={loading} id={"loginButton"}>{loading ? 'Working...' : 'Login'}</button></td>
                        </tr>
                        <tr>
                            <td colSpan={2}><a href="/register" id={"registerLink"}>Don't have an account? Register here!</a></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </section>
    )
}