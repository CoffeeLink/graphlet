import { useState} from "react";
import "./createNewTag.css"
import "../error/errorComponent.tsx"
import {ErrorComponent} from "../error/errorComponent.tsx";

interface CreatingNewProps {
    onClose?: () => void;
}

export function         CreateNewTag({onClose}: CreatingNewProps) {
    const [error, setError] = useState(false);
    const [color, setColor] = useState("gray");
    const [tagName, setTagName] = useState("tag");

    function handleClose(){
        if (onClose) onClose();
    }

    async function handleCreateNewTag() {
        setError(false);
        try {
            const rawRes = await fetch("http://localhost:5188/api/workspace/", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    name: tagName,
                    color: color
                })
            })
            if (rawRes.status === 201) {
                handleClose();
                //gettags in the parent component
            }
            else {
                console.error( "Failed to create tag, status code:", rawRes.status);
            }
            console.log("Creating tag:", {tagName, color});

        } catch (err) {
            console.error("Error creating new tag:", err);
        }
        //TODO: show error message
    }


    return (
        <div className={"create-new-tag"}>
            <div className="header-row">
                <h3 className={"header-title"}>Create new tag</h3>
                <button className="header-close close-button" onClick={handleClose}>X</button>
            </div>
            <table>
                <tbody>
                <tr>
                    <td>Tag name:</td>
                    <td><input type="text" id={"tagNameInput"} value={tagName} onChange={e => setTagName(e.target.value)}/></td>
                </tr>
                <tr>
                    <td>Tag color:</td>
                    <td><input type="color"  value={color} onChange={e => setColor(e.target.value)}/></td>
                </tr>
                <tr>
                    <td colSpan={2}>{error && <ErrorComponent error={"An error occurred during creating tag."}/>}</td>
                </tr>
                <tr>
                    <td colSpan={2}>
                        <button onClick={()=>{handleCreateNewTag(); handleClose()}}>Create Tag</button>
                    </td>
                </tr>
                </tbody>
            </table>
        </div>
    );
}
