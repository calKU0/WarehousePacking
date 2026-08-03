// Native <dialog> helpers for the shared AppModal component.
//
// Using showModal() puts the dialog in the browser's *top layer*, which sits
// above every stacking context on the page. That removes a whole class of
// z-index bugs: sticky table headers, toast overlays and composited layers can
// no longer show through or paint over a modal, no matter what the page does.
//
// Focus is not handled here: scanFocus watches the dialog's open attribute and
// puts the caret on the modal's [autofocus] field once its content renders —
// or on the dialog itself when it has no field, so no button lights up.
window.appDialog = (function () {
    // Matches the entrance/exit animation in components.css (--dur-slow).
    const ENTRANCE_MS = 280;

    return {
        /**
         * Bring the dialog's open state in sync with Blazor.
         * @param {HTMLDialogElement} el
         * @param {boolean} shouldBeOpen
         * @param {any} dotNetRef  notified when the close animation finishes, and on Esc when escDismiss
         * @param {boolean} escDismiss  whether Esc should dismiss (off for touch screens)
         */
        sync: function (el, shouldBeOpen, dotNetRef, escDismiss) {
            if (!el) return;

            if (shouldBeOpen) {
                // Reopened mid-exit: cancel the pending close and drop the class.
                if (el.__closeTimer) {
                    clearTimeout(el.__closeTimer);
                    el.__closeTimer = 0;
                    el.classList.remove('is-closing');
                }

                if (!el.open) {
                    // Esc triggers "cancel"; report it so the component can run
                    // the same close logic as the X button (resolving pending
                    // tasks).
                    if (escDismiss && dotNetRef && !el.__cancelBound) {
                        el.__cancelBound = true;
                        el.addEventListener('cancel', function (e) {
                            e.preventDefault();
                            dotNetRef.invokeMethodAsync('NotifyDismissed');
                        });
                    }

                    // Hand the caret back to the browser first. Chrome skips a
                    // dialog's autofocus when the document already has a focused
                    // element ("Autofocus processing was blocked because a
                    // document already has a focused element"), which would leave
                    // the caret on the page's scan field — a field this dialog is
                    // about to make inert, so it draws a focus ring, shows no
                    // caret and swallows every keystroke.
                    if (window.scanFocus && window.scanFocus.release) {
                        window.scanFocus.release();
                    }

                    // Before showModal, so the compositing hint is already in
                    // place when the dialog is first laid out and painted —
                    // promoting the layer on the same frame the entrance starts
                    // is what makes it hitch.
                    el.classList.add('is-opening');

                    try {
                        el.showModal();
                    } catch {
                        // Already open, or not attached yet — nothing to recover.
                    }

                    window.setTimeout(function () {
                        el.classList.remove('is-opening');
                    }, ENTRANCE_MS);
                }
            } else if (el.open && !el.__closeTimer) {
                // Fade out first, then actually close. The dialog is left in the
                // DOM (and top layer) for the animation; the caller keeps its
                // content rendered until NotifyClosed fires at the end.
                el.classList.remove('is-opening');
                el.classList.add('is-closing');

                el.__closeTimer = window.setTimeout(function () {
                    el.__closeTimer = 0;
                    el.classList.remove('is-closing');

                    try {
                        el.close();
                    } catch {
                        // Detached mid-animation — nothing left to close.
                    }

                    if (dotNetRef) {
                        try {
                            dotNetRef.invokeMethodAsync('NotifyClosed');
                        } catch {
                            // Circuit gone; the DOM close above is enough.
                        }
                    }
                }, ENTRANCE_MS);
            }
        }
    };
})();
