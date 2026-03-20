import {useState} from "react";
import {useNavigate} from "react-router-dom";
import SuccessfulRegister from "../components/loginregister/successfulRegister.tsx";
import '../components/loginregister/login.css'
import {ErrorComponent} from "../components/error/errorComponent.tsx";
import {Input} from "@heroui/input";
import {Button} from "@heroui/button";

export default function Register() {
    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState(false);
    const navigate = useNavigate();
    const [error, setError] = useState(false);
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    const [errormsg, setErrormsg] = useState("");
    const [email, setEmail] = useState("");
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    async function registerUser() {
        setError(false);
        if (!email || !password || !username) {
            setError(true);
            setErrormsg("All fields are required.");
            return;
        }
        if (!emailRegex.test(email)) {
            setError(true);
            setErrormsg("Invalid email format.");
            return;
        }
        try {
            const rawRes = await fetch("http://localhost:5188/api/user/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ name: username, email, password })
            });
            const res = await rawRes.json();

            if (!rawRes.ok) {
                setError(true);
                setErrormsg(res ?? "Registration failed. Please try again.");
                return;
            }

            setLoading(true);
            setSuccess(true);

            setTimeout(() => {
                navigate('/login');
            }, 1000);
        } catch {
            setError(true);
            setErrormsg("Registration failed. Please try again.");
        }
    }

    return (
        <div className={"container"}>
            <section className="login-section fg">
                <h1>Register</h1>
                {success && <SuccessfulRegister/>}
                <div className={"loginForm"}>
                    <div className="flex w-full flex-wrap md:flex-nowrap gap-4">
                        <Input label="Email" placeholder="Enter your email" type="email" required
                               value={email} onValueChange={setEmail} className={"emailInput"}/>
                    </div>
                    <div className="flex w-full flex-wrap md:flex-nowrap gap-4">
                        <Input label="Username" placeholder="Enter your username" type="text" required
                               value={username} onValueChange={setUsername} className={"usernameInput"}/>
                    </div>
                    <div className="flex w-full flex-wrap md:flex-nowrap gap-4">
                        <Input label="Password" placeholder="Enter your password" type="password" required
                               value={password} onValueChange={setPassword} className={"passwordInput"}
                               onKeyDown={e => { if (e.key === "Enter") registerUser(); }}/>
                    </div>
                    {error && <ErrorComponent error={errormsg}/>}
                    <Button onPress={registerUser} isDisabled={loading} id={"registerButton"}>{loading ? 'Working...' : 'Register'}</Button>
                    <a href="/login" id={"loginLink"}>Already have an account? Login here!</a>
                </div>
            </section>
        </div>
    );
}