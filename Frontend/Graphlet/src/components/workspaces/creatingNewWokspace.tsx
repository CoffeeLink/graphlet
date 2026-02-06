import "./createNewWorkspace.css"
import {useState} from "react";
import {Tag} from "./tag.tsx";
import {CreateNewTag} from "./createNewTag.tsx";

interface CreatingNewProps {
    onClose?: () => void;
}

export default function CreatingNewWokspace({ onClose }: CreatingNewProps) {

    const tag = <Tag text="Aasd" color="blue" />;
    console.log(tag);

    const [workspaceName, setWorkspaceName] = useState('')
    async function handleCreateWorkspace(){
        console.log(workspaceName);

        await  fetch("http://localhost:5188/api/workspaces", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                name: workspaceName
            })
        })
    }

    function handleClose(){
        if (onClose) onClose();
    }

    const [showCreateTag, setShowCreateTag] = useState(false);
    function handleCreateNewTag() {
        setShowCreateTag(perv => !perv);
    }
    function onCloseCreate() {
        setShowCreateTag(false);
        getTags();
    }

    async function getTags(){
       //kell endpoint
       //  const rawres = await fetch("http://localhost:5188/api/userTags", {
       //      method: "GET",
       //      headers: {
       //
       //      }
       //  })

    }

    return (
        <>
            <div className={"creating-new-workspace fg"}>
                <div className="header-row">
                    <div className="header-title text">Create new workspace</div>
                    <button className="header-close close-button" aria-label="Close" onClick={handleClose}>X</button>
                </div>
                <table>
                    <tbody>
                    <tr >
                        <td >Workspace name:</td>
                        <td><input type="text" id={"nameInput"} value={workspaceName} onChange={(e) => setWorkspaceName(e.target.value)} /></td>
                    </tr>
                    <tr>
                        <td>Tags</td>
                        <td className={"tag-container"}>  </td>
                        <td><button onClick={handleCreateNewTag}>Create new tag</button></td>
                    </tr>
                    </tbody>
                </table>
                <button onClick={handleCreateWorkspace} id={"create-new-workspace-button"}>Create new Workspace</button>
                {showCreateTag && <CreateNewTag onClose={onCloseCreate}/>}
            </div>
        </>
    );
}