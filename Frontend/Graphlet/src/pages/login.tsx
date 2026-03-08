import '../components/loginregister/login.css'
import {useNavigate} from "react-router-dom";
import {useState} from 'react';
import SuccessfulLogin from '../components/loginregister/successfulLogin.tsx';
import {ErrorComponent} from "../components/error/errorComponent.tsx";
//ui
import {Input} from "@heroui/input";
import {Button} from "@heroui/button";

export default function Login(){
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState(false);
    const [error, setError] = useState(false);
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");


    async function login(){
        setError(false);
        if(!email || !password) {
            setError(true);
            return;
        }
        if(!emailRegex.test(email)){
            setError(true);
            console.log("Invalid email format");
            return;
        }
        const rawRes = await fetch("http://localhost:5188/api/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                email:email,
                password:password
            })
        })
        const res = await rawRes.json();

        if(rawRes.status === 200){
            localStorage.removeItem("token");
            localStorage.setItem("token", res.token);
        } else {
            setError(true);
            return;
        }

        setLoading(true);
        setSuccess(true);

        setTimeout(() => {
            navigate("/workspaces");
        }, 1000);
    }

    return(
        <div className={"container"}>
        <section className="login-section fg">
            <h1>Login</h1>
            {success && <SuccessfulLogin />}
            <div className={"loginForm"}>
                <div className="flex w-full flex-wrap md:flex-nowrap gap-4">
                    <Input label="Email" placeholder="Enter your email" type="email" required
                        value={email} onValueChange={setEmail}/>
                </div>
                <div className="flex w-full flex-wrap md:flex-nowrap gap-4">
                    <Input label="Password" placeholder="Enter your password" type="password" required
                        value={password} onValueChange={setPassword}
                        onKeyDown={e => { if (e.key === "Enter") login(); }}/>
                </div>
                {error && <ErrorComponent error={"Wrong email or password!"}/>}
                <Button onPress={login} isDisabled={loading} id={"loginButton"}>{loading ? 'Working...' : 'Login'}</Button>
                <a href="/register" id={"registerLink"}>Don't have an account? Register here!</a>
            </div>
        </section>
        </div>
    )
}