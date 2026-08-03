// Puts the notification stack in the browser's top layer.
//
// A popover is the only way to float above an open modal <dialog> without
// blocking anything: same top layer, no backdrop, no inertness. The previous
// notification was itself a modal dialog, so every message stopped the line
// until someone dismissed it — and made the scan field inert while it was up.
//
// Where popovers are unsupported this is a no-op and the stack falls back to a
// plain fixed-position corner, which only loses the ability to sit above modals.
window.toastStack = {
    /**
     * @param {HTMLElement} el   the stack container
     * @param {boolean} visible  whether anything is queued
     */
    sync: function (el, visible) {
        if (!el || typeof el.showPopover !== 'function') return;

        try {
            const open = el.matches(':popover-open');

            if (visible && !open) el.showPopover();
            else if (!visible && open) el.hidePopover();
        } catch {
            // Raced with the element being removed — nothing to recover.
        }
    }
};
