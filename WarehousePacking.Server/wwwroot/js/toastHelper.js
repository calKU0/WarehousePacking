window.toastHelper = {
    moveToRoot: function (element) {
        const root = document.getElementById('toast-root');
        if (root && element && element.parentElement !== root) {
            root.appendChild(element);
        }
    }
};