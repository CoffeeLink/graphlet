import "./confirmDialog.css"
import {useState} from "react";

interface ConfirmDialogProps {
    title?: string;
    message?: string;
    showInput?: boolean;
    inputValue?: string;
    onInputChange?: (v: string) => void;
    onConfirm: (value?: string) => void;
    onCancel: () => void;
    confirmLabel?: string;
    cancelLabel?: string;
    isSubmitting?: boolean;
    error?: string | null;
    confirmVariant?: "default" | "primary" | "danger";
}

export default function ConfirmDialog({
    title = "Are you sure?",
    message,
    showInput = false,
    inputValue = "",
    onInputChange,
    onConfirm,
    onCancel,
    confirmLabel = "Yes",
    cancelLabel = "No",
    isSubmitting = false,
    error = null
    , confirmVariant = "default"
}: ConfirmDialogProps){
    const [localInput, setLocalInput] = useState(inputValue);

    // keep local input in sync if parent controls it
    if (showInput && inputValue !== undefined && inputValue !== localInput) {
        // minimal sync — this may run many times but inputs are small
        setLocalInput(inputValue);
    }

    function handleConfirm() {
        if (showInput) onConfirm(localInput);
        else onConfirm();
    }

    return(
        <section className="confirm-dialog" onClick={(e) => { e.stopPropagation(); /* prevent clicks reaching parent workspace preview */ }}>
            <div className="confirm-box">
                <div className="header-row">
                    <div className="header-title">{title}</div>
                    <button className="header-close close-button" aria-label="Close" onClick={onCancel}>X</button>
                </div>
                {message && <p className="confirm-message">{message}</p>}
                {showInput && (
                    <p>
                        <input
                            type="text"
                            value={localInput}
                            onChange={e => { setLocalInput(e.target.value); if (onInputChange) onInputChange(e.target.value); }}
                            disabled={isSubmitting}
                            aria-label="Confirmation input"
                        />
                    </p>
                )}
                {error && <div className="confirm-error">{error}</div>}
                <p>
                    <button className={`confirm-button ${confirmVariant}`} onClick={handleConfirm} disabled={isSubmitting}>{isSubmitting ? (confirmLabel + "...") : confirmLabel}</button>
                    <button onClick={onCancel} disabled={isSubmitting}>{cancelLabel}</button>
                </p>
            </div>
        </section>
    )
}