async function get_workInfo() {
    let res = await chrome.storage.local.get('workInfo');
    return (new workInfoType(res.workInfo));
};

var workInfoType = function (wi) {
    if (wi) this.tabs = wi.tabs || {};
};
workInfoType.prototype = {
    tabs: {},
    save_2_storage: async function () { await chrome.storage.local.set({ workInfo: this }); },

    onActivateTab: async function (tabId) {
        var o = this.tabs[tabId];
        if (!o) {
            o = this.tabs[tabId] = { m3u8_urls: [] };
        }
        else if (!o.m3u8_urls) {
            o.m3u8_urls = [];
        }

        await this.set_urls_count_text(tabId, o);
        await this.save_2_storage();
    },
    onRemoveTab: async function (tabId) {
        delete this.tabs[tabId];
        await this.save_2_storage();
    },
    deleteTab: async function (tabId) {
        delete this.tabs[tabId];

        await chrome.action.disable(tabId);
        await this.set_urls_count_text(tabId, null);
        await this.save_2_storage();
    },
    deleteTabUrls: async function (tabId) {
        delete this.tabs[tabId];

        await this.set_urls_count_text(tabId, null);
        await this.save_2_storage();
    },
    removeEmptyTabs: async function () {
        let openTabs = (await chrome.tabs.query({ currentWindow: false })).map( tab => tab.id + '' );
        var has = false;
        for (var tabId in this.tabs) {
            var o = this.tabs[tabId];
            if (!o || !o.m3u8_urls || !o.m3u8_urls.length || (openTabs.indexOf(tabId) === -1)) {
                delete this.tabs[tabId];
                has = true;
            }
        }
        if (has) await this.save_2_storage();
    },
    add_m3u8_url: async function (tabId, m3u8_url) {
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

        await this.set_urls_count_text(tabId, o);
        await this.save_2_storage();
    },
    set_urls_count_text: async function (tabId, o) {
        var cnt = ((o && o.m3u8_urls && o.m3u8_urls.length) ? o.m3u8_urls.length : 0);

        await chrome.action.setBadgeText({ text: (cnt ? cnt + '' : '') });
        if (cnt) {
            await chrome.action.enable(tabId);
        }
    },

    get_m3u8_urls: function (tabId) {
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
