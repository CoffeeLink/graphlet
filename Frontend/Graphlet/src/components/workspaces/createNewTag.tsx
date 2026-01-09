import {useState} from "react";


export function CreateNewTag() {

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

    return (
        <div className={"create-new-tag"}>
            <h3>Create new tag</h3>
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
