/* work object */
var workInfoType = function (wi) {
    if (wi) {
        this.tabs = wi.tabs || {};
        this.active_tabId = wi.active_tabId || -1;
    }
};
workInfoType.prototype = {
    tabs: {},
    active_tabId: -1,

    updateActiveTab: function (tabId) {
        this.active_tabId = tabId;

        var o = this.tabs[tabId];
        if (!o) {
            o = this.tabs[tabId] = { m3u8_urls: [] };
        } else {
            o.m3u8_urls = [];
        }
        // set actual count of m3u8_urls for current tab
        this.set_actual_m3u8_urls_count_text(o);
    },
    /* Function will be called when user change active tab */
    activateTab: function (tabId) {
        this.active_tabId = tabId;

        var o = this.tabs[tabId];
        if (!o) {
            o = this.tabs[tabId] = { m3u8_urls: [] };
        }
        else if (!o.m3u8_urls) {
            o.m3u8_urls = [];
        }

        // set actual count of m3u8_urls for current tab
        this.set_actual_m3u8_urls_count_text(o);
    },
    removeTab: function (tabId) {
        if (tabId) {
            delete this.tabs[tabId];
        }
    },
    save_m3u8_url: function (tabId, m3u8_url) {
        var o = this.tabs[tabId];
        if (!o) {
            o = this.tabs[tabId] = { m3u8_urls: [] };
        }
        else if (!o.m3u8_urls) {
            o.m3u8_urls = [];
        }

        if (m3u8_url && o.m3u8_urls.indexOf(m3u8_url) === -1) {
            o.m3u8_urls.push(m3u8_url);
        }

        this.set_actual_m3u8_urls_count_text(o);
    },
    set_actual_m3u8_urls_count_text: function (o) {
        var txt = ((o && o.m3u8_urls && o.m3u8_urls.length) ? o.m3u8_urls.length + '' : '');

        chrome.action.setBadgeText({ text: txt });
    },

    get_m3u8_urls: function () {
        var o = this.tabs[this.active_tabId];
        if (!o) {
            o = this.tabs[this.active_tabId] = { m3u8_urls: [] };
        }
        else if (!o.m3u8_urls) {
            o.m3u8_urls = [];
        }
        return (o.m3u8_urls);
    }
};
