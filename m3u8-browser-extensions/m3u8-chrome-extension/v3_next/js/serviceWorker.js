self.importScripts('workInfoType.js');

//urls-list will be saved between reloads.
(async () => {
    const opt = await root.storage.local.get();
    if (!opt.saveUrlListBetweenTabReload) {
        await conv_2_workInfo().clear();
    } else {
        await conv_2_workInfo(opt.workInfo).removeEmptyTabs();
    }
})();

const requestHeaders_by_url = {};
root.webRequest.onCompleted.addListener(async d => {
    const ext = (d.url.split('?')[ 0 ].split('.').pop() || '').toLowerCase();
    if (ext === 'm3u8') {
        const requestHeaders = requestHeaders_by_url[d.url];
        if (requestHeaders) delete requestHeaders_by_url[d.url];

        const workInfo = await load_workInfo();
        await workInfo.addM3u8Urls(d.tabId, d.url, requestHeaders);
        await workInfo.setUrlsCountText(d.tabId);
    }
},
{ urls: ['<all_urls>'] });

root.webRequest.onBeforeSendHeaders.addListener(d => {
    const ext = (d.url.split('?')[ 0 ].split('.').pop() || '').toLowerCase();
    if (ext === 'm3u8') {
        requestHeaders_by_url[d.url] = JSON.stringify(d.requestHeaders);
    }
},
{ urls: ['<all_urls>'] }, ['requestHeaders', 'extraHeaders'] );

root.tabs.onActivated.addListener(async d => await (await load_workInfo()).activateTab(d.tabId));

root.tabs.onRemoved.addListener(async tabId => await (await load_workInfo()).deleteTab(tabId));

// set handler to tabs
root.tabs.onUpdated.addListener(async (tabId, info, tab) => {
    if (!info?.status || (info.status.toLowerCase() !== 'complete')) return;

    // if user open empty tab or ftp protocol and etc.
    if ((tabId === undefined) || !tab?.url || (!tab.url.startsWith('http://') && !tab.url.startsWith('https://'))) {
        if (tabId !== undefined) await root.browserAction.disable(tabId);
    }
    else {
        const opt = await root.storage.local.get(),
            workInfo = new workInfoType(opt.workInfo);
        if (!opt.saveUrlListBetweenTabReload) {
            workInfo.resetM3u8Urls(tabId);
        }

        //if (opt.enableButtonEvenIfNoUrls) {
        //    await root.browserAction.enable(tabId);
        //}
        await workInfo.activateTab(tabId);
    }
});

/*
// set handler to tabs
root.tabs.onActivated.addListener(async (info) => await (await load_workInfo()).onActivateTab(info.tabId));

root.tabs.onRemoved.addListener(async (tabId) => await (await load_workInfo()).onRemoveTab(tabId));

// set handler to tabs
root.tabs.onUpdated.addListener(async function (tabId, info, tab) {
    // if tab not-load
    if (!info?.status || (info.status.toLowerCase() !== 'complete')) return;

    // if user open empty tab or ftp protocol and etc.
    if (!tab?.url || ((tab.url.indexOf('http:') === -1) && (tab.url.indexOf('https:') === -1))) {
        await (await load_workInfo()).deleteTab(tabId);
    }
    else {
        const opt = await root.storage.local.get();
        if (!opt.saveUrlListBetweenTabReload) {
            await conv_2_workInfo(opt.workInfo).deleteTabUrls(tabId);
        }
    }    
});
*/