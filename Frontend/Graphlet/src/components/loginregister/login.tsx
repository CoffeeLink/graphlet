import './login.css'
import {useNavigate} from "react-router-dom";
import { useState } from 'react';
import SuccessfulLogin from './successfulLogin';

export default function Login(){
    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState(false);

    function login(){
        const emailInput = document.querySelector(".usernameInput") as HTMLInputElement | null;
        const passwordInput = document.querySelector(".passwordInput") as HTMLInputElement | null;

        if(!emailInput || !passwordInput) return;

        //TODO if backend returns success
        console.log(emailInput.value, passwordInput.value);

        setLoading(true);
        // simulate success
        setSuccess(true);
        setLoading(false);

        setTimeout(() => {
            navigate("/workspaces");
        }, 1000);
    }

    return(
        <section className="loginsection">
            <h1>Login</h1>
            {success && <SuccessfulLogin />}
            <div>
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
                        <tr>
                            <td colSpan={2}><button onClick={login} disabled={loading}>{loading ? 'Working...' : 'Login'}</button></td>
                        </tr>
                        <tr>
                            <td colSpan={2}><a href="/register">Don't have an account? Register here!</a></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </section>
    )
}