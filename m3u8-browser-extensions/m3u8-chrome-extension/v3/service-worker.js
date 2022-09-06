self.importScripts('workInfoType.js');

//------------------------------------------------------------------------------------------------//
let lifeline, keepAliveNum = 0;
keepAlive();

chrome.runtime.onConnect.addListener(port => {
    if (port.name === 'keepAlive') {
        console.log('keepAlive (for don\'t sleep): ' + (++keepAliveNum));
        lifeline = port;
        setTimeout(keepAliveForced, 30 * 1000); // (295e3) 5 minutes minus 5 seconds
        port.onDisconnect.addListener(keepAliveForced);
    }
});
chrome.runtime.onSuspend.addListener(async () => {
    console.log('onSuspend-unloading');
    await keepAliveForced();
});

async function keepAliveForced() {
    try { lifeline?.disconnect(); } catch (ex) { ; }
    lifeline = null;
    await keepAlive();
}

async function keepAlive() {
    if (lifeline) return;
    for (const tab of await chrome.tabs.query({ url: '*://*/*' })) {
        try {
            await chrome.scripting.executeScript({
                target: { tabId: tab.id },
                func: () => chrome.runtime.connect({ name: 'keepAlive' }),
            });
            chrome.tabs.onUpdated.removeListener(retryOnTabUpdate);
            return;
        } catch (ex) {
            ;
        }
    }
    chrome.tabs.onUpdated.addListener(retryOnTabUpdate);
}

async function retryOnTabUpdate(tabId, info, tab) {
    if (info.url && /^(http|https?):/.test(info.url)) {
        await keepAlive();
    }
}
//------------------------------------------------------------------------------------------------//

//urls-list will be saved between reloads.
(async () => {
    let res = await chrome.storage.local.get();
    if (!res.saveUrlListBetweenTabReload) {
        await (new workInfoType()).save_2_storage();
    } else {
        let workInfo = new workInfoType(res.workInfo);
        await workInfo.removeEmptyTabs();
    }
})();

chrome.webRequest.onCompleted.addListener(async function (d/*details*/) {
    var ext_1 = d.url.split('?')[0].split('.').pop(),
        ext_2 = (ext_1 || '').toLowerCase();
    if (ext_2 !== 'm3u8') return;

//---console.log('add_m3u8_url() => tabId: ' + d.tabId + ', url: ' + d.url );

    let workInfo = await get_workInfo();
    await workInfo.add_m3u8_url(d.tabId, d.url);
}, {
    urls: ["<all_urls>"]
});

// set handler to tabs
chrome.tabs.onActivated.addListener(async function (info) {
    let workInfo = await get_workInfo();
    await workInfo.onActivateTab(info.tabId);
});

chrome.tabs.onRemoved.addListener(async function (tabId) {
    let workInfo = await get_workInfo();
    await workInfo.onRemoveTab(tabId);
});

// set handler to tabs
chrome.tabs.onUpdated.addListener(async function (tabId, info, tab) {
    // if tab not-load
    if (!info || !info.status || (info.status.toLowerCase() !== 'complete')) return;

    // if user open empty tab or ftp protocol and etc.
    if (!tab || !tab.url || ((tab.url.indexOf('http:') === -1) && (tab.url.indexOf('https:') === -1))) {
        let res = await chrome.storage.local.get();
        await (new workInfoType(res.workInfo)).deleteTab(tabId);
    }
    else {
        let res = await chrome.storage.local.get();
        if (!res.saveUrlListBetweenTabReload) {
            await (new workInfoType(res.workInfo)).deleteTabUrls(tabId);
        }
    }    
});

