export default function Register(){
    function register(){
        const emailInput = document.querySelector(".usernameInput") as HTMLInputElement
        const passwordInput = document.querySelector(".passwordInput") as HTMLInputElement
        const usernameInput = document.querySelector(".usernameInput") as HTMLInputElement

        console.log(emailInput.value, passwordInput.value, usernameInput.value)
    }
 
    return(
        <section className="registersection">
            <h1>Register</h1>
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
                        <tr>
                            <td colSpan={2}><button onClick={register}>Login</button></td>
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