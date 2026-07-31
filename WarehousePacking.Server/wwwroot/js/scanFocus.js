// Keeps a barcode-scan field focused without the page having to re-focus it
// after every interaction.
//
// A hardware scanner types into whatever has focus, so on a packing station the
// scan field must effectively always own the caret. Previously every action had
// to remember to call FocusAsync() again, which was easy to miss and left the
// scanner typing into nowhere.
//
// This guardian watches for focus leaving the field and restores it, except
// when the operator is legitimately somewhere else:
//   - a modal <dialog> is open (its own inputs must keep focus),
//   - the user is typing in another input/textarea/select/contenteditable,
//   - the field is disabled or detached.
window.scanFocus = (function () {
    const registry = new Map();
    let nextId = 1;
    let listenersBound = false;

    function anyDialogOpen() {
        // Only real modals block refocusing. Toasts are deliberately excluded:
        // they are transient notifications that must not leave the scanner
        // without a focus target (and they restore focus themselves on close).
        return document.querySelector('dialog[open], .modal.show') !== null;
    }

    function isUserTypingElsewhere(active) {
        if (!active || active === document.body) return false;
        return active.matches('input, textarea, select, [contenteditable=""], [contenteditable="true"]');
    }

    function shouldRestore(entry) {
        const el = entry.element;
        if (!el || !el.isConnected || el.disabled || el.readOnly) return false;
        if (!entry.enabled) return false;
        if (anyDialogOpen()) return false;

        const active = document.activeElement;
        if (active === el) return false;
        if (isUserTypingElsewhere(active)) return false;

        return true;
    }

    function restore() {
        // Newest registration wins: the active page owns the caret.
        const entries = [...registry.values()];
        for (let i = entries.length - 1; i >= 0; i--) {
            const entry = entries[i];
            if (shouldRestore(entry)) {
                try {
                    entry.element.focus({ preventScroll: true });
                } catch {
                    /* element went away mid-frame */
                }
                return;
            }
        }
    }

    // Defer so we run after the browser has settled focus and Blazor has
    // finished its DOM updates. Debounced because a Blazor page re-renders
    // frequently and a tap fires several of these triggers at once — without
    // coalescing, the observer below would schedule work on every diff.
    let pending = null;

    function scheduleRestore() {
        if (pending !== null) return;
        pending = setTimeout(function () {
            pending = null;
            restore();
        }, 30);
    }

    function bindListeners() {
        if (listenersBound) return;
        listenersBound = true;

        // Focus left something — maybe our field, maybe a button that stole it.
        document.addEventListener('focusout', scheduleRestore, true);

        // Tapping a button gives it focus; take it back once the click is done.
        document.addEventListener('pointerup', scheduleRestore, true);
        document.addEventListener('click', scheduleRestore, true);

        // A modal closing removes it from the DOM: reclaim focus afterwards.
        new MutationObserver(scheduleRestore)
            .observe(document.body, { childList: true, subtree: true });

        // Returning to the tab/kiosk window.
        window.addEventListener('focus', scheduleRestore);
        document.addEventListener('visibilitychange', scheduleRestore);
    }

    return {
        /**
         * Start guarding an element. Returns an id to pass to unregister().
         * @param {HTMLElement} element
         */
        register: function (element) {
            if (!element) return null;
            bindListeners();

            const id = 'scan-' + nextId++;
            registry.set(id, { element, enabled: true });
            scheduleRestore();
            return id;
        },

        unregister: function (id) {
            if (id) registry.delete(id);
        },

        /** Temporarily stop/resume guarding (e.g. while a page is busy). */
        setEnabled: function (id, enabled) {
            const entry = registry.get(id);
            if (entry) {
                entry.enabled = !!enabled;
                if (enabled) scheduleRestore();
            }
        },

        /** Force an immediate attempt, e.g. right after a flow completes. */
        refocus: scheduleRestore
    };
})();
