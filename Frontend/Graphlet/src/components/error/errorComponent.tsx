export function ErrorComponent(props: {error: string}) {
    console.log("Error displayed:", props.error);
    return(
        <p className={"error-text"}>Hiba: {props.error}</p>
    )
}