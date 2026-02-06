export async function getWorkspaces(){
    await fetch("http://localhost:5188/api/workspace",{
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${localStorage.getItem("token")}`
        }
    }).then((response: Response) => {
        if (response.status === 200) {
            return response.json().then((data) => {
                console.log("Workspaces data:", data);
                return data;
            });
        }
        else {
            console.log(response);
        }
    });
}