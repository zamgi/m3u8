var workInfoType = function (wi) {
    if (wi) {
        this.tabs = wi.tabs || {};
    }
};
workInfoType.prototype = {
    tabs: {},

    resetTabUrls: async function (tabId) {
        var o = this.tabs[tabId];
        if (!o) {
            o = this.tabs[tabId] = { m3u8_urls: [] };
        } else {
            o.m3u8_urls = [];
        }
        // set actual count of m3u8_urls for current tab
        await chrome.action.setBadgeText({ text: '' }); //---await this.set_actual_m3u8_urls_count_text(o);
    },
    /* Function will be called when user change active tab */
    activateTab: async function (tabId) {
        var o = this.tabs[tabId];
        if (!o) {
            o = this.tabs[tabId] = { m3u8_urls: [] };
        }
        else if (!o.m3u8_urls) {
            o.m3u8_urls = [];
        }

        // set actual count of m3u8_urls for current tab
        await this.set_actual_m3u8_urls_count_text(o);
    },
    removeTab: function (tabId) {
        if (tabId || (tabId === 0)) {
            delete this.tabs[tabId];
        }
    },
    save_m3u8_url: async function (tabId, m3u8_url) {
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

        await this.set_actual_m3u8_urls_count_text(o);
    },
    set_actual_m3u8_urls_count_text: async function (o) {
        var txt = ((o && o.m3u8_urls && o.m3u8_urls.length) ? o.m3u8_urls.length + '' : '');

        await chrome.action.setBadgeText({ text: txt });
    },

    get_m3u8_urls: function (tabId) {
//---console.log('get_m3u8_urls() => tabId: ' + tabId);

        var o = this.tabs[tabId];
        if (!o) {
            o = this.tabs[tabId] = { m3u8_urls: [] };
        }
        else if (!o.m3u8_urls) {
            o.m3u8_urls = [];
        }
        return (o.m3u8_urls);
    }
};
