window.blazorFocus = {
    focusElementRef: function (element) {
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