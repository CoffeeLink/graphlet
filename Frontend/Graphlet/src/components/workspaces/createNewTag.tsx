import {useEffect, useState} from "react";
import "./createNewTag.css"
interface CreatingNewProps {
    onClose?: () => void;
}

export function CreateNewTag({onClose}: CreatingNewProps) {

    const [color, setColor] = useState("gray");
    const [tagName, setTagName] = useState("tag");


    function handleCreateNewTag() {
        try {
            // Here you can send the tagName and color to the server
            //TODO: send new tag to server
            //wait for 201
            console.log("Creating tag:", {tagName, color});
        } catch (err) {
            console.error("Error creating new tag:", err);
        }
        //TODO: show error message 
    }

    useEffect(() => {
        // async function createTag() {
        //     const res = await fetch("http://localhost:5188/")
        //
        //     // if (res.status === 201) {
        //     //     handleClose();
        //     // } else {
        //
        // }
    }, []);

    function handleClose(){
        if (onClose) onClose();
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
                    <td><input type="text" value={tagName} onChange={e => setTagName(e.target.value)}/></td>
                </tr>
                <tr>
                    <td>Tag color:</td>
                    <td><input type="color" value={color} onChange={e => setColor(e.target.value)}/></td>
                </tr>
                <tr>
                    <td colSpan={2}>
                        <button onClick={handleCreateNewTag}>Create Tag</button>
                    </td>
                </tr>
                </tbody>
            </table>
        </div>
    );
}
