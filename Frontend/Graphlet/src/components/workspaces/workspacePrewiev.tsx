import "./workspacePrewiev.css"

export default function WorkspacePrewiev(props: {id: string, name: string, createdAt: string}) {


    return (
        <div className="workspacePreview">
            <div className="workspace-preview">
                <p>{props.name}</p>
            </div>
            <p>description</p>
            <p>Created at: {props.createdAt}</p>
        </div>
    )
}