self.importScripts('workInfoType.js');

//------------------------------------------------------------------------------------------------//

//let lifeline, keepAliveNum = 0;
//keepAlive();

//chrome.runtime.onConnect.addListener(port => {
//    if (port.name === 'keepAlive') {
//        console.log('keepAlive (for don\'t sleep): ' + (++keepAliveNum));
//        lifeline = port;
//        setTimeout(keepAliveForced, 30 * 1000); // (295e3) 5 minutes minus 5 seconds
//        port.onDisconnect.addListener(keepAliveForced);
//    }
//});
//chrome.runtime.onSuspend.addListener(async () => {
//    console.log('onSuspend-unloading');
//    await keepAliveForced();
//});

//async function keepAliveForced() {
//    try { lifeline?.disconnect(); } catch (ex) { ; }
//    lifeline = null;
//    await keepAlive();
//}

//async function keepAlive() {
//    if (lifeline) return;
//    for (const tab of await chrome.tabs.query({ url: '*://*/*' })) {
//        try {
//            await chrome.scripting.executeScript({
//                target: { tabId: tab.id },
//                func: () => chrome.runtime.connect({ name: 'keepAlive' }),
//            });
//            chrome.tabs.onUpdated.removeListener(retryOnTabUpdate);
//            return;
//        } catch (ex) {
//            ;
//        }
//    }
//    chrome.tabs.onUpdated.addListener(retryOnTabUpdate);
//}

//async function retryOnTabUpdate(tabId, info, tab) { if (info.url && /^(http|https?):/.test(info.url)) await keepAlive(); }

//------------------------------------------------------------------------------------------------//

//urls-list will be saved between reloads.
(async () => {
    let res = await chrome.storage.local.get();
    if (!res.saveUrlListBetweenTabReload) {
        await get_workInfo().clear();
    } else {
        await get_workInfo(res.workInfo).removeEmptyTabs();
    }
    chrome.storage.local.onChanged.addListener(async function () {
        let res = await chrome.storage.local.get();
        if (res.saved_from_out_service_worker) {            
            let workInfo = get_workInfo(res.workInfo); //restore changes from 'm3u8.js'      
            await chrome.storage.local.remove('saved_from_out_service_worker');
        }        
    });
})();

chrome.webRequest.onCompleted.addListener(async function (d/*details*/) {
    let ext = (d.url.split('?')[0].split('.').pop() || '').toLowerCase();
    if (ext === 'm3u8') {
        //---console.log('addM3u8Urls() => tabId: ' + d.tabId + ', url: ' + d.url );

        await get_workInfo().addM3u8Urls(d.tabId, d.url);
    }
    //else console.log('discarded => tabId: ' + d.tabId + ', url: ' + d.url );
}, {
    urls: ["<all_urls>"]
});

// set handler to tabs
chrome.tabs.onActivated.addListener(async function (info) { await get_workInfo().onActivateTab(info.tabId); });

chrome.tabs.onRemoved.addListener(async function (tabId) { await get_workInfo().onRemoveTab(tabId); });

// set handler to tabs
chrome.tabs.onUpdated.addListener(async function (tabId, info, tab) {
    // if tab not-load
    if (!info || !info.status || (info.status.toLowerCase() !== 'complete')) return;

    // if user open empty tab or ftp protocol and etc.
    if (!tab || !tab.url || ((tab.url.indexOf('http:') === -1) && (tab.url.indexOf('https:') === -1))) {
        await get_workInfo().deleteTab(tabId);
    }
    else {
        let res = await chrome.storage.local.get();
        if (!res.saveUrlListBetweenTabReload) {
            await get_workInfo(res.workInfo).deleteTabUrls(tabId);
        }
    }    
});

