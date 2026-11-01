const root = browser; //chrome;

const is_match_url = url => {
    const ext = (url.split('?')[ 0 ].split('.').pop() || '').toLowerCase();
    return (ext === 'm3u8');
};
const is_match_method = method => {
    const m = (method || '').toLowerCase();
    switch (m) { case 'get': case 'post': return (true); default: console.log(`skip req-method: ${method}`); break; }
};
const is_match_url_n_method = d => is_match_url(d.url) && is_match_method(d.method);

window.addEventListener('load', function () {
    window.workInfo = new workInfoType();

    const _RequestHeadersByUrl = {};
    root.webRequest.onCompleted.addListener(async d => {
        if (is_match_url_n_method(d)) {
            const requestHeaders = _RequestHeadersByUrl[d.url];
            if (requestHeaders) delete _RequestHeadersByUrl[d.url];

            window.workInfo.addM3u8Urls(d.tabId, d.url, requestHeaders);
            await window.workInfo.setUrlsCountText(d.tabId);
        }
    }, 
    {urls: ['<all_urls>']});

    root.webRequest.onBeforeSendHeaders.addListener(d => {
        if (is_match_url_n_method(d)) {
            _RequestHeadersByUrl[d.url] = JSON.stringify(d.requestHeaders);
        }
    },
    {urls: ['<all_urls>']}, ['requestHeaders']);

    // set handler to tabs
    root.tabs.onActivated.addListener(async d => await window.workInfo.activateTab(d.tabId));

    // set handler to tabs
    root.tabs.onUpdated.addListener(async (tabId, info, tab) => {
        if (!info?.status || (info.status.toLowerCase() !== 'complete')) return;

        if ((tabId === undefined) || !tab?.url || (!tab.url.startsWith('http://') && !tab.url.startsWith('https://'))) {
            if (tabId !== undefined) await root.browserAction.disable(tabId);
        }
        else {
            const opt = await root.storage.local.get();
            if (!opt.saveUrlListBetweenTabReload) {
                window.workInfo.resetM3u8Urls(tabId);
            }

            // Обновляем badge ТОЛЬКО если это активная вкладка
            const activeTabs = await root.tabs.query({ active: true, currentWindow: true });
            if (activeTabs?.[0]?.id === tabId) {
                await window.workInfo.activateTab(tabId);
            } else {
                // Для неактивной — просто обновляем badge без смены activeTabId
                await window.workInfo.setUrlsCountText(tabId);
            }
        }
    });

    root.tabs.onRemoved.addListener(async tabId => await window.workInfo.deleteTab(tabId));
});
//---------------------------------------------------------------------------//

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
        if (tabId === undefined) return;

        const cnt = this.tabs[tabId]?.m3u8_urls?.length ?? 0;
        const text = cnt ? String(cnt) : '';

        // Устанавливаем badge ДЛЯ КОНКРЕТНОЙ вкладки
        await root.browserAction.setBadgeText({ text: text, tabId: tabId });

        const opt = await root.storage.local.get();
        if (opt.enableButtonEvenIfNoUrls || (0 < cnt)) {
            await root.browserAction.enable(tabId);
        } else {
            try { await root.browserAction.disable(tabId); } catch (e) { }
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
