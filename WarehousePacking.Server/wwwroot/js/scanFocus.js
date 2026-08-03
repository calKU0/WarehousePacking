// Owns "which field has the caret" for the whole app.
//
// On a packing station a hardware scanner types into whatever holds the caret,
// so a field must always own it — the page's scan field normally, the modal's
// own field while a modal is open. Chasing focus after every click and render
// fights the browser (it breaks double-clicks) and needs a re-focus call in
// every flow, which is what this replaces.
//
// The rule is a single one: the caret belongs to the *innermost open dialog's*
// field, or to the page's scan field when no dialog is open. Everything else
// follows from it:
//
//   1. A tap on anything that is not a form field never takes focus in the
//      first place — cancelling mousedown's default action suppresses the
//      focus change and text selection only, so click/dblclick still fire.
//      This is what keeps buttons from stealing the caret inside modals too.
//   2. Focus is re-asserted whenever the picture changes: a dialog opened or
//      closed, the field was removed or re-enabled, the window came back. Each
//      attempt is *verified* with :focus and retried over the next frames —
//      focusing during a dialog's own close handling half-applies, leaving a
//      field that draws its focus ring but has no caret and eats every key.
//   3. A keystroke arriving while nothing really holds the caret re-arms the
//      field, so a scan is never typed into the void whatever went wrong.
//
// A modal with no field of its own parks the caret on the <dialog> itself, so
// its buttons stay unhighlighted instead of the browser focusing the first one.
window.scanFocus = (function () {
    const FIELD = 'input:not([type="hidden"]), textarea, select, [contenteditable=""], [contenteditable="true"]';

    // What counts as "a modal is up". :modal is what the browser itself goes by
    // when it makes the rest of the page inert, so it also catches a dialog
    // that is still in the top layer while its open attribute is already gone.
    const MODAL_SUPPORTED = supports('dialog:modal');
    const SCOPE = MODAL_SUPPORTED ? 'dialog[open], dialog:modal' : 'dialog[open]';

    function supports(selector) {
        try {
            document.querySelector(selector);
            return true;
        } catch {
            return false;
        }
    }

    // Per dialog: the field the operator last used, so restoring lands back
    // where they were rather than on the modal's first field.
    const lastField = new WeakMap();

    // Long enough to outlast a modal's open/close animation, after which the
    // browser has settled focus and ours sticks.
    const RETRIES = 20;
    const FRAME_MS = 16;
    // Comfortably past the browser's double-tap window, so a restore can never
    // land between the two taps of a double click.
    const TOUCH_SETTLE_MS = 450;

    let pageField = null;
    let lastPointerType = '';
    let handle = 0;
    let pending = null;
    let retriesLeft = 0;

    function isUsable(el) {
        return !!el && el.isConnected && !el.disabled && !el.readOnly;
    }

    // Reverse document order: with stacked modals the innermost is tried first.
    // A wrong guess is harmless — the browser refuses focus outside the topmost
    // modal, so we simply move on to the next candidate.
    function openScopes() {
        return Array.prototype.slice.call(document.querySelectorAll(SCOPE)).reverse();
    }

    function fieldIn(scope) {
        const remembered = lastField.get(scope);
        if (isUsable(remembered) && scope.contains(remembered)) return remembered;

        const marked = scope.querySelector('[autofocus]');
        return isUsable(marked) ? marked : null;
    }

    // document.activeElement alone is not proof of a working caret: a field
    // left over from a closing dialog, or one made inert by a modal still in
    // the top layer, stays activeElement (and keeps drawing its focus ring)
    // while having no caret and receiving no keystrokes. :focus tells the two
    // apart — but only while this document actually holds keyboard focus; in a
    // background window nothing matches :focus and it would report nonsense.
    function hasRealFocus(el) {
        if (document.activeElement !== el) return false;
        if (!document.hasFocus() || !el.matches) return true;
        return el.matches(':focus');
    }

    function focusEl(el) {
        if (hasRealFocus(el)) return true;

        try {
            // Landing mid-way through the browser's own focus handling leaves
            // that half-applied state; blurring first makes focus() take.
            if (document.activeElement === el) el.blur();
            el.focus({ preventScroll: true });
        } catch {
            return false; // element went away mid-frame
        }

        return hasRealFocus(el);
    }

    function hasOwner() {
        return openScopes().length > 0 || isUsable(pageField);
    }

    // A dialog can be left in the browser's top layer after the app considers
    // it closed — anything that drops the open attribute without going through
    // close() does it, and Blazor re-rendering a <dialog> is one such path.
    // That alone keeps the whole page inert: the scan field still draws its
    // focus ring but has no caret and swallows every keystroke, which is
    // exactly the "looks focused, types nothing" state.
    //
    // close() alone cannot fix it — it returns early when there is no open
    // attribute — so the attribute goes back on first, purely so that close()
    // will run and release the top layer.
    function dropLingeringModals() {
        if (!MODAL_SUPPORTED) return;

        const stuck = document.querySelectorAll('dialog:modal');
        for (let i = 0; i < stuck.length; i++) {
            if (stuck[i].hasAttribute('open')) continue;

            try {
                stuck[i].setAttribute('open', '');
                stuck[i].close();
            } catch {
                /* nothing more we can do from here */
            }
        }
    }

    /**
     * Puts the caret where it belongs.
     * @returns {boolean} true once a field really owns it (or there is nothing
     *          to do); false asks the caller to try again next frame.
     */
    function restore() {
        dropLingeringModals();

        const scopes = openScopes();

        if (scopes.length === 0) {
            return isUsable(pageField) ? focusEl(pageField) : true;
        }

        for (let i = 0; i < scopes.length; i++) {
            const field = fieldIn(scopes[i]);
            if (!field || !focusEl(field)) continue;

            // Focus only — the caret sits in the field without selecting its
            // contents, so a pre-filled value (a quantity of "1", say) is not
            // painted blue on open.
            return true;
        }

        // No field to hold the caret: park it on the modal itself so none of
        // its buttons ends up focused. Reported as unsettled so a modal whose
        // content is still rendering gets picked up by the next retry.
        for (let i = 0; i < scopes.length; i++) {
            if (!scopes[i].hasAttribute('tabindex')) scopes[i].setAttribute('tabindex', '-1');
            if (focusEl(scopes[i])) break;
        }
        return false;
    }

    // Retry for a short while: a dialog opens before Blazor has rendered its
    // content, so the field to focus may not exist yet. Timers rather than
    // animation frames — those stop firing while the tab is hidden, which would
    // leave a retry pending forever and the guard permanently deaf.
    function attempt() {
        pending = null;

        if (restore() || retriesLeft <= 0) {
            retriesLeft = 0;
            return;
        }

        retriesLeft--;
        pending = setTimeout(attempt, FRAME_MS);
    }

    function scheduleRestore(retries, delayMs) {
        retriesLeft = Math.max(retriesLeft, retries || 0);
        if (pending !== null) return;

        // Let the current task finish first: a dialog's close, an element
        // removal and a Blazor re-render all settle focus after their own event.
        pending = setTimeout(attempt, delayMs || 0);
    }

    // Nobody is really typing anywhere: either nothing is focused, or a field
    // only looks focused (see hasRealFocus) and would swallow every keystroke.
    function noRealFocus() {
        const active = document.activeElement;
        if (!active || active === document.body) return true;
        return !hasRealFocus(active);
    }

    // A tap on a touch screen is not a click: the browser replays it as mouse
    // events only after the gesture is over, and a second tap within the
    // double-tap window is what produces dblclick. Touching focus anywhere in
    // that window moves the on-screen keyboard and reflows the page under the
    // operator's finger, so the second tap lands somewhere else and the double
    // click never happens. On touch we therefore keep our hands off the gesture
    // entirely and only put the caret back once it is over.
    function isTouchDriven(e) {
        if (e.sourceCapabilities && typeof e.sourceCapabilities.firesTouchEvents === 'boolean') {
            return e.sourceCapabilities.firesTouchEvents;
        }
        return lastPointerType !== 'mouse' && lastPointerType !== '';
    }

    function onPointerDown(e) {
        lastPointerType = e.pointerType || '';
    }

    function onPointerUp(e) {
        // Backstop for touch: if anything swallows the compatibility mouse
        // events, the caret still comes back once the gesture is over.
        if (e.pointerType === 'mouse' || !hasOwner()) return;
        if (e.target.closest && e.target.closest(FIELD)) return;

        scheduleRestore(RETRIES, TOUCH_SETTLE_MS);
    }

    function onMouseDown(e) {
        if (!hasOwner()) return;

        const target = e.target;
        if (target.closest && target.closest(FIELD)) return;

        if (isTouchDriven(e)) {
            scheduleRestore(RETRIES, TOUCH_SETTLE_MS);
            return;
        }

        // Mouse: cancelling the default action keeps the caret where it is and
        // costs nothing — the click/dblclick that follows is dispatched exactly
        // as before, because only focus and text selection are suppressed.
        e.preventDefault();
        restore();
    }

    function onFocusIn(e) {
        const el = e.target;
        if (!el || !el.matches || !el.matches(FIELD)) return;

        const scope = el.closest(SCOPE);
        if (scope) lastField.set(scope, el);
    }

    function onFocusOut() {
        // Only step in when focus fell off the page rather than moving to
        // something the operator picked.
        setTimeout(function () {
            if (noRealFocus()) scheduleRestore(RETRIES);
        }, 0);
    }

    function onKeyDown(e) {
        // The safety net: whatever went wrong, a keystroke must never be lost.
        // Repairing focus here — before the browser applies the keystroke —
        // means the character still lands in the field.
        if (noRealFocus()) {
            restore();
            return;
        }

        const active = document.activeElement;

        // A scanned character heading anywhere but a field (a button that kept
        // focus after a tap, say) would be dropped. Only printable keys are
        // rerouted, so Tab/Enter/Esc on a button still do what they should.
        if (e.key && e.key.length === 1 && active && active.matches && !active.matches(FIELD)) {
            restore();
            return;
        }

        // Keys are arriving outside the modal that is blocking the page, so
        // whatever holds the caret cannot really receive them.
        const scopes = openScopes();
        if (scopes.length === 0 || !e.target) return;

        for (let i = 0; i < scopes.length; i++) {
            if (scopes[i].contains(e.target)) return;
        }
        restore();
    }

    document.addEventListener('pointerdown', onPointerDown, true);
    document.addEventListener('pointerup', onPointerUp, true);
    document.addEventListener('mousedown', onMouseDown, true);
    document.addEventListener('focusin', onFocusIn, true);
    document.addEventListener('focusout', onFocusOut, true);
    document.addEventListener('keydown', onKeyDown, true);

    // showModal()/close() flip the open attribute, and watching it is the only
    // reliable way to see a modal open or close: neither is a DOM insertion,
    // and <dialog>'s close event is dispatched straight at the dialog — it
    // reaches no listener on document, not even in the capture phase. Filtered
    // down to that one attribute, so page re-renders cost nothing.
    new MutationObserver(function () {
        scheduleRestore(RETRIES);
    }).observe(document.documentElement, { subtree: true, attributes: true, attributeFilter: ['open'] });

    window.addEventListener('focus', function () { scheduleRestore(RETRIES); });
    document.addEventListener('visibilitychange', function () { scheduleRestore(RETRIES); });

    return {
        /**
         * Make this element the page's caret owner while no dialog is open.
         * Returns a handle for detach() — a page being torn down must not clear
         * the field a newly opened page has already claimed.
         * @param {HTMLElement} element
         */
        attach: function (element) {
            if (!element) return 0;

            pageField = element;
            scheduleRestore(RETRIES);
            return ++handle;
        },

        detach: function (id) {
            if (id === handle) pageField = null;
        },

        /** Force an attempt now, e.g. after the field is re-enabled. */
        focus: function () { scheduleRestore(RETRIES); },

        /**
         * Let go of the caret so the browser can run its own focus handling —
         * called right before a dialog is shown. Chrome skips a dialog's
         * autofocus while the document already has a focused element, and the
         * caret would then be stranded on a field the dialog makes inert.
         */
        release: function () {
            const active = document.activeElement;
            if (!active || active === document.body || typeof active.blur !== 'function') return;

            // Our own restore must not race the dialog opening in the meantime.
            if (pending !== null) {
                clearTimeout(pending);
                pending = null;
            }
            retriesLeft = 0;
            active.blur();
        }
    };
})();
