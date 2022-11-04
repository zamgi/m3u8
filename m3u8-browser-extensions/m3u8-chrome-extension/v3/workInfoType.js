function get_workInfo(saved_wi) { return (new workInfoType(saved_wi)); }

var workInfoType = function (saved_wi) {
    if (saved_wi && saved_wi.tabs) {
        this.tabs = _Global_Tabs = saved_wi.tabs;
    } else {
        this.tabs = _Global_Tabs;
    }
};

var _Global_Tabs = {};
workInfoType.prototype = {
    tabs: {},
    save2Storage: async function (saved_from_out_service_worker) {
        var res = { workInfo: this };
        if (saved_from_out_service_worker) res.saved_from_out_service_worker = true;
        await chrome.storage.local.set(res);
    },
    clear: async function () {
        this.tabs = _Global_Tabs = {};
        await this.save2Storage();
    },

    onActivateTab: async function (tabId) { await this.setUrlsCountText(tabId, this.tabs[tabId]); },
    onRemoveTab: async function (tabId) {
        if (this.tabs[tabId]) {
            delete this.tabs[tabId];
            await this.save2Storage();
        }
    },
    deleteTab: async function (tabId) {
        let has = !!this.tabs[tabId];
        if (has) delete this.tabs[tabId];

        await chrome.action.disable(tabId);
        await this.setUrlsCountText(tabId, null);
        if (has) await this.save2Storage();
    },
    deleteTabUrls: async function (tabId, saved_from_out_service_worker) {
        let has = !!this.tabs[tabId];
        if (has) delete this.tabs[tabId];

        await this.setUrlsCountText(tabId, null);
        if (has) await this.save2Storage(saved_from_out_service_worker);
    },
    removeEmptyTabs: async function () {
        let openTabs = (await chrome.tabs.query({ currentWindow: false })).map(tab => tab.id + ''),
            need_save = false;
        for (let tabId in this.tabs) {
            let o = this.tabs[tabId];
            if (!o || !o.m3u8_urls || !o.m3u8_urls.length || (openTabs.indexOf(tabId) === -1)) {
                delete this.tabs[tabId];
                need_save = true;
            }
        }
        if (need_save) await this.save2Storage();
    },
    addM3u8Urls: async function (tabId, m3u8_url) {
        let o = this.tabs[tabId], need_save = true;
        if (!o) {
            o = this.tabs[tabId] = { m3u8_urls: [m3u8_url] };
        }
        else if (!o.m3u8_urls) {
            o.m3u8_urls = [m3u8_url];
        }
        else if (o.m3u8_urls.indexOf(m3u8_url) === -1) {
            o.m3u8_urls.push(m3u8_url);
        } else {
            need_save = false;
        }

        await this.setUrlsCountText(tabId, o);
        if (need_save) await this.save2Storage();
    },
    setUrlsCountText: async function (tabId, o) {
        let cnt = ((o && o.m3u8_urls && o.m3u8_urls.length) ? o.m3u8_urls.length : 0);

        await chrome.action.setBadgeText({ text: (cnt ? cnt + '' : '') });
        if (cnt) {
            await chrome.action.enable(tabId);
        }
    },

    getM3u8Urls: function (tabId) {
        let o = this.tabs[tabId];
        return ((o && o.m3u8_urls) ? o.m3u8_urls : []);
    }
};
