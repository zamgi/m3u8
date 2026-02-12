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
    const opt = await chrome.storage.local.get();
    if (!opt.saveUrlListBetweenTabReload) {
        await conv_2_workInfo().clear();
    } else {
        await conv_2_workInfo(opt.workInfo).removeEmptyTabs();
    }
})();

const requestHeaders_by_url = {};
chrome.webRequest.onCompleted.addListener(async function (d) {
    const ext = (d.url.split('?')[ 0 ].split('.').pop() || '').toLowerCase();
    if (ext === 'm3u8') {
        const requestHeaders = requestHeaders_by_url[d.url];
        if (requestHeaders) delete requestHeaders_by_url[d.url];
        await (await load_workInfo()).addM3u8Urls(d.tabId, d.url, requestHeaders);
    }
},
{ urls: ['<all_urls>'] });

chrome.webRequest.onBeforeSendHeaders.addListener(async function (d) {
    const ext = (d.url.split('?')[ 0 ].split('.').pop() || '').toLowerCase();
    if (ext === 'm3u8') {
        requestHeaders_by_url[d.url] = JSON.stringify(d.requestHeaders);
    }
},
{ urls: ['<all_urls>'] }, ['requestHeaders', 'extraHeaders'] );


// set handler to tabs
chrome.tabs.onActivated.addListener(async (info) => await (await load_workInfo()).onActivateTab(info.tabId));

chrome.tabs.onRemoved.addListener(async (tabId) => await (await load_workInfo()).onRemoveTab(tabId));

// set handler to tabs
chrome.tabs.onUpdated.addListener(async function (tabId, info, tab) {
    // if tab not-load
    if (!info?.status || (info.status.toLowerCase() !== 'complete')) return;

    // if user open empty tab or ftp protocol and etc.
    if (!tab?.url || ((tab.url.indexOf('http:') === -1) && (tab.url.indexOf('https:') === -1))) {
        await (await load_workInfo()).deleteTab(tabId);
    }
    else {
        const opt = await chrome.storage.local.get();
        if (!opt.saveUrlListBetweenTabReload) {
            await conv_2_workInfo(opt.workInfo).deleteTabUrls(tabId);
        }
    }    
});

