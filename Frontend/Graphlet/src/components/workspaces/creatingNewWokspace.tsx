import "./createNewWorkspace.css"
import {useState} from "react";
import {Tag} from "./tag.tsx";
import {CreateNewTag} from "./createNewTag.tsx";

interface CreatingNewProps {
    onClose?: () => void;
}

export default function CreatingNewWokspace({ onClose }: CreatingNewProps) {

    // function getTags(){
    //     //TODO: fetch tags from server
    //
    // }

    const tag = <Tag text="Aasd" color="blue" />;
    console.log(tag);

    const [_name, setName] = useState('')
    function handleCreateWorkspace(){
        console.log(_name);
        //todo send workspace creation request to server
        //if returns success, redirect to workspaces page
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
                        <td><input type="text" id={"nameInput"} value={_name} onChange={(e) => setName(e.target.value)} /></td>
                    </tr>
                    <tr>
                        <td>Tags</td>
                        <td className={"tag-container"}>list of tags {tag}</td>
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