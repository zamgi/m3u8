self.importScripts('workInfoType.js');

//entry-piont
let _WorkInfo, _Opt = {};
const initPromise = (async () => {
    _Opt = await root.storage.local.get();

    root.storage.local.onChanged.addListener(d => {
        for (let p in d) _Opt[p] = d[p]?.newValue;

        console.log(`root.storage.local.onChanged: ${JSON.stringify(d || {})}`);
    });

    if (_Opt.SaveUrlListBetweenTabReload) {
        _WorkInfo = create_WorkInfoType(_Opt.workInfo);
        await _WorkInfo.removeEmptyTabs();
    } else {
        _WorkInfo = create_WorkInfoType();
    }
})();

const is_match_url = url => {
    const ext = (url.split('?')[ 0 ].split('.').pop() || '').toLowerCase();
    return (ext === 'm3u8');
};
const is_match_method = method => {
    const m = (method || '').toLowerCase();
    //return (m === 'get');
    switch (m) { case 'get': case 'post': return (true); default: console.log(`skip req-method: ${method}`); break; }
};
const is_match_url_n_method = d => is_match_url(d.url) && is_match_method(d.method);

const _RequestHeadersByUrl = {};
root.webRequest.onBeforeSendHeaders.addListener(d => {
    if (is_match_url_n_method(d)) {
        _RequestHeadersByUrl[d.url] = JSON.stringify(d.requestHeaders);
    }
},
{ urls: ['<all_urls>'] }, ['requestHeaders', 'extraHeaders'] );

root.webRequest.onCompleted.addListener(async d => {
    if (is_match_url_n_method(d)) {
        await initPromise;

        const requestHeaders = _RequestHeadersByUrl[d.url];
        if (requestHeaders) delete _RequestHeadersByUrl[d.url];

        await _WorkInfo.addM3u8Urls(d.tabId, d.url, requestHeaders);
    }
},
{ urls: ['<all_urls>'] });

root.tabs.onActivated.addListener(async d => {
    await initPromise;
    await _WorkInfo.activateTab(d.tabId);
});

root.tabs.onRemoved.addListener(async tabId => {
    await initPromise;
    await _WorkInfo.deleteTab(tabId);
});

root.tabs.onUpdated.addListener(async (tabId, info, tab) => {
    if ((tabId === undefined) || !info?.status || (info.status.toLowerCase() !== 'complete')) return;

    if (!tab?.url || (!tab.url.startsWith('http://') && !tab.url.startsWith('https://'))) {
        await root.browserAction.disable(tabId);
    } else {
        await root.browserAction.enable(tabId);

        await initPromise;
        // Обновляем badge ТОЛЬКО если это активная вкладка
        const activeTabs = await root.tabs.query({ active: true, currentWindow: true });
        if (activeTabs?.[0]?.id === tabId) {
            await _WorkInfo.activateTab(tabId);
        } else {
            // Для неактивной — просто обновляем badge без смены activeTabId
            await _WorkInfo.setUrlsCountText(tabId);
        }
    }
});

root.webNavigation.onCompleted.addListener(async d => {
    if (d.frameId === 0) {
        if (!_Opt.saveUrlListBetweenTabReload) {
            await initPromise;
            await _WorkInfo.resetM3u8Urls(d.tabId);
        }

        // frameId === 0 - is always the main page, not the iframe
        console.log('Main page has fully loaded.');
    }
});

root.runtime.onMessage.addListener(create_RpcSeverRoutine(() => _WorkInfo));
