self.importScripts('workInfoType.js');

chrome.storage.local.set({ workInfo: new workInfoType() });

chrome.webRequest.onCompleted.addListener(function (d/*details*/) {
    var ext_1 = d.url.split('?')[0].split('.').pop(),
        ext_2 = (ext_1 || '').toLowerCase();
    if (ext_2 !== 'm3u8') return;

    chrome.storage.local.get('workInfo', function (res) {
        let workInfo = new workInfoType(res.workInfo);
        workInfo.save_m3u8_url(d.tabId, d.url);        
        chrome.storage.local.set({ workInfo: workInfo });
    });
}, {
    urls: ["<all_urls>"]
});

// set handler to tabs
chrome.tabs.onActivated.addListener(function (info) {
    chrome.storage.local.get('workInfo', function (res) {
        let workInfo = new workInfoType( res.workInfo );
        workInfo.activateTab(info.tabId);
        chrome.storage.local.set({ workInfo: workInfo });
    });
});

chrome.tabs.onRemoved.addListener(function (tabId) {
    chrome.storage.local.get('workInfo', function (res) {
        let workInfo = new workInfoType( res.workInfo );
        workInfo.removeTab(tabId);
        chrome.storage.local.set({ workInfo: workInfo });
    });
});

// set handler to tabs
chrome.tabs.onUpdated.addListener(function (tabId, info, tab) {
    // if tab load
    if (info && info.status && (info.status.toLowerCase() === 'complete')) {
        // if user open empty tab or ftp protocol and etc.
        if (!tabId || !tab || !tab.url || ((tab.url.indexOf('http:') === -1) && (tab.url.indexOf('https:') === -1))) {
            if (tabId) {
                chrome.action.disable(tabId);
            }
            return (0);
        }

        chrome.storage.local.get('workInfo', function (res) {
            let workInfo = new workInfoType( res.workInfo );

            chrome.action.enable(tabId);
            workInfo.updateActiveTab(tabId);
            //---workInfo.activateTab(tabId); //urls-list will be saved between reloads.

            chrome.storage.local.set({ workInfo: workInfo });
        });
    }
});

