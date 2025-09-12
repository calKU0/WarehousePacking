window.userSession = {
    setUsername: function (username) {
        sessionStorage.setItem('username', username);
    },
    getUsername: function () {
        return sessionStorage.getItem('username');
    },
    clearUsername: function () {
        sessionStorage.removeItem('username');
    }
};