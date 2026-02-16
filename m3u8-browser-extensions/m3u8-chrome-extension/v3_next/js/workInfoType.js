const root = chrome;
root.browserAction = root.browserAction || root.action;

const getActiveTabId = async () => {
    const tabs = await root.tabs.query({ active: true, currentWindow: true });
    return (tabs?.length ? tabs[ 0 ].id : undefined);
};
//-----------------------------------------------------------------//

const create_RpcClient = function () {
    const getLocalWorkInfo = async () => {
        const json = await root.runtime.sendMessage({ action: 'getWorkInfo' });
        const stored = JSON.parse(json); 
        const wi = create_WorkInfoType(stored);

        console.log(`getWorkInfo => tabs.count: ${Object.keys(stored?.tabs || {}).length}`);

        return (wi);
    };
    const deleteAllUrlsFromTab = async tabId => {
        const suc = await root.runtime.sendMessage({ action: 'deleteAllUrlsFromTab', options: { tabId } });
    };
    const deleteUrlFromTab = async (tabId, m3u8_url, save2Storage) => {
        const suc = await root.runtime.sendMessage({ action: 'deleteUrlFromTab', options: { tabId, m3u8_url, save2Storage } });
    };

    const save2Storage = async () => {
        const suc = await root.runtime.sendMessage({ action: 'save2Storage' });
    };

    const getM3u8UrlsByActiveTabId = async activeTabId => {
        const tabInfo = await root.runtime.sendMessage({ action: 'getM3u8UrlsByActiveTabId', options: { activeTabId } });

        console.log(`getM3u8UrlsByActiveTabId => tabInfo.m3u8_urls.length: ${tabInfo?.m3u8_urls?.length || '/xz (unk)/'}`);

        return (tabInfo);
    };

    return { getLocalWorkInfo, deleteAllUrlsFromTab, deleteUrlFromTab, save2Storage, getM3u8UrlsByActiveTabId };
};

const create_RpcSeverRoutine = function (getWorkInfoFunc) {
    if (!getWorkInfoFunc) throw (new '!getWorkInfoFunc');
    const _getWorkInfoFunc = getWorkInfoFunc;

    const routine_async = async (message, sender, sendResponse) => {
        const op = message.options;
        switch (message.action || '') {
            case 'getWorkInfo':
                sendResponse(_getWorkInfoFunc().toJson()/*{ data: 'Send message from serviceWorker.' }*/);
                break;

            case 'deleteAllUrlsFromTab':
                await (_getWorkInfoFunc()).deleteAllUrlsFromTab(op.tabId);
                break;

            case 'deleteUrlFromTab':
                await (_getWorkInfoFunc()).deleteUrlFromTab(op.tabId, op.m3u8_url, op.save2Storage);
                break;

            case 'save2Storage':
                await (_getWorkInfoFunc()).save2Storage();
                break;

            case 'getM3u8UrlsByActiveTabId':
                const tabInfo = await (_getWorkInfoFunc()).getM3u8UrlsByActiveTabId(op.activeTabId);
                const json = JSON.stringify(tabInfo);
                sendResponse(json/*tabInfo*/);
                break;

            default:
                console.error(`unknown rpc-action: ${message.action || '[NULL]'}`)
                break;
        }
        return (true); // Important for asynchronous response
    };
    const routine = (message, sender, sendResponse) => {
        const op = message.options;
        switch (message.action || '') {
            case 'getWorkInfo':
                sendResponse(_getWorkInfoFunc().toJson());
                break;

            case 'deleteAllUrlsFromTab':
                _getWorkInfoFunc().deleteAllUrlsFromTab(op.tabId).then(() => sendResponse({ suc: true }));
                break;

            case 'deleteUrlFromTab':
                _getWorkInfoFunc().deleteUrlFromTab(op.tabId, op.m3u8_url, op.save2Storage).then(() => sendResponse({ suc: true }));
                break;

            case 'save2Storage':
                _getWorkInfoFunc().save2Storage().then(() => sendResponse({ suc: true }));
                break;

            case 'getM3u8UrlsByActiveTabId':
                const tabInfo = _getWorkInfoFunc().getM3u8UrlsByActiveTabId(op.activeTabId)
                sendResponse(tabInfo);
                break;

            default:
                console.error(`unknown rpc-action: ${message.action || '[NULL]'}`)
                break;
        }
        return (true); // Important for asynchronous response
    };    
    return (routine);
}
//-----------------------------------------------------------------//

const create_WorkInfoType = stored => {
    const _Store = {
        tabs       : stored?.tabs || {},
        activeTabId: stored?.activeTabId
    };

    const save2Storage = async () => { } ,//await root.storage.local.set({ workInfo: _Store }),
    //clear = async () => { _Store.tabs = {}; await save2Storage(); },
    toJson = () => JSON.stringify( _Store ),
    removeEmptyTabs = async () => {
        let openTabs = (await root.tabs.query({ currentWindow: false })).map(tab => tab.id + ''),
            need_save = false;
        for (let tabId in _Store.tabs) {
            let o = _Store.tabs[tabId];
            if (!o?.m3u8_urls?.length || (openTabs.indexOf(tabId) === -1)) {
                delete _Store.tabs[tabId];
                need_save = true;
            }
        }
        if (need_save) await save2Storage();
    },

    activateTab = async tabId => {
        if (_Store.activeTabId !== tabId) {
            _Store.activeTabId = tabId;

            if (tabId !== undefined) {
                let o = _Store.tabs[tabId];
                if (!o) {
                    o = _Store.tabs[tabId] = { m3u8_urls: [] };
                }
                else if (!o.m3u8_urls) {
                    o.m3u8_urls = [];
                }
            }

            await save2Storage();
        }

        await setUrlsCountText(tabId);
    },
    deleteTab = async tabId =>  {
        const has = !!_Store.tabs[tabId];
        if (has) {
            delete _Store.tabs[tabId];
            await save2Storage();

            const activeTabId = await getActiveTabId();
            await setUrlsCountText(activeTabId);
        }
    },
    deleteAllUrlsFromTab = async tabId => await deleteTab(tabId),
    deleteUrlFromTab = async (tabId, m3u8_url, execSave2Storage) => {
        if ((tabId !== undefined) && m3u8_url) {
            const o = _Store.tabs[tabId];
            if (o?.m3u8_urls) {
                const i = o.m3u8_urls.indexOf(m3u8_url);
                if (i !== -1) {
                    o.m3u8_urls.splice(i, 1);
                    delete o.requestHeaders[m3u8_url];

                    await setUrlsCountText(tabId);
                    if (execSave2Storage !== false) await save2Storage();
                }
            }
        }
    },

    setUrlsCountText = async tabId => {
        if (tabId !== undefined) {
            const opt = await root.storage.local.get();
            if (opt.enableButtonEvenIfNoUrls) {
                await root.browserAction.enable(tabId);

                if (tabId === _Store.activeTabId) {
                    const cnt = _Store.tabs[tabId]?.m3u8_urls?.length;
                    await root.browserAction.setBadgeText({ text: (cnt || '') + '' });
                }
            } else {
                const cnt = _Store.tabs[tabId]?.m3u8_urls?.length;
                if (tabId === _Store.activeTabId) {
                    await root.browserAction.setBadgeText({ text: (cnt || '') + '' });
                }

                if (cnt) await root.browserAction.enable(tabId);
                else try { await root.browserAction.disable(tabId); } catch (ex) { ; }
            }
        }
    },

    resetM3u8Urls = async tabId => {
        const o = _Store.tabs[tabId];
        if (o) {
            o.m3u8_urls = [];
            o.requestHeaders = {};
            await save2Storage();
        }
    },
    addM3u8Urls = async (tabId, m3u8_url, requestHeaders) => {
        if ((tabId !== undefined) && m3u8_url) {
            let o = _Store.tabs[tabId], need_save = true;
            if (!o) {
                o = _Store.tabs[tabId] = { m3u8_urls: [m3u8_url], requestHeaders: { [m3u8_url]: requestHeaders } };
            }
            else if (!o.m3u8_urls) {
                o.m3u8_urls = [m3u8_url];
                if (!o.requestHeaders) o.requestHeaders = {};
                o.requestHeaders[m3u8_url] = requestHeaders;
            }
            else if (o.m3u8_urls.indexOf(m3u8_url) === -1) {
                o.m3u8_urls.push(m3u8_url);
                if (!o.requestHeaders) o.requestHeaders = {};
                o.requestHeaders[m3u8_url] = requestHeaders;
            }
            else {
                need_save = false;
            }

            if (need_save) {
                await setUrlsCountText(tabId);
                await save2Storage();
            }
        }
    },
    getM3u8UrlsByActiveTabId = activeTabId => {
        const o = _Store.tabs[activeTabId] || _Store.tabs[_Store.activeTabId]
        return (o?.m3u8_urls ? { m3u8_urls: o.m3u8_urls, requestHeaders: o.requestHeaders || {} } : { m3u8_urls: [], requestHeaders: {} });
    };

    return { save2Storage, toJson, removeEmptyTabs, activateTab, deleteTab, deleteAllUrlsFromTab, deleteUrlFromTab, resetM3u8Urls, addM3u8Urls, getM3u8UrlsByActiveTabId };
};
