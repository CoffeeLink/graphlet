import "../components/workspaces/workspaces.css"
import OtherOptions from "../components/workspaces/otherOptions.tsx";
import {useState} from "react";
import WorkspacePreview from "../components/workspaces/workspacePrewiev.tsx";
import CreatingNewWokspace from "../components/workspaces/creatingNewWokspace.tsx";
import {getWorkspaces} from "../components/workspaces/getWorkspaces.tsx";
import {Workspace} from "../components/classes/workspace.tsx"

export default function Workspaces() {
    const [workspaces, setWorkspaces] = useState<Workspace>(new Workspace(null, "", new Date()));
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
                        {Object.keys(workspaces).length === 0 && <p>No workspaces found. Create a new workspace to get started!</p>}
                        {Object.entries(workspaces).map(([id, workspace]) => (
                            <WorkspacePreview key={id} id={id} name={workspace.name} createdAt={workspace.createdAt} />
                        ))}
                    </div>
                    {showCreatingNew && <CreatingNewWokspace onClose={handleCloseCreatingNew}  />}
                </main>
            </div>
        </>
    );
}