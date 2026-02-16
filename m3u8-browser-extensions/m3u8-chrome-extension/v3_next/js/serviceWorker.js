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

const is_match_url = url => {
    const ext = (url.split('?')[ 0 ].split('.').pop() || '').toLowerCase();
    return (ext === 'm3u8');
};

const requestHeaders_by_url = {};
root.webRequest.onCompleted.addListener(async d => {
    if (is_match_url(d.url)) {
        const requestHeaders = requestHeaders_by_url[d.url];
        if (requestHeaders) delete requestHeaders_by_url[d.url];

        const workInfo = await load_workInfo();
        await workInfo.addM3u8Urls(d.tabId, d.url, requestHeaders);
        await workInfo.setUrlsCountText(d.tabId);
    }
},
{ urls: ['<all_urls>'] });

root.webRequest.onBeforeSendHeaders.addListener(d => {
    if (is_match_url(d.url)) {
        requestHeaders_by_url[d.url] = JSON.stringify(d.requestHeaders);
    }
},
{ urls: ['<all_urls>'] }, ['requestHeaders', 'extraHeaders'] );

root.tabs.onActivated.addListener(async d => await (await load_workInfo()).activateTab(d.tabId));

root.tabs.onRemoved.addListener(async tabId => await (await load_workInfo()).deleteTab(tabId));

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

//root.runtime.onMessage.addListener(async (message, sender, sendResponse) => {
//    if (message.action === 'getData') {
//        sendResponse({ data: 'Привет из Service Worker!' });
//    }
//    return (true); // Важно для асинхронного ответа
//});