export function Tag(props: { text: string; color?: string }) {
    const Color = props.color ? props.color : "gray";

    return(
        <div className={"tag"}>
            <span style={{
                backgroundColor: Color,
            }}>{props.text}</span>
        </div>
    )
}