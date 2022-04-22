self.importScripts('workInfoType.js');

chrome.storage.local.set({ workInfo: new workInfoType() });

chrome.webRequest.onCompleted.addListener(function (details) {
    var extension = details.url.split('?')[0].split('.').pop(),
        ext       = (extension || '').toLowerCase();
    if (ext !== 'm3u8') return;

    chrome.storage.local.get('workInfo', function (result) {
        let workInfo = new workInfoType(result.workInfo);

        workInfo.save_m3u8_url(details.tabId, details.url);        
        workInfo.m3u8_urls_count({ tab_id: details.tabId });

        chrome.storage.local.set({ workInfo: workInfo });
    });
}, {
    urls: ["<all_urls>"]
});

// set handler to tabs: need for seng objects
chrome.runtime.onConnect.addListener(function (port) {
    port.onMessage.addListener(function(o) {
        if (o && o.method) {
            chrome.storage.local.get('workInfo', function (result) {
                let workInfo = new workInfoType( result.workInfo );
                workInfo[o.method](o.data);
            });
        }
    });
});

// set handler to tabs
chrome.tabs.onActivated.addListener(function (info) {
    chrome.storage.local.get('workInfo', function (result) {
        let workInfo = new workInfoType( result.workInfo );
        workInfo.onActivated(info.tabId);
        chrome.storage.local.set({ workInfo: workInfo });
    });
});

chrome.tabs.onRemoved.addListener(function (id) {
    chrome.storage.local.get('workInfo', function (result) {
        let workInfo = new workInfoType( result.workInfo );
        workInfo.removeTab(id);
        chrome.storage.local.set({ workInfo: workInfo });
    });
});

// set handler to tabs
chrome.tabs.onUpdated.addListener(function (id, info, tab) {
    // if tab load
    if (info && info.status && (info.status.toLowerCase() === 'complete')) {
        // if user open empty tab or ftp protocol and etc.
        if (!id || !tab || !tab.url || ((tab.url.indexOf('http:') === -1) && (tab.url.indexOf('https:') === -1))) {
            if (id) {
                chrome.action.disable(id);
            }
            return (0);
        }

        chrome.storage.local.get('workInfo', function (result) {
            let workInfo = new workInfoType( result.workInfo );

            chrome.action.enable(id);
            workInfo.onActivated(id);            
            workInfo.addTab(tab);

            chrome.storage.local.set({ workInfo: workInfo });
        });
    }
});

