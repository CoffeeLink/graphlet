import {useState} from "react";
import {useNavigate} from "react-router-dom";
import SuccessfulRegister from "../components/loginregister/successfulRegister.tsx";
import '../components/loginregister/login.css'
import {ErrorComponent} from "../components/error/errorComponent.tsx";

export default function Register(){
    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState(false);
    const navigate = useNavigate();
    const [error, setError] = useState(false);

    function register(){
        const emailInput = document.querySelector(".emailInput") as HTMLInputElement | null;
        const passwordInput = document.querySelector(".passwordInput") as HTMLInputElement | null;
        const usernameInput = document.querySelector(".usernameInput") as HTMLInputElement | null;

        if(!emailInput?.value || !passwordInput?.value || !usernameInput?.value){
            setError(true);
            return;
        }

        // Simulate sending to backend
        setLoading(true);

        console.log(emailInput.value, passwordInput.value, usernameInput.value)

        // On successful response: show success immediately, then redirect after a delay
        setSuccess(true);
        setLoading(false);

        setTimeout(() => {
            navigate('/login');
        }, 1000);
    }

    return(
        <section className="register-section fg">
            <h1 >Register</h1>
            {success && <SuccessfulRegister />}
            <div>
                <table className="registerForm">
                    <tbody>
                        <tr>
                            <td>Email: </td>
                            <td><input type="email" className="emailInput" required /></td>
                        </tr>
                        <tr>
                            <td>Username: </td>
                            <td><input type="text" className="usernameInput" required /></td>
                        </tr>
                        <tr>
                            <td>Password: </td>
                            <td><input type="password" className="passwordInput" required /></td>
                        </tr>
                        {error && <ErrorComponent error={"Hiba van, itt valami nem jó!"}/>}
                        <tr>
                            <td colSpan={2}><button onClick={register} disabled={loading}>{loading ? 'Working...' : 'Register'}</button></td>
                        </tr>
                        <tr>
                            <td colSpan={2}><a href="/login">Already have an account? Login here!</a></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </section>
    );
}