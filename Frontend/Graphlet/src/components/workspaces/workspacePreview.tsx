import "./workspacePreview.css"

export default function WorkspacePreview(props: {id: string, name: string}) {


    return (
        <div className="workspacePreview">
            <div className="workspace-preview">
                <p style={{color:"black"}}>{props.name}</p>
            </div>
            <p>description</p>
        </div>
    )
}