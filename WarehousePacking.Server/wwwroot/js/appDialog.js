// Native <dialog> helpers for the shared AppModal component.
//
// Using showModal() puts the dialog in the browser's *top layer*, which sits
// above every stacking context on the page. That removes a whole class of
// z-index bugs: sticky table headers, toast overlays and composited layers can
// no longer show through or paint over a modal, no matter what the page does.
// Focus the element a modal marked with `autofocus`, retrying across a few
// frames while its content settles. Selects any existing text so the operator
// can overtype a pre-filled value.
function focusMarked(el, attemptsLeft) {
    const target = el.querySelector('[autofocus]');
    if (!target) return;

    if (target.isConnected && !target.disabled && !target.readOnly) {
        try {
            target.focus({ preventScroll: true });
            if (typeof target.select === 'function') target.select();
        } catch {
            /* element went away mid-frame */
        }
        return;
    }

    if (attemptsLeft > 0) {
        requestAnimationFrame(() => focusMarked(el, attemptsLeft - 1));
    }
}

window.appDialog = {
    /**
     * Bring the dialog's open state in sync with Blazor.
     * @param {HTMLDialogElement} el
     * @param {boolean} shouldBeOpen
     * @param {any} dotNetRef  optional; notified when the user dismisses via Esc
     */
    sync: function (el, shouldBeOpen, dotNetRef) {
        if (!el) return;

        if (shouldBeOpen) {
            if (!el.open) {
                // Esc triggers "cancel"; report it so the component can run the
                // same close logic as the X button (resolving pending tasks).
                if (dotNetRef && !el.__cancelBound) {
                    el.__cancelBound = true;
                    el.addEventListener('cancel', function (e) {
                        e.preventDefault();
                        dotNetRef.invokeMethodAsync('NotifyDismissed');
                    });
                }

                try {
                    el.showModal();
                } catch {
                    // Already open, or not attached yet — nothing to recover.
                }

                // showModal()'s own focusing step lands on the first focusable
                // element, which is the header's close button — not the input a
                // form modal wants active. Move focus to the element the modal
                // marked with `autofocus`. Retried a few frames because Blazor
                // may still be settling the dialog's content.
                focusMarked(el, 5);
            }
        } else if (el.open) {
            el.close();
        }
    }
};
