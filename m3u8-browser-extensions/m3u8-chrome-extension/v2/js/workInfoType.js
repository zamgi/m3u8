var workInfoType = function (bg) {
    if (bg) {
        this.tabs         = bg.tabs || {};
        this.active_tabId = bg.active_tabId || -1;
    }
};
workInfoType.prototype = {
    tabs: {},
    active_tabId: -1,

    addTab: function (tab) {
        if (tab.id) {
            var o = this.tabs[tab.id];
            if (!o) {
                this.tabs[tab.id] = { tab_obj: tab };
            } else {
                o.m3u8_urls = [];
            }

            this.m3u8_urls_count({ tab_id: tab.id });
        }
    },
    removeTab: function (tabId) {
        if (tabId) {
            delete this.tabs[tabId];
        }
    },
    save_m3u8_url: function (tabId, m3u8_url) {
        if (tabId && m3u8_url) {
            var o = this.tabs[tabId];
            if (!o) {
                o = { m3u8_urls: [] };
                this.tabs[tabId] = o;
            }
            else if (!o.m3u8_urls) {
                o.m3u8_urls = [];
            }

            if (o.m3u8_urls.indexOf(m3u8_url) === -1) {
                o.m3u8_urls.push(m3u8_url);
            }
        }
    },
    m3u8_urls_count: function (d) {
        var o = this.tabs[d.tab_id];
        if (o && o.m3u8_urls && o.m3u8_urls.length) {
            chrome.action.setBadgeText({ text: o.m3u8_urls.length + '' });
            return (0);
        }
        chrome.action.setBadgeText({ text: '' });
    },
    onActivated: function (tabId) {
        // set active tab
        this.active_tabId = tabId;

        var d = { m3u8_urls: [] };

        if (tabId) {
            d.tab_id = tabId;
            var o = this.tabs[d.tab_id];
            if (!o) {
                o = { m3u8_urls: [] };
                this.tabs[d.tab_id] = o;
            }
            else if (!o.m3u8_urls) {
                o.m3u8_urls = [];
            }
            d.m3u8_urls = o.m3u8_urls;
        }
        this.m3u8_urls_count(d);
    },
    get_m3u8_urls: function () {
        var o = this.tabs[this.active_tabId];
        if (!o) {
            o = { m3u8_urls: [] };
            this.tabs[this.active_tabId] = o;
        }
        else if (!o.m3u8_urls) {
            o.m3u8_urls = [];
        }
        return (o.m3u8_urls);
    }
};
//----------------------------------------------------------//