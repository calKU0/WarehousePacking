// Positions the courier dropdown in the browser's top layer.
//
// The menu lives inside cards that clip their overflow (and inside a page that
// animates with a transform), so an absolutely-positioned menu was cut off by
// the card and the viewport. Shown as a popover it renders in the top layer,
// free of any ancestor's overflow or transform; we only have to place it under
// (or above) its trigger from the trigger's viewport rectangle.
//
// The trigger toggles the popover natively via popovertarget, so a second click
// on the trigger closes it (rather than re-opening). We only listen for the
// resulting open to position it, and keep it positioned while the page scrolls.
window.courierSelect = {
    init: function (trigger, menu) {
        if (!menu || menu.__csInit) return;
        menu.__csInit = true;

        // Scrolling *inside* the menu does not move the trigger, so there is
        // nothing to re-place — and doing it anyway would fight the operator:
        // place() has to lift the height cap to measure, which drops the menu's
        // own scroll position back to the top mid-gesture.
        const reposition = (e) => {
            if (e && e.target === menu) return;
            if (menu.matches(':popover-open')) window.courierSelect.place(trigger, menu);
        };

        menu.addEventListener('toggle', function (e) {
            if (e.newState === 'open') window.courierSelect.place(trigger, menu);
        });
        window.addEventListener('resize', reposition);
        window.addEventListener('scroll', reposition, true);
    },

    place: function (trigger, menu) {
        if (!trigger || !menu) return;

        const r = trigger.getBoundingClientRect();
        const margin = 6;

        menu.style.width = r.width + 'px';
        menu.style.left = r.left + 'px';

        // Lifting the cap makes the menu briefly un-scrollable, which zeroes
        // scrollTop; put the operator back where they were afterwards.
        const scrollTop = menu.scrollTop;

        // Natural height with the cap lifted, so we can decide up vs. down.
        menu.style.maxHeight = 'none';
        const natural = menu.scrollHeight;
        const cap = Math.max(200, Math.round(window.innerHeight * 0.6));
        const spaceBelow = window.innerHeight - r.bottom - margin;
        const spaceAbove = r.top - margin;

        if (spaceBelow >= Math.min(cap, natural) || spaceBelow >= spaceAbove) {
            menu.style.top = (r.bottom + margin) + 'px';
            menu.style.maxHeight = Math.min(cap, spaceBelow) + 'px';
        } else {
            const h = Math.min(cap, spaceAbove);
            menu.style.maxHeight = h + 'px';
            menu.style.top = (r.top - h - margin) + 'px';
        }

        menu.scrollTop = scrollTop;
    }
};
