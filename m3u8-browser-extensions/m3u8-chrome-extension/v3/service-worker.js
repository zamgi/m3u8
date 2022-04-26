self.importScripts('workInfoType.js');

console.log('start \'service-worker.js\': ' + new Date().toLocaleString());

//urls-list will be saved between reloads.
(async () => {
    let res = await chrome.storage.local.get();
//---console.log(JSON.stringify(res));
    if (!res.saveUrlListBetweenTabReload) {
        await chrome.storage.local.set({ workInfo: new workInfoType() });
    }
})();
//---(async () => { await chrome.storage.local.set({ workInfo: new workInfoType() }); })();

chrome.webRequest.onCompleted.addListener(async function (d/*details*/) {
    var ext_1 = d.url.split('?')[0].split('.').pop(),
        ext_2 = (ext_1 || '').toLowerCase();
    if (ext_2 !== 'm3u8') return;

//---console.log('save_m3u8_url() => tabId: ' + d.tabId + ', url: ' + d.url );

    let res = await chrome.storage.local.get('workInfo');
    let workInfo = new workInfoType(res.workInfo);
    await workInfo.save_m3u8_url(d.tabId, d.url);
    await chrome.storage.local.set({ workInfo: workInfo });
}, {
    urls: ["<all_urls>"]
});

// set handler to tabs
chrome.tabs.onActivated.addListener(async function (info) {
    let res = await chrome.storage.local.get('workInfo');
    let workInfo = new workInfoType(res.workInfo);
    await workInfo.activateTab(info.tabId);
    await chrome.storage.local.set({ workInfo: workInfo });
});

chrome.tabs.onRemoved.addListener(async function (tabId) {
    let res = await chrome.storage.local.get('workInfo');
    let workInfo = new workInfoType(res.workInfo);
    workInfo.removeTab(tabId);
    await chrome.storage.local.set({ workInfo: workInfo });
});

// set handler to tabs
chrome.tabs.onUpdated.addListener(async function (tabId, info, tab) {
    // if tab load
    if (info && info.status && (info.status.toLowerCase() === 'complete')) {
        // if user open empty tab or ftp protocol and etc.
        if ((!tabId && (tabId !== 0) ) || !tab || !tab.url || ((tab.url.indexOf('http:') === -1) && (tab.url.indexOf('https:') === -1))) {
            if (tabId || (tabId === 0)) {
                /*await*/ chrome.action.disable(tabId);
            }
            return (0);
        }

        await chrome.action.enable(tabId);

        let res = await chrome.storage.local.get();
        if (!res.saveUrlListBetweenTabReload) {
            let workInfo = new workInfoType(res.workInfo);
    //---console.log('resetTab() => tabId: ' + tabId);
            await workInfo.resetTabUrls(tabId);
            await chrome.storage.local.set({ workInfo: workInfo });
        }
    }
});

