import './login.css'
export default function Login(){


    function login(){
        const emailInput = document.querySelector(".usernameInput") as HTMLInputElement
        const passwordInput = document.querySelector(".passwordInput") as HTMLInputElement

        console.log(emailInput.value, passwordInput.value)
    }

    return(
        <section className="loginsection">
            <h1>Login</h1>
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
                            <td colSpan={2}><button onClick={login}>Login</button></td>
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