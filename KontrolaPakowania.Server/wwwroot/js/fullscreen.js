window.fullscreenHelper = {
    enter: function () {
        let el = document.documentElement;
        if (el.requestFullscreen) {
            el.requestFullscreen();
        } else if (el.webkitRequestFullscreen) { // Safari
            el.webkitRequestFullscreen();
        } else if (el.msRequestFullscreen) { // IE/Edge legacy
            el.msRequestFullscreen();
        }
    },
    exit: function () {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        } else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        }
    },
    isFullscreen: function () {
        return !!(document.fullscreenElement
            || document.webkitFullscreenElement
            || document.msFullscreenElement);
    }
};