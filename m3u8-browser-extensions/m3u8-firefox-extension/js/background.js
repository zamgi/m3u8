window.onload = function () {
    window.workInfo = new workInfoType();

    let requestHeaders_by_url = {};
    chrome.webRequest.onCompleted.addListener(function (d/*details*/) {
        let ext = (d.url.split('?')[ 0 ].split('.').pop() || '').toLowerCase();
        if (ext === 'm3u8') {
            var requestHeaders = requestHeaders_by_url[d.url];
            if (requestHeaders) delete requestHeaders_by_url[d.url];
            window.workInfo.addM3u8Urls( d.tabId, d.url, requestHeaders );
            try {
                let t = window.workInfo.tabs[ d.tabId ];
                t.connectPort.postMessage({ method: 'setM3u8Urls', data: t.m3u8_urls });
            } catch (ex) {
                console.log(ex);
            }
            window.workInfo.setUrlsCountText({ tabId: d.tabId });
        }
    }, {
        urls: ["<all_urls>"]
    });

    chrome.webRequest.onBeforeSendHeaders.addListener(async function (d/*details*/) {
        let ext = (d.url.split('?')[0].split('.').pop() || '').toLowerCase();
        if (ext === 'm3u8') {
            //console.log('onBeforeSendHeaders() => tabId: ' + d.tabId + ', url: ' + d.url );

            requestHeaders_by_url[d.url] = JSON.stringify(d.requestHeaders);
        }
        //else console.log('discarded => tabId: ' + d.tabId + ', url: ' + d.url );
    }, {
        urls: ['<all_urls>']
    }, ['requestHeaders'/*, 'extraHeaders'*/]);

    // set handler to tabs
    chrome.tabs.onActivated.addListener(function (info) { window.workInfo.onActivated(info.tabId); });

    // set handler to tabs:  need for send objects
    if (chrome.extension.onConnect) {
        chrome.extension.onConnect.addListener(function (port) { port.onMessage.addListener(workInfo_methodCaller); });
    }

    // set handler to tabs
    chrome.tabs.onUpdated.addListener(function (tabId, info, tab) {
        if (!info || !info.status || (info.status.toLowerCase() !== 'complete')) return;

        // if user open empty tab or ftp protocol and etc.
        if ((tabId === undefined) || !tab || !tab.url || ((tab.url.indexOf('http:') === -1) && (tab.url.indexOf('https:') === -1))) {
            if (tabId !== undefined) chrome.browserAction.disable(tabId);
            return (0);
        }

        window.workInfo.onActivated(tabId);
        chrome.browserAction.enable(tabId);

        // save tab info if need
        window.workInfo.addTab(tab);

        // connect with new tab, and save object
        let port = chrome.tabs.connect(tabId);
        let t    = window.workInfo.tabs[tabId];
        t.connectPort = port;

        // run function in script_in_content.js
        chrome.tabs.executeScript(tabId, { code: "popupInfo_init()" });

        // send tabId, hosts and others information into script_in_content.js
        port.postMessage({ method: 'setTabId', data: tabId });
        port.postMessage({ method: 'setM3u8Urls', data: t.m3u8_urls });
        port.postMessage({ method: 'connect2Extension' });
    });

    chrome.tabs.onRemoved.addListener(function (tabId) { window.workInfo.deleteTab(tabId); });
};

function workInfo_methodCaller(obj) {
    if (obj && obj.method) {
        if (obj.data)
            window.workInfo[obj.method](obj.data);
        else
            window.workInfo[obj.method]();
    }
}

window.workInfoType = function () { };
window.workInfoType.prototype = {
    tabs: {},
    active_tabId: null,

    addTab: function (tab) {
        if (tab && (tab.id !== undefined)) {
            let o = this.tabs[tab.id];
            if (!o) {
                this.tabs[tab.id] = { tab_obj: tab };
            } else {
                o.m3u8_urls = [];
            }

            this.setUrlsCountText({ tabId: tab.id });
        }
    },
    deleteTab: function (tabId) {
        let has = !!this.tabs[tabId];
        if (has) delete this.tabs[tabId];
    },
    deleteTabUrls: async function (tabId) {
        let has = !!this.tabs[tabId];
        if (has) delete this.tabs[tabId];

        await this.setUrlsCountText({ tabId: tabId });
        //if (has) await this.save2Storage();
    },
    addM3u8Urls: function (tabId, m3u8_url, requestHeaders) {
        if ((tabId !== undefined) && m3u8_url) {
            let o = this.tabs[tabId];
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
            }
        }
    },
    setUrlsCountText: function (d) {
        let o = d ? this.tabs[d.tabId] : null;
        if (o && o.m3u8_urls && o.m3u8_urls.length) {
            chrome.browserAction.setBadgeText({ text: o.m3u8_urls.length + '' });
            return (0);
        }
        chrome.browserAction.setBadgeText({ text: '' });
    },
    onActivated: function (tabId) {
        // set active tab
        this.active_tabId = tabId;

        let d = { m3u8_urls: [] };

        if (tabId !== undefined) {
            let o = this.tabs[tabId];
            if (!o) {
                o = this.tabs[tabId] = { m3u8_urls: [] };
            }
            else if (!o.m3u8_urls) {
                o.m3u8_urls = [];
            }
            d.m3u8_urls = o.m3u8_urls;
        }
        this.setUrlsCountText({ tabId: tabId });
    },
    getM3u8Urls: function () {
        let o = (this.active_tabId !== undefined) ? this.tabs[this.active_tabId] : null;
        return ((o && o.m3u8_urls) ? { m3u8_urls: o.m3u8_urls, requestHeaders: o.requestHeaders || {} } : { m3u8_urls: [], requestHeaders: {} });
    }
};
