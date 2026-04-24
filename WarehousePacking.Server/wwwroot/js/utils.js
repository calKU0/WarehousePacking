window.blazorFocus = {
    focusElement: function (element) {
        if (element) element.focus();
    },
    getValue: function (element) {
        return element.value;
    },
    clearValue: function (element) {
        element.value = '';
    },
    focusOnEnter: function (fromElement, toElement) {
        if (!fromElement || !toElement) return;
        fromElement.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                toElement.focus();
            }
        });
    }
};

window.downloadFile = (url, filename) => {
    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
};

window.dashboardNavigation = {
    listeners: {},

    register: function (dotNetRef) {
        const listenerId = 'dashboard-' + Date.now() + '-' + Math.random().toString(36).slice(2);

        const handler = function (event) {
            if (event.key !== 'ArrowLeft' && event.key !== 'ArrowRight') {
                return;
            }

            const target = event.target;
            const isTypingTarget = target && (
                target.tagName === 'INPUT' ||
                target.tagName === 'TEXTAREA' ||
                target.tagName === 'SELECT' ||
                target.isContentEditable
            );

            if (isTypingTarget) {
                return;
            }

            event.preventDefault();
            dotNetRef.invokeMethodAsync('HandleArrowKey', event.key);
        };

        window.addEventListener('keydown', handler);
        this.listeners[listenerId] = handler;
        return listenerId;
    },

    unregister: function (listenerId) {
        const handler = this.listeners[listenerId];
        if (!handler) {
            return;
        }

        window.removeEventListener('keydown', handler);
        delete this.listeners[listenerId];
    }
};