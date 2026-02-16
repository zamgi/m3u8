const root = browser; //chrome;
window.addEventListener('load', function () {
    window.workInfo = new workInfoType();

    const requestHeaders_by_url = {};
    root.webRequest.onCompleted.addListener(async d => {
        const ext = (d.url.split('?')[ 0 ].split('.').pop() || '').toLowerCase();
        if (ext === 'm3u8') {
            const requestHeaders = requestHeaders_by_url[d.url];
            if (requestHeaders) delete requestHeaders_by_url[d.url];

            window.workInfo.addM3u8Urls(d.tabId, d.url, requestHeaders);
            await window.workInfo.setUrlsCountText(d.tabId);
        }
    }, 
    {urls: ['<all_urls>']});

    root.webRequest.onBeforeSendHeaders.addListener(d => {
        const ext = (d.url.split('?')[ 0 ].split('.').pop() || '').toLowerCase();
        if (ext === 'm3u8') {
            requestHeaders_by_url[d.url] = JSON.stringify(d.requestHeaders);
        }
    },
    {urls: ['<all_urls>']}, ['requestHeaders']);

    // set handler to tabs
    root.tabs.onActivated.addListener(async d => await window.workInfo.activateTab(d.tabId));

    // set handler to tabs
    root.tabs.onUpdated.addListener(async (tabId, info, tab) => {
        if (!info?.status || (info.status.toLowerCase() !== 'complete')) return;

        // if user open empty tab or ftp protocol and etc.
        if ((tabId === undefined) || !tab?.url || (!tab.url.startsWith('http://') && !tab.url.startsWith('https://'))) {
            if (tabId !== undefined) await root.browserAction.disable(tabId);
        }
        else {
            const opt = await root.storage.local.get();
            if (!opt.saveUrlListBetweenTabReload) {
                window.workInfo.resetM3u8Urls(tabId);
            }

            //if (opt.enableButtonEvenIfNoUrls) {
            //    await root.browserAction.enable(tabId);
            //}
            await window.workInfo.activateTab(tabId);
        }
    });

    root.tabs.onRemoved.addListener(async tabId => await window.workInfo.deleteTab(tabId));
});

window.workInfoType = function () { };
window.workInfoType.prototype = {
    tabs: {},
    activeTabId: null,

    activateTab: async function (tabId) {
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
        await this.setUrlsCountText(tabId);
    },
    deleteTab: async function (tabId) {
        const has = !!this.tabs[tabId];
        if (has) {
            delete this.tabs[tabId];
            //await this.setUrlsCountText(tabId);

            const tabs = await root.tabs.query({ active: true, currentWindow: true });
            const activeTabId = (tabs?.length ? tabs[ 0 ].id : undefined);
            await this.setUrlsCountText(activeTabId);
        }
    },
    deleteAllUrlsFromTab: async function (tabId) { await this.deleteTab(tabId); },
    deleteUrlFromTab: async function (tabId, m3u8_url) {
        if ((tabId !== undefined) && m3u8_url) {
            const o = this.tabs[tabId];
            if (o?.m3u8_urls) {
                const i = o.m3u8_urls.indexOf(m3u8_url);
                if (i !== -1) {
                    o.m3u8_urls.splice(i, 1);
                    delete o.requestHeaders[m3u8_url];

                    await this.setUrlsCountText(tabId);
                }
            }
        }
    },

    setUrlsCountText: async function (tabId) {
        //if (tabId !== undefined) {
        //    const cnt = this.tabs[tabId]?.m3u8_urls?.length;
        //    if (tabId === this.activeTabId) {
        //        await root.browserAction.setBadgeText({ text: (cnt || '') + '' });
        //    }

        //    const opt = await root.storage.local.get();
        //    if (!opt.enableButtonEvenIfNoUrls) {
        //        if (cnt) await root.browserAction.enable(tabId);
        //        else     await root.browserAction.disable(tabId)
        //    }
        //}

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
                else     await root.browserAction.disable(tabId)
            }
        }
    },

    resetM3u8Urls: function (tabId) {
        const o = this.tabs[tabId];
        if (o) {
            o.m3u8_urls = [];
            o.requestHeaders = {};
            //await this.setUrlsCountText(tabId);
        }
    },
    addM3u8Urls: function (tabId, m3u8_url, requestHeaders) {
        if ((tabId !== undefined) && m3u8_url) {
            let o = this.tabs[tabId];
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
        }
    },
    getM3u8UrlsByActiveTabId: function (activeTabId) {
        const o = this.tabs[activeTabId] || this.tabs[this.activeTabId]
        return (o?.m3u8_urls ? { m3u8_urls: o.m3u8_urls, requestHeaders: o.requestHeaders || {} } : { m3u8_urls: [], requestHeaders: {} });
    }
};
