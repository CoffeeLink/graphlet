import "../components/workspaces/workspaces.css"
import OtherOptions from "../components/workspaces/otherOptions.tsx";
import {useState} from "react";
import WorkspacePreview from "../components/workspaces/workspacePreview.tsx";
import CreatingNewWokspace from "../components/workspaces/creatingNewWokspace.tsx";
//import {getWorkspaces} from "../components/workspaces/getWorkspaces.tsx";
import {Workspace} from "../components/classes/workspace.tsx"

async function getWorkspaces(){
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

export default function Workspaces() {
    const [workspaces, setWorkspaces] = useState<Workspace>(new Workspace(null, ""));
    setTimeout(
        async () => {
            const w = await getWorkspaces();
            if (w !== undefined && w !== null) setWorkspaces(w);
        },
        100
    )

    const [showOtherOptions, setShowOtherOptions] = useState(false);

    function handleOtherOptionsClick() { //opening options when button clicked
        setShowOtherOptions(prev => !prev);
    }

    const [showCreatingNew, setShowCreatingNew] = useState(false);
    function handleCreateNewClick() {
        setShowCreatingNew(prev => !prev);
    }

    function handleCloseCreatingNew() {
        setShowCreatingNew(false);
    }



    return (
        <>
            <div className="workspaces-page popup-bg">
                <header className={"workspaces-header"}>
                    <h2>My workspaces</h2>
                    <button type="button" onClick={handleCreateNewClick}>Create new</button>
                    <p>Keresés <input type="text"/></p>
                    <button onClick={handleOtherOptionsClick}>...</button>
                    {showOtherOptions && <OtherOptions/>}
                </header>
                <main>
                    <div className="workspaces-container">
                        {workspaces && workspaces.id && <WorkspacePreview id={workspaces.id} name={workspaces.name}/>}

                    </div>
                    {showCreatingNew && <CreatingNewWokspace onClose={handleCloseCreatingNew}  />}
                </main>
            </div>
        </>
    );
}