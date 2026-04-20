window.monitorAutoScroll = {
    instances: {},

    register: function (rootElement, selector) {
        const id = 'monitor-scroll-' + Date.now() + '-' + Math.random().toString(36).slice(2);
        const state = {
            root: rootElement,
            selector: selector || '.monitor-auto-scroll',
            containers: [],
            elementStates: new WeakMap(),
            speedPxPerSecond: 35,
            pauseMs: 900,
            tickHandle: null,
            refreshHandle: null,
            observer: null,
            lastFrame: 0,
            refreshPending: false
        };

        const refreshContainers = () => {
            if (state.refreshPending) return;
            state.refreshPending = true;

            window.requestAnimationFrame(() => {
                state.refreshPending = false;

                if (!state.root || !state.root.querySelectorAll) {
                    state.containers = [];
                    return;
                }

                state.containers = Array.from(state.root.querySelectorAll(state.selector)).filter(x => !!x);

                for (const el of state.containers) {
                    if (el.parentElement && el.parentElement.clientHeight > 0) {
                        const newMaxHeight = el.parentElement.clientHeight + 'px';
                        if (el.style.maxHeight !== newMaxHeight) {
                            el.style.maxHeight = newMaxHeight;
                        }
                    }

                    if (el.style.overflowY !== 'auto') {
                        el.style.overflowY = 'auto';
                    }

                    // Fix headers to top
                    const theads = el.querySelectorAll('thead th, thead td');
                    for (let i = 0; i < theads.length; i++) {
                        if (theads[i].style.position !== 'sticky') {
                            theads[i].style.position = 'sticky';
                            theads[i].style.top = '0';
                            theads[i].style.zIndex = '2';
                        }
                    }

                    if (!state.elementStates.get(el)) {
                        state.elementStates.set(el, { direction: 1, pauseUntil: Date.now() + state.pauseMs, currentScroll: el.scrollTop });
                    }
                }
            });
        };

        const tick = (timestamp) => {
            if (!state.lastFrame) {
                state.lastFrame = timestamp;
            }

            const deltaMs = Math.max(0, timestamp - state.lastFrame);
            state.lastFrame = timestamp;
            const step = (state.speedPxPerSecond * deltaMs) / 1000;
            const now = Date.now();

            for (const el of state.containers) {
                if (!el || el.clientHeight <= 0) {
                    continue;
                }

                const maxScroll = Math.max(0, el.scrollHeight - el.clientHeight);
                if (maxScroll <= 1) {
                    el.scrollTop = 0;
                    state.elementStates.delete(el);
                    continue;
                }

                let item = state.elementStates.get(el);
                if (!item) {
                    item = { direction: 1, pauseUntil: now + state.pauseMs, currentScroll: el.scrollTop };
                    state.elementStates.set(el, item);
                }

                if (now < item.pauseUntil) {
                    item.currentScroll = el.scrollTop;
                    continue;
                }

                if (Math.abs(el.scrollTop - item.currentScroll) > 2) {
                    item.currentScroll = el.scrollTop;
                }

                const next = item.currentScroll + item.direction * step;
                if (next <= 0) {
                    item.currentScroll = 0;
                    el.scrollTop = 0;
                    item.direction = 1;
                    item.pauseUntil = now + state.pauseMs;
                    continue;
                }

                if (next >= maxScroll) {
                    item.currentScroll = maxScroll;
                    el.scrollTop = maxScroll;
                    item.direction = -1;
                    item.pauseUntil = now + state.pauseMs;
                    continue;
                }

                item.currentScroll = next;
                el.scrollTop = next;
            }

            state.tickHandle = window.requestAnimationFrame(tick);
        };

        refreshContainers();
        if (state.root && window.MutationObserver) {
            state.observer = new MutationObserver(refreshContainers);
            state.observer.observe(state.root, { childList: true, subtree: true });
        }

        state.tickHandle = window.requestAnimationFrame(tick);
        state.refreshHandle = window.setInterval(refreshContainers, 1200);

        this.instances[id] = state;
        return id;
    },

    unregister: function (id) {
        const state = this.instances[id];
        if (!state) {
            return;
        }

        if (state.tickHandle) {
            window.cancelAnimationFrame(state.tickHandle);
        }

        if (state.refreshHandle) {
            window.clearInterval(state.refreshHandle);
        }

        if (state.observer) {
            state.observer.disconnect();
        }

        delete this.instances[id];
    }
};

window.monitorLayout = {
    enter: function () {
        document.body.classList.add('monitor-mode');
    },
    exit: function () {
        document.body.classList.remove('monitor-mode');
    }
};