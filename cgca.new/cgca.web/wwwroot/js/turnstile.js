window.cgcaTurnstile = {
    tokens: {},
    widgetIds: {},

    render: function (containerId, siteKey) {
        var attempt = function () {
            var el = document.getElementById(containerId);
            if (!el) return;

            if (!window.turnstile) {
                setTimeout(attempt, 100);
                return;
            }

            if (window.cgcaTurnstile.widgetIds[containerId]) return;

            var widgetId = turnstile.render(el, {
                sitekey: siteKey,
                callback: function (token) {
                    window.cgcaTurnstile.tokens[containerId] = token;
                },
                'expired-callback': function () {
                    window.cgcaTurnstile.tokens[containerId] = null;
                },
                'error-callback': function () {
                    window.cgcaTurnstile.tokens[containerId] = null;
                }
            });
            window.cgcaTurnstile.widgetIds[containerId] = widgetId;
        };
        attempt();
    },

    getToken: function (containerId) {
        return window.cgcaTurnstile.tokens[containerId] || null;
    },

    reset: function (containerId) {
        window.cgcaTurnstile.tokens[containerId] = null;
        var widgetId = window.cgcaTurnstile.widgetIds[containerId];
        if (window.turnstile && widgetId) {
            turnstile.reset(widgetId);
        }
    }
};
