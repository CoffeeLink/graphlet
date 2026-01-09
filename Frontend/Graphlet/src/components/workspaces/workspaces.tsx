import "./workspaces.css"
import OtherOptions from "./otherOptions.tsx";
import {useState} from "react";
import WorkspacePreview from "./workspacePrewiev.tsx";
import CreatingNewWokspace from "./creatingNewWokspace.tsx";


export default function Workspaces() {
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

    // function getWorkspaces(){
    //     //TODO fetch workspaces from backend
    //
    // }

    return (
        <>
            <div className="workspaces-page popup-bg">
                <header>
                    <h2>My workspaces</h2>
                    <button type="button" onClick={handleCreateNewClick}>Create new</button>
                    <p>Keresés <input type="text"/></p>
                    <button onClick={handleOtherOptionsClick}>...</button>
                    {showOtherOptions && <OtherOptions/>}
                </header>
                <main>
                    <WorkspacePreview/>
                    <WorkspacePreview/>
                    <WorkspacePreview/>
                    {showCreatingNew && <CreatingNewWokspace onClose={handleCloseCreatingNew} />}
                </main>
            </div>
        </>
    );
}