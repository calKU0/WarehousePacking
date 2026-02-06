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