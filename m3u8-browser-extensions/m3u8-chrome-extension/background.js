window.onload = function () {
    // work object
    window.bg = new bgObj();

    chrome.webRequest.onCompleted.addListener(function (details) {
            var extension = details.url.split('?')[ 0 ].split('.').pop();
            if ((extension || '').toLowerCase() === 'm3u8') {
                window.bg.save_m3u8_url( details.tabId, details.url );
                try {
                    var t = window.bg.tabs[ details.tabId ];
                    t.port_info.postMessage({ method: 'set_m3u8_urls', data: t.m3u8_urls });
                } catch (ex) {
                    console.log(ex);
                }
                window.bg.m3u8_urls_count( { tab_id: details.tabId } );
            }
        }, {
            urls: ["<all_urls>"]
        });

    // set handler to tabs
    chrome.tabs.onActivated.addListener(function (info) {
        window.bg.onActivated(info);
    });

    // set handler to tabs:  need for seng objects
    chrome.extension.onConnect.addListener(function (port) {
        port.onMessage.addListener(methodCaller);
    });

    // set handler to tabs
    chrome.tabs.onUpdated.addListener(function (id, info, tab) {
        // if tab load
        if (info && info.status && (info.status.toLowerCase() === 'complete')) {
            // if user open empty tab or ftp protocol and etc.
            if (!id || !tab || !tab.url || ((tab.url.indexOf('http:') === -1) && (tab.url.indexOf('https:') === -1))) {
                if (id) {
                    chrome.browserAction.disable(id);
                }
                return (0);
            }

            chrome.browserAction.enable(id);

            // save tab info if need
            window.bg.addTab(tab);

            // connect with new tab, and save object
            var port = chrome.tabs.connect(id);
            var t = window.bg.tabs[id];
            t.port_info = port;

            // run function in script_in_content.js
            chrome.tabs.executeScript(id, { code: "initialization()" });

            // send id, hosts and others information into script_in_content.js
            port.postMessage({ method: 'setTabId', data: id });
            port.postMessage({ method: 'set_m3u8_urls', data: t.m3u8_urls });
            port.postMessage({ method: 'run' });
        }
    });

    chrome.tabs.onRemoved.addListener(function (id, info) {
        window.bg.removeTab(id);
    });
};

function methodCaller(obj) {
    if (obj && obj.method) {
        if (obj.data)
            window.bg[obj.method](obj.data);
        else
            window.bg[obj.method]();
    }
}

/* work object */
window.bgObj = function () { };

/* Public methods */
window.bgObj.prototype = {
    /* internal params */
    tabs: {},
    active_tab: {},

    /* Function add tab into $tabs object, if need */
    addTab: function (tab) {
        if (tab.id && (tab.id != 0)) {
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
    /* Function will be called from script_in_content.js */
    m3u8_urls_count: function (d) {
        var id = d.tab_id, o = (id ? this.tabs[id] : null);
        if (o && o.m3u8_urls) {
            o.m3u8_urls_count = o.m3u8_urls.length + '';

            if (o.m3u8_urls_count && o.m3u8_urls_count != '0') {
                chrome.browserAction.setBadgeText({ text: o.m3u8_urls_count });
                return (0);
            }
        }

        // show default text
        chrome.browserAction.setBadgeText({ text: '' });
    },    
    m3u8_urls_count_internal: function (d) {
        var id = d.tab_id, o = (id ? this.tabs[id] : null);
        if (o) {
            o.m3u8_urls = d.m3u8_urls;
            o.m3u8_urls_count = o.m3u8_urls.length + '';

            if (o.m3u8_urls_count && o.m3u8_urls_count != '0') {
                chrome.browserAction.setBadgeText({ text: o.m3u8_urls_count });
                return (0);
            }
        }

        // show default text
        chrome.browserAction.setBadgeText({ text: '' });
    },
    /* Function will be called when user change active tab */
    onActivated: function (info) {
        // set active tab
        this.active_tab = info;

        var d = { m3u8_urls: [] };

        if (info.tabId) {
            d.tab_id = info.tabId;
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

        // set actual count of m3u8_urls for current tab
        this.m3u8_urls_count_internal(d);
    },

    /* Function will be called from find.js and others places */
    get_m3u8_urls: function () {
        // init if need
        var o = this.tabs[this.active_tab.tabId];
        if (!o) {
            o = { m3u8_urls: [] };
            this.tabs[this.active_tab.tabId] = o;
        }
        else if (!o.m3u8_urls) {
            o.m3u8_urls = [];
        }
        return (o.m3u8_urls);
    }
};
