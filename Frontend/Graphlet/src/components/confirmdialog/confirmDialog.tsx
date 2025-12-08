import "./confirmDialog.css"

export default function ConfirmDialog(){

    return(
        <section className="confirm-dialog">
            <h2>Are you sure?</h2>
            <p><button>Yes</button><button>No</button></p>
        </section>
    )
}