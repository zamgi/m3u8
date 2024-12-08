var conv_2_workInfo = function (saved_wi) { return (new workInfoType(saved_wi)); }
var load_workInfo = async function () {
    let res = await chrome.storage.local.get();
    return (new workInfoType(res.workInfo));
}

var workInfoType = function (saved_wi) {
    if (saved_wi && saved_wi.tabs) {
        this.tabs = saved_wi.tabs;
    }
};

workInfoType.prototype = {
    tabs: {},
    save2Storage: async function () { await chrome.storage.local.set({ workInfo: this }); },
    clear: async function () {
        this.tabs = {};
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
    deleteTabUrls: async function (tabId) {
        let has = !!this.tabs[tabId];
        if (has) delete this.tabs[tabId];

        await this.setUrlsCountText(tabId, null);
        if (has) await this.save2Storage();
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
    addM3u8Urls: async function (tabId, m3u8_url, requestHeaders) {
        let o = this.tabs[tabId], need_save = true;
        if (!o) {
            o = this.tabs[tabId] = { m3u8_urls: [m3u8_url], requestHeaders: { m3u8_url: requestHeaders } };
        }
        else if (!o.m3u8_urls) {
            o.m3u8_urls = [m3u8_url];
            if (!o.requestHeaders) o.requestHeaders = {};
            o.requestHeaders[m3u8_url] = requestHeaders;
        }
        else if (o.m3u8_urls.indexOf(m3u8_url) === -1) {
            o.m3u8_urls.push(m3u8_url);
            if (!o.requestHeaders) o.requestHeaders = {};
            o.requestHeaders[m3u8_url] = requestHeaders;
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
        return ((o && o.m3u8_urls) ? { m3u8_urls: o.m3u8_urls, requestHeaders: o.requestHeaders || {} } : { m3u8_urls: [], requestHeaders: {} });
    }
};
