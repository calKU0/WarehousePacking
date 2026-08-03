// Makes double tap work like double click on the packing stations.
//
// The packing lists open a row with @ondblclick, which relies on the browser
// turning two taps into a dblclick event. On a touch panel that is unreliable:
// the two taps have to be recognised as one gesture on one element, and any
// re-render, scroll or focus change in between splits them into two separate
// clicks — leaving the operator tapping a row that never opens.
//
// So the gesture is recognised here instead: two taps, same spot, same element,
// within the double-tap window, and no dblclick from the browser itself — then
// one is dispatched. Mouse input is left completely alone; the browser is
// reliable there and this must not double-fire an operator's click.
(function () {
    const MAX_GAP_MS = 400;   // between the two taps
    const MAX_MOVE_PX = 40;   // finger wobble between taps
    const SETTLE_MS = 40;     // wait for a native dblclick before stepping in

    let previous = null;
    let lastNativeDblClick = 0;

    // Compatibility clicks replayed from a touch carry this; a mouse click says
    // false and a programmatic .click() carries nothing at all, so both are left
    // alone — this must never turn one operator click into two.
    function isTouchClick(e) {
        return !!(e.sourceCapabilities && e.sourceCapabilities.firesTouchEvents);
    }

    // The element both taps landed on — the second tap can hit a child (an icon
    // inside a cell, say) without meaning a different target.
    function sharedTarget(first, second) {
        if (first === second) return second;
        if (first.contains && first.contains(second)) return first;
        if (second.contains && second.contains(first)) return second;
        return null;
    }

    document.addEventListener('dblclick', function () {
        lastNativeDblClick = Date.now();
    }, true);

    document.addEventListener('click', function (e) {
        if (!isTouchClick(e)) {
            previous = null;
            return;
        }

        const now = Date.now();
        const first = previous;
        previous = { target: e.target, time: now, x: e.clientX, y: e.clientY };

        if (!first) return;
        if (now - first.time > MAX_GAP_MS) return;
        if (Math.abs(e.clientX - first.x) > MAX_MOVE_PX) return;
        if (Math.abs(e.clientY - first.y) > MAX_MOVE_PX) return;

        const target = sharedTarget(first.target, e.target);
        if (!target) return;

        // Consume both taps so a third one starts a fresh gesture.
        previous = null;

        setTimeout(function () {
            if (Date.now() - lastNativeDblClick < SETTLE_MS + MAX_GAP_MS) return;
            if (!target.isConnected) return;

            target.dispatchEvent(new MouseEvent('dblclick', {
                bubbles: true,
                cancelable: true,
                view: window,
                detail: 2,
                clientX: e.clientX,
                clientY: e.clientY
            }));
        }, SETTLE_MS);
    }, true);
})();
