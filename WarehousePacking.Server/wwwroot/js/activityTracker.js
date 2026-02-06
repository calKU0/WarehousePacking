window.activityTracker = {
    lastActivity: Date.now(),
    resetTimer: function () {
        this.lastActivity = Date.now();
    },
    getLastActivity: function () {
        return this.lastActivity;
    },
    init: function () {
        ['mousemove', 'keydown', 'scroll', 'click'].forEach(event => {
            window.addEventListener(event, () => this.resetTimer());
        });
    }
};

window.activityTracker.init();