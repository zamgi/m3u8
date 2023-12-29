self.importScripts('workInfoType.js');

//------------------------------------------------------------------------------------------------//
/*
const KEEP_ALIVE_PORT = 'KEEP_ALIVE_PORT';
const SECONDS = 1000;

const keepAliveer = {
    alivePort: null,
    lastCall : Date.now()
};
// -------------------------------------------------------
setInterval(KeepAliveRoutine, 270 * SECONDS);
KeepAliveRoutine();
// -------------------------------------------------------
async function KeepAliveRoutine() {
    const age = Date.now() - keepAliveer.lastCall;
    const convertNoDate = (long) => new Date(long).toISOString().slice(-13, -5); // HH:MM:SS

    console.log('(DEBUG KeepAliveRoutine) ------------- time elapsed from first start: ' + convertNoDate(age));
    if (!keepAliveer.alivePort) {
        keepAliveer.alivePort = chrome.runtime.connect({ name: KEEP_ALIVE_PORT });

        keepAliveer.alivePort.onDisconnect.addListener(_ => {
            if (chrome.runtime.lastError) {
                console.log('(DEBUG KeepAliveRoutine) Expected disconnect (on error: "' + chrome.runtime.lastError.message + '"). SW should be still running.');
            } else {
                console.log('(DEBUG KeepAliveRoutine): port disconnected.');
            }
            keepAliveer.alivePort = null;
        });
    }
    if (keepAliveer.alivePort) {
        keepAliveer.alivePort.postMessage({ content: 'keep-alive' });
        if (chrome.runtime.lastError) {
            console.log('(DEBUG KeepAliveRoutine): postMessage error: "' + chrome.runtime.lastError.message + '".');
        } else {
            console.log('(DEBUG KeepAliveRoutine): "ping" sent through "' + keepAliveer.alivePort.name + '" port.');
        }
    }
}
*/

//------------------------------------------------------------------------------------------------//

//urls-list will be saved between reloads.
(async () => {
    let res = await chrome.storage.local.get();
    if (!res.saveUrlListBetweenTabReload) {
        await get_workInfo().clear();
    } else {
        await get_workInfo(res.workInfo).removeEmptyTabs();
    }
})();

chrome.webRequest.onCompleted.addListener(async function (d/*details*/) {
    let ext = (d.url.split('?')[0].split('.').pop() || '').toLowerCase();
    if (ext === 'm3u8') {
        //---console.log('addM3u8Urls() => tabId: ' + d.tabId + ', url: ' + d.url );

        await get_workInfo().addM3u8Urls(d.tabId, d.url);
    }
    //else console.log('discarded => tabId: ' + d.tabId + ', url: ' + d.url );
}, {
    urls: ['<all_urls>']
});

// set handler to tabs
chrome.tabs.onActivated.addListener(async (info) => await get_workInfo().onActivateTab(info.tabId));

chrome.tabs.onRemoved.addListener(async (tabId) => await get_workInfo().onRemoveTab(tabId));

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

