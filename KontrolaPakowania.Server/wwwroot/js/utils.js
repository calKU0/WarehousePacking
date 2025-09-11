window.blazorFocus = {
    focusElementRef: function (element) { element.focus(); },
    getValue: function (element) { return element.value; },
    clearValue: function (element) { element.value = ''; }
};