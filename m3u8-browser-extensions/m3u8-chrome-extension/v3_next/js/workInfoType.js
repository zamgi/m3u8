const root = chrome;
root.browserAction = root.browserAction || root.action;

const conv_2_workInfo = saved_wi => new workInfoType(saved_wi);
const load_workInfo = async () => {
    const opt = await root.storage.local.get();
    return (new workInfoType(opt.workInfo));
}

const workInfoType = function (saved_wi) {
    if (saved_wi) {
        if (saved_wi.tabs) this.tabs = saved_wi.tabs;
        if (saved_wi.activeTabId) this.activeTabId = saved_wi.activeTabId;
    }
};

const getActiveTabId = async () => {
    // Query for the active tab in the current window
    const tabs = await root.tabs.query({ active: true, currentWindow: true });
    return (tabs?.length ? tabs[ 0 ].id : undefined);
};

workInfoType.prototype = {
    tabs: {},
    activeTabId: null,

    save2Storage: async function () { await root.storage.local.set({ workInfo: this }); },
    clear: async function () {
        this.tabs = {};
        await this.save2Storage();
    },
    removeEmptyTabs: async function () {
        let openTabs = (await root.tabs.query({ currentWindow: false })).map(tab => tab.id + ''),
            need_save = false;
        for (let tabId in this.tabs) {
            let o = this.tabs[tabId];
            if (!o?.m3u8_urls?.length || (openTabs.indexOf(tabId) === -1)) {
                delete this.tabs[tabId];
                need_save = true;
            }
        }
        if (need_save) await this.save2Storage();
    },

    activateTab: async function (tabId) {
        if (this.activeTabId !== tabId) {
            this.activeTabId = tabId;

            if (tabId !== undefined) {
                let o = this.tabs[tabId];
                if (!o) {
                    o = this.tabs[tabId] = { m3u8_urls: [] };
                }
                else if (!o.m3u8_urls) {
                    o.m3u8_urls = [];
                }
            }

            await this.save2Storage();
        }

        await this.setUrlsCountText(tabId);
    },
    deleteTab: async function (tabId) {
        const has = !!this.tabs[tabId];
        if (has) {
            delete this.tabs[tabId];
            await this.save2Storage();

            const activeTabId = await getActiveTabId();
            await this.setUrlsCountText(activeTabId);
        }
    },
    deleteAllUrlsFromTab: async function (tabId) { await this.deleteTab(tabId); },
    deleteUrlFromTab: async function (tabId, m3u8_url, save2Storage) {
        if ((tabId !== undefined) && m3u8_url) {
            const o = this.tabs[tabId];
            if (o?.m3u8_urls) {
                const i = o.m3u8_urls.indexOf(m3u8_url);
                if (i !== -1) {
                    o.m3u8_urls.splice(i, 1);
                    delete o.requestHeaders[m3u8_url];

                    await this.setUrlsCountText(tabId);
                    if (save2Storage !== false) await this.save2Storage();
                }
            }
        }
    },

    setUrlsCountText: async function (tabId) {
        if (tabId !== undefined) {
            const opt = await root.storage.local.get();
            if (opt.enableButtonEvenIfNoUrls) {
                await root.browserAction.enable(tabId);

                if (tabId === this.activeTabId) {
                    const cnt = this.tabs[tabId]?.m3u8_urls?.length;
                    await root.browserAction.setBadgeText({ text: (cnt || '') + '' });
                }
            } else {
                const cnt = this.tabs[tabId]?.m3u8_urls?.length;
                if (tabId === this.activeTabId) {
                    await root.browserAction.setBadgeText({ text: (cnt || '') + '' });
                }

                if (cnt) await root.browserAction.enable(tabId);
                else try { await root.browserAction.disable(tabId); } catch (ex) { ; }
            }
        }
    },

    resetM3u8Urls: async function (tabId) {
        const o = this.tabs[tabId];
        if (o) {
            o.m3u8_urls = [];
            o.requestHeaders = {};
            //await this.setUrlsCountText(tabId);
            await this.save2Storage();
        }
    },
    addM3u8Urls: async function (tabId, m3u8_url, requestHeaders) {
        if ((tabId !== undefined) && m3u8_url) {
            let o = this.tabs[tabId], need_save = true;
            if (!o) {
                o = this.tabs[tabId] = { m3u8_urls: [m3u8_url], requestHeaders: { [m3u8_url]: requestHeaders } };
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
            }
            else {
                need_save = false;
            }

            if (need_save) await this.save2Storage();
        }
    },
    getM3u8UrlsByActiveTabId: function (activeTabId) {
        const o = this.tabs[activeTabId] || this.tabs[this.activeTabId]
        return (o?.m3u8_urls ? { m3u8_urls: o.m3u8_urls, requestHeaders: o.requestHeaders || {} } : { m3u8_urls: [], requestHeaders: {} });
    }
};
