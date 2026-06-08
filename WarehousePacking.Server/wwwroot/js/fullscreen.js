window.fullscreenHelper = {
    enter: async function () {
        try {
            let el = document.documentElement;

            if (el.requestFullscreen) {
                await el.requestFullscreen();
            } else if (el.webkitRequestFullscreen) { // Safari
                await el.webkitRequestFullscreen();
            } else if (el.msRequestFullscreen) { // IE/Edge legacy
                el.msRequestFullscreen();
            }

            console.log("Fullscreen entered");
        } catch (err) {
            console.error("Failed to enter fullscreen:", err);
        }
    },

    exit: async function () {
        try {
            if (document.exitFullscreen) {
                await document.exitFullscreen();
            } else if (document.webkitExitFullscreen) {
                await document.webkitExitFullscreen();
            } else if (document.msExitFullscreen) {
                document.msExitFullscreen();
            }

            console.log("Fullscreen exited");
        } catch (err) {
            console.error("Failed to exit fullscreen:", err);
        }
    },

    isFullscreen: function () {
        return !!(
            document.fullscreenElement ||
            document.webkitFullscreenElement ||
            document.msFullscreenElement
        );
    }
};