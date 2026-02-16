self.importScripts('workInfoType.js');

//urls-list will be saved between reloads.
let _WorkInfo;
(async () => {
    const opt = await root.storage.local.get();
    if (opt.saveUrlListBetweenTabReload) {
        _WorkInfo = create_WorkInfoType(opt.workInfo);
        await _WorkInfo.removeEmptyTabs();
    } else {
        _WorkInfo = create_WorkInfoType();
    }
})();

const is_match_url = url => {
    const ext = (url.split('?')[ 0 ].split('.').pop() || '').toLowerCase();
    return (ext === 'm3u8');
};

const _RequestHeadersByUrl = {};
root.webRequest.onBeforeSendHeaders.addListener(d => {
    if (is_match_url(d.url)) {
        _RequestHeadersByUrl[d.url] = JSON.stringify(d.requestHeaders);
    }
},
{ urls: ['<all_urls>'] }, ['requestHeaders', 'extraHeaders'] );

root.webRequest.onCompleted.addListener(async d => {
    if (is_match_url(d.url)) {
        const requestHeaders = _RequestHeadersByUrl[d.url];
        if (requestHeaders) delete _RequestHeadersByUrl[d.url];

        await _WorkInfo.addM3u8Urls(d.tabId, d.url, requestHeaders);
    }
},
{ urls: ['<all_urls>'] });

root.tabs.onActivated.addListener(async d => await _WorkInfo.activateTab(d.tabId));

root.tabs.onRemoved.addListener(async tabId => await _WorkInfo.deleteTab(tabId));

root.tabs.onUpdated.addListener(async (tabId, info, tab) => {
    if (!info?.status || (info.status.toLowerCase() !== 'complete')) return;

    // if user open empty tab or ftp protocol and etc.
    if ((tabId === undefined) || !tab?.url || (!tab.url.startsWith('http://') && !tab.url.startsWith('https://'))) {
        if (tabId !== undefined) await root.browserAction.disable(tabId);
    }
    else {
        //const opt = await root.storage.local.get();
        //if (!opt.saveUrlListBetweenTabReload) {
        //    _WorkInfo.resetM3u8Urls(tabId);
        //}

        //if (opt.enableButtonEvenIfNoUrls) {
        //    await root.browserAction.enable(tabId);
        //}
        await _WorkInfo.activateTab(tabId);
    }
});

root.runtime.onMessage.addListener(create_RpcSeverRoutine(() => _WorkInfo));
